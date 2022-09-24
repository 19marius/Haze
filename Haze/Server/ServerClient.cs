using System.Collections.Generic;
using System.Linq.Extensions;
using System.Diagnostics;
using System.Net.Sockets;
using System.Reflection;
using Haze.Abstractions;
using Haze.Packets;
using Haze.Logging;
using System.Linq;
using System.Net;
using Haze.Keys;
using System;

namespace Haze
{
    /// <summary>
    /// Supervises recieved data from a server's client and allows new data to be sent to the client.
    /// </summary>
    public class ServerClient : RemoteUnit, IDisposable
    {
        #region Delegates

        /// <summary>
        /// Represents the method(s) that will be called when a <see cref="Packet"/> sent by a <see cref="ServerClient"/> to its remote client caused an operation to fail.
        /// </summary>
        public delegate void OperationFailedEventHandler(object sender, OperationFailedEventArgs e);

        /// <summary>
        /// Represents the method(s) that will be called when a <see cref="RemoteUnit"/> recieves data from another <see cref="RemoteUnit"/>.
        /// </summary>
        public delegate void DataRecievedEventHandler(object sender, DataRecievedEventArgs e);

        /// <summary>
        /// Represents the method(s) called when a <see cref="ServerClient"/>'s remote client gets disconnected from its server, whether invalidated or not.
        /// </summary>
        public delegate void ClientDisconnectedEventHandler(object sender, ServerClientDisconnectedEventArgs e);

        /// <summary>
        /// Represents the method(s) called when a <see cref="ServerClient"/> gets validated and is sent a <see cref="WelcomePacket"/>.
        /// </summary>
        public delegate void ClientConnectedEventHandler(object sender, ServerClientConnectedEventArgs e);

        #endregion

        #region Properties

        /// <summary>
        /// The server of this <see cref="ServerClient"/>.
        /// </summary>
        public Server Server { get; }

        /// <summary>
        /// Represents the remote client of this <see cref="ServerClient"/>.
        /// </summary>
        public TcpClient Client { get; }

        /// <summary>
        /// The <see cref="NetworkStream"/> of this <see cref="ServerClient"/> which allows sending and recieving of data to and from the remote client.
        /// </summary>
        public NetworkStream Stream { get; }

        /// <summary>
        /// The IP address of this <see cref="ServerClient"/>
        /// </summary>
        public IPAddress Address
        {
            //If disposed get the last address stored
            get => conned ? ((IPEndPoint)Client.Client.RemoteEndPoint).Address : addr;
        }

        /// <summary>
        /// Checks if the this <see cref="ServerClient"/> is connected to its server.
        /// </summary>
        public bool IsConnected
        {
            get => conned;
        }

        /// <summary>
        /// Checks if the client is valid.
        /// <para>
        /// Valid is defined as the server having sent this <see cref="ServerClient"/> a <see cref="WelcomePacket"/>.
        /// </para>
        /// </summary>
        public bool IsValid
        {
            get => valid;
        }

        /// <summary>
        /// The data this <see cref="ServerClient"/>'s remote client has sent over the lifetime of its connection.
        /// <para>
        /// The initial <see cref="ServerKey"/> object sent by the remote client to connect to the server will not be included in this list.
        /// </para>
        /// </summary>
        public List<(Packet Packet, DateTime RecievalTime)> SentData { get; } = new List<(Packet Packet, DateTime RecievalTime)>();

        /// <summary>
        /// The logger of this <see cref="ServerClient"/>.
        /// <para>
        /// Logs are written as if the server this <see cref="ServerClient"/> belongs to owns it.
        /// </para>
        /// </summary>
        public override Logger Logger { get; }

        /// <summary>
        /// The custom nickname of this <see cref="ServerClient"/>.
        /// </summary>
        public override string Name
        {
            get => name;
            set 
            {
                name = value;
                Logger.WriteLog(this, true, "name set to \"" + name + "\"", ConsoleColor.Yellow);
                Server.SendUpdates();
            }
        }

        /// <summary>
        /// Determines which commands cannot be executed.
        /// </summary>
        public override string[] DisabledCommands { get; } = Array.Empty<string>();

        /// <summary>
        /// This <see cref="ServerClient"/>'s custom tags. If the remote client uses a <see cref="ServerKey"/> with a tag to connect, that tag will be added to this property.
        /// </summary>
        public object[] Tags { get; set; }

        /// <summary>
        /// The amount of time this <see cref="ServerClient"/> was connected to the server.
        /// </summary>
        public TimeSpan ConnectionTime
        {
            get => joinTime.Elapsed; 
        }

        #endregion

        #region Events

        /// <summary>
        /// Invoked when a <see cref="Packet"/> sent to the remote client caused an operation to fail.
        /// </summary>
        public event OperationFailedEventHandler OperationFailed;

        /// <summary>
        /// Invoked when this <see cref="ServerClient"/> recieves data from the remote client.
        /// <para>
        /// This event will not invoke if the recieved packet is a <see cref="ServerKey"/>, the object used by the remote client to connect to the server.
        /// </para>
        /// </summary>
        public event DataRecievedEventHandler DataRecieved;

        /// <summary>
        /// Invoked when this <see cref="ServerClient"/> gets disconnected from its server for any reason.
        /// </summary>
        public event ClientDisconnectedEventHandler Disconnected;

        /// <summary>
        /// Invoked when this <see cref="ServerClient"/> remote client is validated.
        /// <para>
        /// Validation is defined in the IsValid property.
        /// </para>
        /// </summary>
        public event ClientConnectedEventHandler Connected;

        #endregion

        #region Fields

        IPAddress addr;
        byte[] lastData = new byte[BufferSize];
        bool valid = false, conned = true, disposed;

        internal string dcr = "";
        string name;

        internal LinkedListNode<ServerClient> queueNode;

        Stopwatch joinTime = new Stopwatch(), dataTimer = new Stopwatch();

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerClient"/> class with a parent server and a <see cref="TcpClient"/> which handles communication with the remote client.
        /// </summary>
        public ServerClient(Server server, TcpClient client)
        {
            //Assignments
            Color = ConsoleColor.Cyan;
            (Client, Server, Stream, addr, Logger) = (client,
                                                      server,
                                                      client.GetStream(),
                                                      ((IPEndPoint)client.Client.RemoteEndPoint).Address,
                                                      new Logger(this));

            //Start listening for data & start validity timer
            Stream.BeginRead(lastData, 0, lastData.Length, RecievedDataCallback, Stream);
            AwaitKeyCheck(Server.ValidationTime);
        }

        #endregion

        #region Send Method

        /// <summary>
        /// Packs and sends an object through the <see cref="NetworkStream"/> of this <see cref="ServerClient"/>'s client.
        /// </summary>
        public void Send(object obj)
        {
            Packet packet = obj.GetType() == typeof(string) ? new StringPacket((string)obj) : new Packet(obj);
            Packet.Send(Stream, packet);

            if (Server.LogSends) Logger.WriteLog(this, false, packet.GetType().Name);
        }

        /// <summary>
        /// Sends a <see cref="Packet"/> through the <see cref="NetworkStream"/> of this <see cref="ServerClient"/>'s client.
        /// </summary>
        public void Send(Packet packet)
        {
            Packet.Send(Stream, packet);
            if (Server.LogSends && !(packet is PeerPacket)) Logger.WriteLog(this, false, packet.GetType().Name);
        }

        #endregion

        /// <summary>
        /// Invalidates this <see cref="ServerClient"/>, changing the IsValid property to <see langword="false"/> and disposing it.
        /// </summary>
        public void Invalidate()
        {
            valid = false;
            Dispose();
        }

        /// <summary>
        /// Sends a <see cref="DisconnectPacket"/> to the remote client, preparing it for disconnection and disposes of this <see cref="ServerClient"/>.
        /// </summary>
        public void Disconnect(string reason)
        {
            dcr = reason;

            //If the disconnection was reqested by the server, inform the client
            if (IsValid) Send(new DisconnectPacket(reason));

            //Since Disconnect should only be called on valid clients, revalidate
            valid = true;
            Dispose();
        }

        /// <summary>
        /// Releases all unmanaged resources and disconnects this <see cref="ServerClient"/> from its server.
        /// </summary>
        public void Dispose()
        {
            //Ensure Dispose can only be called once
            if (!disposed)
            {
                //Set the address to be used in the future and change connection status
                (addr, conned) = (Address, false);

                //Dispose and remove event handlers
                Connected = null;
                DataRecieved = null;
                OperationFailed = null;
                Logger.Dispose();
                Stream.Close();
                Client.Close();

                joinTime.Stop();
                Disconnected?.Invoke(this, new ServerClientDisconnectedEventArgs(this, dcr, !IsValid));
                Disconnected = null;

                disposed = true;
            }
        }

        #region Private Methods

        /// <summary>
        /// Called when this <see cref="ServerClient"/>'s Stream recieves data from the remote client.
        /// </summary>
        void RecievedDataCallback(IAsyncResult streamResult)
        {
            object obj = null;

            //Create a timer to measure the time processing the data takes (in case of a ping packet)
            dataTimer.Restart();

            //An exception means the client has disconnected or didn't send a Packet & should be disposed
            try
            {
                //Check if packet was sent by the Packet.Send method
                object[] data = (object[])(obj = lastData.Deserialize(true));
                if (!(data[1] is PacketKey))
                {
                    Invalidate();
                    return;
                }

                Packet packet = (Packet)data[0];

                //Add the recieved data to the list and reassign the data buffer
                lastData = new byte[BufferSize];

                //Log the packet's content
                if (Server.LogRecieves) Logger.WriteLog(this, true, packet, " (" + packet.GetType().Name + ")");

                //The first packet should always be the server key
                if (SentData.Count <= 1)
                    if (packet.Unpack() is ServerKey key)
                    {
                        valid = true;
                        name = key.ClientName;
                        Tags = key.Tag is null ? null : new object[] { key.Tag };
                        Logger.WriteLog(this, true, "name set to \"" + name + "\"", ConsoleColor.Yellow);
                        joinTime.Start();

                        Connected?.Invoke(this, new ServerClientConnectedEventArgs(this));
                        Send(new WelcomePacket(new ServerUpdatePacket(Server.MaxClients, Server.CurrentClients, Server.Name, (int)Tags[0], Server.Select(x => new ServerClientAbstract(x.ID) { Name = x.Name }).ToArray())));
                    }
                    else
                    {
                        Invalidate();
                        return;
                    }

                //Check for specific packets
                else
                {
                    SentData.Add((packet, TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.Local)));
                    ConditionalHelper.Switch(value: packet.GetType(),

                                              //Always execute default action
                                              breakSwitch: false,

                                              //Invoke event
                                              defaultAction: () => DataRecieved?.Invoke(this, new DataRecievedEventArgs(packet, SentData[SentData.Count - 1].RecievalTime)),

                                              conditions: new Dictionary<Type, Action>()
                                              {
                                                  //Check for remote client name update
                                                  {
                                                      typeof(NameUpdatePacket),
                                                      () => Name = (string)packet.Unpack()
                                                  },

                                                  //Recieved ping from remote client
                                                  {
                                                      typeof(PingPacket),
                                                      () =>
                                                      {
                                                          //Change a new packet's data buffer to the determined upload speed & send back
                                                          var respone = new PingPacket(dataTimer.ElapsedMilliseconds);
                                                          Send(respone);
                                                      }
                                                  },

                                                  //Check for communication with different remote client
                                                  {
                                                      typeof(PeerPacket),
                                                      () =>
                                                      {
                                                          var pp = (PeerPacket)packet;

                                                          //Change ID of the sender accrodingly if needed
                                                          if (pp.SenderID != ID) typeof(PeerPacket).GetField("<SenderID>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(packet, ID);

                                                          //Send to desired client
                                                          if (Server.ContainsClient(pp.RecieverID))
                                                          {
                                                              Logger.WriteLog(null, true, "client " + ToString() + " sends to " + Server[pp.RecieverID].ToString(), ConsoleColor.Yellow);
                                                              Server[pp.RecieverID].Send(pp);
                                                          }
                                                          
                                                          //If client doesn't exist fail the operation
                                                          else Send(new OperationFailedPacket(packet, new KeyNotFoundException("Server contains no client with ID " + pp.RecieverID)));
                                                      }
                                                  },

                                                  //Check if an operation failed
                                                  {
                                                      typeof(OperationFailedPacket),
                                                      () => FailOperation((OperationFailedPacket)packet)
                                                  },

                                                  //Check if the client is disconnecting
                                                  {
                                                      typeof(DisconnectPacket),
                                                      () =>
                                                      {
                                                          valid = false;
                                                          string reason = ((DisconnectPacket)packet).Reason;
                                                          Disconnect(string.IsNullOrEmpty(reason) ? "remote client disconnected" : reason);
                                                          return;
                                                      }
                                                  }
                                              });
                }

                //Start listening for more data
                Stream.BeginRead(lastData, 0, lastData.Length, RecievedDataCallback, Stream);
            }
            catch (Exception e)
            {
                if (IsValid)
                {
                    //Packet validation check, for InvalidCastException
                    try
                    {
                        //Only disconnect if lastData is not a Packet
                        if (!(obj is object[] lockedPacket) || !(lockedPacket[0] is Packet packet) || !(lockedPacket[1] is PacketKey)) Disconnect("The sent packet was not formatted correctly.");
                        else
                        {
                            var fail = new OperationFailedPacket(packet, e);
                            Send(new OperationFailedPacket(packet, e));
                            FailOperation(fail);

                            Stream.BeginRead(lastData, 0, lastData.Length, RecievedDataCallback, Stream);
                        }
                    }

                    //If Send or Disconnect threw, the client disconnected
                    catch
                    {
                        //Temporarily invalidate so Disconnect doesn't throw
                        valid = false;
                        Disconnect("remote client forcibly disconnected");
                    }
                }
                else Invalidate();
            }

            dataTimer.Reset();
        }

        /// <summary>
        /// After <paramref name="time"/> milliseconds, checks if this <see cref="ServerClient"/>'s remote client sent the server key.
        /// <para>
        /// If this <see cref="ServerClient"/> does not recieve the key within the given time, this instance gets invalidated.
        /// </para>
        /// </summary>
        async void AwaitKeyCheck(int time)
        {
            await time;
            if (SentData.Count < 1) Invalidate();
        }

        /// <summary>
        /// Announces that a sent packet caused an operation to fail.
        /// </summary>
        void FailOperation(OperationFailedPacket packet)
        {
            var causingPacket = packet.GetCausingPacket();

            //Invoke & log the failed operation
            OperationFailed?.Invoke(this, new OperationFailedEventArgs(causingPacket));
            Logger.WriteLog(Server, false, packet.Reason?.Message ?? $"operation from packet of type {causingPacket.GetType().Name} failed", ConsoleColor.Red);
        }

        #endregion
    }
}