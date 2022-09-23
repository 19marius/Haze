using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq.Extensions;
using System.Net.Sockets;
using System.Diagnostics;
using System.Reflection;
using Haze.Abstractions;
using Haze.Packets;
using Haze.Logging;
using System.Net;
using Haze.Keys;
using System;

#region Warnings
 
#pragma warning disable CS4014 //Asynchronous method call in constructor
 
#endregion

namespace Haze
{
    /// <summary>
    /// Represents a remote client which can connect to a server.
    /// </summary>
    public partial class Client : RemoteUnit, IDisposable
    {
        #region Delegates

        /// <summary>
        /// Represents the method(s) that will be called when a <see cref="Packet"/> sent by a <see cref="Client"/> caused an operation to fail.
        /// </summary>
        public delegate void OperationFailedEventHandler(object sender, OperationFailedEventArgs e);

        /// <summary>
        /// Represents the method(s) that will be called when a <see cref="RemoteUnit"/> recieves data from another <see cref="RemoteUnit"/>.
        /// </summary>
        public delegate void DataRecievedEventHandler(object sender, DataRecievedEventArgs e);

        /// <summary>
        /// Represents the method(s) that will be called when a <see cref="Client"/> gets disconnected from a server, for any reason.
        /// </summary>
        public delegate void DisconnectedEventHandler(object sender, ClientDisconnectedEventArgs e);

        /// <summary>
        /// Represents the method(s) that will be called when a new <see cref="Client"/> conenctes to a server to which another <see cref="Client"/> is already connected to.
        /// </summary>
        public delegate void ClientConnectedEventHandler(object sender, ClientConnectedEventArgs e);

        /// <summary>
        /// Represents the method(s) that will be called when a <see cref="Client"/> which is in the same server as another <see cref="Client"/> gets disconnected.
        /// </summary>
        public delegate void ClientDisconnectedEventHandler(object sender, ClientDisconnectedEventArgs e);

        /// <summary>
        /// Represents the method(s) that will be called when a <see cref="Client"/> connects and is validated to a server.
        /// </summary>
        public delegate void ConnectedEventHandler(object sender, ClientConnectedEventArgs e);

        #endregion

        #region Properties

        /// <summary>
        /// Represents the server of this <see cref="Client"/>.
        /// </summary>
        public ServerAbstract Server
        {
            get => server;
        }

        /// <summary>
        /// The data this <see cref="Client"/> has recieved from the server over its lifetime.
        /// </summary>
        public List<(Packet Packet, DateTime RecievalTime)> RecievedData { get; } = new List<(Packet Packet, DateTime RecievalTime)>();

        /// <summary>
        /// The data this <see cref="Client"/> has sent to the server over its lifetime.
        /// </summary>
        public List<Packet> SentData { get; } = new List<Packet>();

        /// <summary>
        /// This <see cref="Client"/>'s logger.
        /// </summary>
        public override Logger Logger { get; }

        /// <summary>
        /// Determines if this <see cref="Client"/> will log the packets it sends.
        /// <para>
        /// If sends are logged, only the sent packet's type will be shown.
        /// </para>
        /// </summary>
        public bool LogSends { get; set; }

        /// <summary>
        /// Determines if this <see cref="Client"/> will log the packets it recieves.
        /// <para>
        /// If the recieves are logged, the contents of the recieved packet will be shown, along with the packet's type.
        /// </para>
        /// </summary>
        public bool LogRecieves { get; set; } = true;

        /// <summary>
        /// the custom nickname of this <see cref="Client"/>.
        /// </summary>
        public override string Name
        {
            get => name;
            set
            {
                name = value;
                if (stream != null) Send(new NameUpdatePacket(name));
            }
        }

        /// <summary>
        /// Determines which commands cannot be executed.
        /// </summary>
        public override string[] DisabledCommands { get; } = new string[] { };

        /// <summary>
        /// Checks if this <see cref="Client"/> is connected to the server.
        /// </summary>
        public bool IsConnected
        {
            get => connected;
        }

        /// <summary>
        /// Gets the port this <see cref="Client"/> used to connect to the server.
        /// </summary>
        public int Port
        {
            get => port;
        }

        #endregion

        #region Events

        /// <summary>
        /// Invoked when a <see cref="Packet"/> that this <see cref="Client"/> sent caused an operation to fail.
        /// <para>
        /// If the <see cref="Packet"/> was sent to another <see cref="Client"/>, the third parameter's first element will be a <see cref="ServerClientAbstract"/> object representing the <see cref="Client"/> where the operation failed, otherwise, the third parameter will be <see langword="null"/>.
        /// </para>
        /// </summary>
        public event OperationFailedEventHandler OperationFailed;

        /// <summary>
        /// Invoked when this <see cref="Client"/> recieves data from the server or another <see cref="Client"/>.
        /// <para>
        /// If another <see cref="Client"/> sent data, the third parameter's first element will be a <see cref="ServerClientAbstract"/> object representing the <see cref="Client"/> that sent it, otherwise, the third parameter will be <see langword="null"/>.
        /// </para>
        /// </summary>
        public event DataRecievedEventHandler DataRecieved;

        /// <summary>
        /// Invoked when this <see cref="Client"/> gets disconnected from the server for any reason.
        /// </summary>
        public event DisconnectedEventHandler Disconnected;

        /// <summary>
        /// Invoked when this <see cref="Client"/> is approved by the server.
        /// <para>
		/// Approval is defined as this <see cref="Client"/> having recieved a <see cref="WelcomePacket"/> from the server.
		/// </para>
		/// </summary>
        public event ConnectedEventHandler Connected;

        /// <summary>
        /// Invoked when a new <see cref="Client"/> connected to the server this <see cref="Client"/> is connected to.
        /// </summary>
        public event ClientConnectedEventHandler ClientConnected;

        /// <summary>
        /// Invoked when a new <see cref="Client"/> gets disconnected from the server this <see cref="Client"/> is connected to.
        /// </summary>
        public event ClientDisconnectedEventHandler ClientDisconnected;

        #endregion

        #region Fields

        TcpClient client;
        NetworkStream stream = null;
        ServerAbstract server;
        Server shallowServer = new Server(0, "");

        IPAddress hostAddr;
        int port;

        string name = null, dcr = null;
        bool ready, connected, disconnected;
        Stopwatch joinTime = new Stopwatch();

        byte[] lastData = new byte[BufferSize];

        #endregion  

        #region Constructors

        /// <summary>
        /// Instantiates a new instance of the <see cref="Client"/> class with the specified <see cref="IPAddress"/> and port number.
        /// </summary>
        public Client(IPAddress hostAddress, int port)
        {
            //Ensure the shallow server is not selected.
            if (GetSelectedExecuter().Equals(shallowServer)) SelectExecuter(this);

            Color = ConsoleColor.Cyan;
            (client, Logger, Throw) = (new TcpClient(), new Logger(this), false);
            InitClient(hostAddress, false, port);
        }

        /// <summary>
        /// Instantiates a new instance of the <see cref="Client"/> class with the specified port number and the caller's address.
        /// <para>
		/// If <paramref name="local"/> is true, the machine's local address is used, else the router's public address.
		/// </para>
		/// </summary>
        public Client(bool local, int port)
        {
            //Ensure the shallow server is not selected.
            if (GetSelectedExecuter().Equals(shallowServer)) SelectExecuter(this);

            Color = ConsoleColor.Cyan;
            (client, Logger, Throw) = (new TcpClient(), new Logger(this), false);
            InitClient(null, local, port);
        }

        #endregion

        /// <summary>
        /// Connects this <see cref="Client"/> to the server. This method returns when the server validates the client.
        /// </summary>
        public async Task ConnectAsync()
        {
            //Wait for InitClient to get the public IP address (if necessary)
            while (!ready) await Task.Delay(1);

            //Connect & get the stream
            client.Connect(hostAddr, port);
            stream = client.GetStream();
            server = new ServerAbstract(0, 0, "", 0, null);
            Logger.WriteLog(shallowServer, false, "connecting", ConsoleColor.Yellow);

            //Send the password as the first packet & begin listening for data
            Send(new ServerKey(Name));
            stream.BeginRead(lastData, 0, lastData.Length, RecievedDataCallback, stream);

            //Wait until validated
            while (!connected) await 1;
        }

        /// <summary>
        /// Connects this <see cref="Client"/> to the server.
        /// </summary>
        public async void Connect()
        {
            //Wait for InitClient to get the public IP address (if necessary)
            while (!ready) await Task.Delay(1);

            //Connect & get the stream
            client.Connect(hostAddr, port);
            stream = client.GetStream();
            server = new ServerAbstract(0, 0, "", 0, null);
            Logger.WriteLog(shallowServer, false, "connecting");

            //Send the password as the first packet & begin listening for data
            Send(new ServerKey(Name));
            stream.BeginRead(lastData, 0, lastData.Length, RecievedDataCallback, stream);
        }

        /// <summary>
        /// Connects this <see cref="Client"/> to the server using an identifying object. This method returns when the server validates the client.
        /// </summary>
        public async Task ConnectAsync(object tag)
        {
            //Wait for InitClient to get the public IP address (if necessary)
            while (!ready) await Task.Delay(1);

            //Connect & get the stream
            client.Connect(hostAddr, port);
            stream = client.GetStream();
            server = new ServerAbstract(0, 0, "", 0, null);
            Logger.WriteLog(shallowServer, false, "connecting", ConsoleColor.Yellow);

            //Send the password as the first packet & begin listening for data
            Send(new ServerKey(Name, tag));
            stream.BeginRead(lastData, 0, lastData.Length, RecievedDataCallback, stream);

            //Wait until validated
            while (!connected) await 1;
        }

        /// <summary>
        /// Connects this <see cref="Client"/> to the server using an identifying object.
        /// </summary>
        public async void Connect(object tag)
        {
            //Wait for InitClient to get the public IP address (if necessary)
            while (!ready) await Task.Delay(1);

            //Connect & get the stream
            client.Connect(hostAddr, port);
            stream = client.GetStream();
            server = new ServerAbstract(0, 0, "", 0, null);
            Logger.WriteLog(shallowServer, false, "connecting");

            //Send the password as the first packet & begin listening for data
            Send(new ServerKey(Name, tag));
            stream.BeginRead(lastData, 0, lastData.Length, RecievedDataCallback, stream);
        }

        /// <summary>
        /// Pings the server this <see cref="Client"/> is connected to and returns a <see cref="NetworkSpeed"/> object representing the results of the operation.
        /// </summary>
        public async Task<NetworkSpeed> GetPing()
        {
            //Declarations
            Packet response;
            Stopwatch timer = new Stopwatch();

            //Send the ping
            timer.Start();
            Send(new PingPacket());

            //Get the upload speed after sending
            var upSpeed = timer.ElapsedMilliseconds;

            //Wait for the response and return the speed
            int count = RecievedData.Count;
            while (count == RecievedData.Count || (response = RecievedData[RecievedData.Count - 1].Packet).GetType() != typeof(PingPacket)) await 1;
            timer.Stop();

            //Console.WriteLine(response.Unpack().GetType());
            return new NetworkSpeed(upSpeed, timer.ElapsedMilliseconds - upSpeed - (long)response.Unpack());

        }

        /// <summary>
        /// Disconnects this <see cref="Client"/> from the server and disposes it.
        /// </summary>
        public void Disconnect(string reason)
        {
            dcr = reason;
            Dispose();
        }

        /// <summary>
        /// Releases all resources and this disconnects this <see cref="Client"/> from the server.
        /// </summary>
        public void Dispose()
        {
            //Ensure Dispose can only be called once
            if (!disconnected)
            {
                try
                {
                    //Inform the server
                    Send(new DisconnectPacket(dcr));
                }
                catch
                {
                    dcr = "server is closed";
                }

                //Log the disconnection
                Logger.WriteLog(null, true, (string.IsNullOrEmpty(dcr) ? "disconnected, " : "disconnected (" + dcr + "), ") + "total conntection time: " + joinTime.Elapsed, ConsoleColor.Yellow);

                //Dispose the client & stream
                client?.Close();
                stream?.Close();
                Logger.Dispose();

                //Stop the timer and clear invocation list for recieving data
                joinTime.Stop();
                DataRecieved = null;

                connected = false;
                Disconnected?.Invoke(this, new ClientDisconnectedEventArgs(Server[Server.RecieverIndex], dcr, joinTime.Elapsed));

                disconnected = true;
            }
        }

        #region Send Method

        /// <summary>
        /// Packs and sends an <see cref="object"/> through the <see cref="NetworkStream"/> of this <see cref="Client"/>.
        /// </summary>
        public void Send(object obj)
        {
            if (stream is null) throw new InvalidOperationException("The client is not yet connected.");
            if (!connected && TypeHelper.GetCallingType(false) != typeof(Client)) throw new MethodAccessException("Cannot call Send until the client is validated.");

            Packet packet = obj?.GetType() == typeof(string) ? new StringPacket((string)obj) : new Packet(obj);
            Packet.Send(stream, packet);

            if (LogSends) Logger.WriteLog(shallowServer, false, packet.GetType().Name);
        }

        /// <summary>
        /// Sends a <see cref="Packet"/> through the <see cref="NetworkStream"/> of this <see cref="Client"/>.
        /// </summary>
        public void Send(Packet packet)
        {
            //Check for client connection & if a PeerPacket was sent
            if (stream is null) throw new InvalidOperationException("The client is not yet connected.");
            if (!connected && TypeHelper.GetCallingType(false) != typeof(Client)) throw new MethodAccessException("Cannot call Send until the client is validated.");

            PeerPacket peerPacket = packet as PeerPacket;

            if (packet is PeerPacket) Send(peerPacket.RecieverID, ((Packet)peerPacket.Unpack())?.Unpack());
            else Packet.Send(stream, packet);

            if (LogSends) Logger.WriteLog(packet is PeerPacket ? (RemoteUnit)CreateShallowClient(Server[peerPacket.RecieverID]) : shallowServer, false, packet is PeerPacket ? peerPacket.Unpack().GetType().Name : packet.GetType().Name);
        }

        /// <summary>
        /// Packs and sends an <see cref="object"/> through the <see cref="NetworkStream"/> of this <see cref="Client"/> to the client in the server at the specified index.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void Send(int index, object obj)
        {
            //Check for client connection & self-sending
            if (stream is null) throw new InvalidOperationException("The client is not yet connected.");
            if (!connected && TypeHelper.GetCallingType(false) != typeof(Client)) throw new MethodAccessException("Cannot call Send until the client is validated.");
            if (index == Server.RecieverIndex)
            {
                Logger.WriteLog(null, false, obj.ToString());
                return;
            }

            Packet packet = new PeerPacket(ID, Server[index].ID, obj?.GetType() == typeof(string) ? new StringPacket((string)obj) : new Packet(obj));
            Packet.Send(stream, packet);

            if (LogSends) Logger.WriteLog(CreateShallowClient(Server[index]), false, packet.Unpack().GetType().Name);
        }

        /// <summary>
        /// Packs and sends an <see cref="object"/> through the <see cref="NetworkStream"/> of this <see cref="Client"/> to the client in the server with the specified ID.
        /// </summary>
        public void Send(string id, object obj)
        {
            //Check for client connection, self-sending and argument validity
            if (stream is null) throw new InvalidOperationException("The client is not yet connected.");
            if (Server.ContainsClient(id)) throw new ArgumentException("No client with id " + id + " is connected to the server.");
            if (!connected && TypeHelper.GetCallingType(false) != typeof(Client)) throw new MethodAccessException("Cannot call Send until the client is validated.");
            if (id == ID)
            {
                Logger.WriteLog(null, false, obj.ToString());
                return;
            }

            Packet packet = new PeerPacket(ID, id, obj?.GetType() == typeof(string) ? new StringPacket((string)obj) : new Packet(obj));
            Packet.Send(stream, packet);

            if (LogSends) Logger.WriteLog(CreateShallowClient(Server[id]), false, packet.Unpack().GetType().Name);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Called when this <see cref="Client"/>'s stream recieves data from the server.
        /// </summary>
        void RecievedDataCallback(IAsyncResult streamResult)
        {
            //Default disconnection reason
            string disconnectionReason = "An operation failed while recieving data from the server";

            //An exception means the server is closed
            try
            {
                //Check if the the data was actually sent by the server
                object[] data = (object[])lastData.Deserialize(true);
                Packet packet = (Packet)data[0];

                if (data[1] is PacketKey)
                {
                    //Check if client should be disconnected
                    if ((RecievedData.Count == 0 && packet is WelcomePacket) || (RecievedData.Count >= 1 && !(packet is DisconnectPacket)))
                    {
                        //Add the recieved data to the list and reassign the data buffer
                        RecievedData.Add((packet, TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.Local)));
                        lastData = new byte[BufferSize];

                        //Check if packet is sent from peer
                        PeerPacket peerPacket = packet as PeerPacket;

                        //Log the recieved data
                        if (LogRecieves) Logger.WriteLog(packet is PeerPacket ? CreateShallowClient(Server[peerPacket.SenderID]) : (RemoteUnit)shallowServer, true, packet is PeerPacket ? (Packet)packet.Unpack() : packet, " (" + (packet is PeerPacket ? packet.Unpack().GetType().Name : packet.GetType().Name) + ")");

                        //Check for special packets
                        ConditionalHelper.Switch(value: packet.GetType(),

                                                //Always execute default action
                                                breakSwitch: false,

                                                //Invoke event
                                                defaultAction:
                                                () =>
                                                {
                                                    if (!(packet is OperationFailedPacket) && !(packet is PeerPacket && packet.Unpack() is OperationFailedPacket))
                                                        try
                                                        {
                                                            DataRecieved?.Invoke(this, new DataRecievedEventArgs(packet is PeerPacket ? (Packet)packet.Unpack() : packet, RecievedData[RecievedData.Count - 1].RecievalTime, packet is PeerPacket ? new object[] { Server[peerPacket.SenderID] } : new object[0]));
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            //Only inform if the event didnt throw on a failing packet to prevent recursiveness
                                                            if (!(packet.Unpack() is OperationFailedPacket)) Packet.Send(stream, packet is PeerPacket ? new PeerPacket(peerPacket.RecieverID, peerPacket.SenderID, new OperationFailedPacket(packet, e)) : (Packet)new OperationFailedPacket(packet, e));
                                                            FailOperation(new OperationFailedPacket(packet, e), ID);
                                                        }
                                                },

                                                conditions: new Dictionary<Type, Action>()
                                                {
                                                    //Check if client was just approved
                                                    {
                                                        typeof(WelcomePacket),
                                                        () =>
                                                        {
                                                            SetServerInfo((ServerUpdatePacket)packet.Unpack());

                                                            //Change ID according to server client
                                                            typeof(RemoteUnit).GetField("<ID>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, Server[Server.RecieverIndex].ID);
                                                            Logger.WriteLog(shallowServer, true, "changed ID to correspond to server", ConsoleColor.Yellow);

                                                            //Start timer & invoke event
                                                            connected = true;
                                                            joinTime.Start();
                                                            Connected?.Invoke(this, new ClientConnectedEventArgs(Server[Server.RecieverIndex]));
                                                            Logger.WriteLog(shallowServer, true, "successfully connected", ConsoleColor.Green);
                                                        }
                                                    },

                                                    //Check for server updates
                                                    {
                                                        typeof(ServerUpdatePacket),
                                                        () =>
                                                        {
                                                            var update = (ServerUpdatePacket)packet;
                                                            SetServerInfo(update);

                                                            //Log if any client has connected/disconnected
                                                            if ((update.NewClient ?? update.DisconnectedClient) != null)
                                                            {
                                                                Logger.WriteLog(shallowServer, true, update.NewClient != null ? "client " + (string.IsNullOrEmpty(update.NewClient.Value.Name) ? update.NewClient.Value.ID : update.NewClient.Value.Name) + " connected" :
                                                                                                                                                                                         "client " + (string.IsNullOrEmpty(update.DisconnectedClient.Value.Name) ? update.DisconnectedClient.Value.ID : update.DisconnectedClient.Value.Name) + " was disconnected" + (string.IsNullOrEmpty(update.DisconnectReason) ? "" : " (" + update.DisconnectReason + ")"), ConsoleColor.Cyan);
                                                                if (update.NewClient != null) ClientConnected?.Invoke(this, new ClientConnectedEventArgs(update.NewClient.Value));
                                                                else ClientDisconnected?.Invoke(this, new ClientDisconnectedEventArgs(update.DisconnectedClient.Value, update.DisconnectReason, update.ConnectionTime.Value));
                                                            }
                                                        }
                                                    },

                                                    //Check if an operation failed
                                                    {
                                                        typeof(OperationFailedPacket),
                                                        () => FailOperation((OperationFailedPacket)packet)
                                                    },

                                                    //Check if a remote operation failed
                                                    {
                                                        typeof(PeerPacket),
                                                        () =>
                                                        {
                                                            if (peerPacket.Unpack() is OperationFailedPacket failedPacket) FailOperation(failedPacket, peerPacket.SenderID);
                                                        }
                                                    },
                                                }
                                                );

                        //Start listening for data & return
                        stream.BeginRead(lastData, 0, lastData.Length, RecievedDataCallback, stream);
                        return;
                    }

                    disconnectionReason = ((DisconnectPacket)packet).Reason;
                }
            }
            catch (Exception e)
            {
                Disconnect("exception of type " + e.GetType() + " was thrown" + (string.IsNullOrEmpty(e.Message) ? "." : " (" + e.Message + ")"));
                return;
            }

            Disconnect(string.IsNullOrEmpty(disconnectionReason) ? "server is closed" : disconnectionReason);
        }

        /// <summary>
        /// Initializes this <see cref="Client"/> with the specified address and port number.
        /// <para>
		/// If <paramref name="addr"/> is <see langword="null"/>, then the address will be chosen in consideration to <paramref name="local"/>. If <paramref name="local"/> is true, the machine's local address is used, else the router's public address.
		/// </para>
		/// </summary>
        async Task InitClient(IPAddress addr, bool local, int port)
        {
            (hostAddr, this.port, ready) = (addr ?? (local ? IPAddressHelper.GetLocalAddress() : await IPAddressHelper.GetPublicAddressAsync()), port, true);
        }

        /// <summary>
        /// Updates this <see cref="Client"/>'s server infomation according to an update packet.
        /// </summary>
        void SetServerInfo(ServerUpdatePacket serverPacket)
        {
            server.ChangeServerInformation(serverPacket.MaxClients, serverPacket.CurrentClients, serverPacket.Name, serverPacket.RecieverIndex, serverPacket.Clients);
            shallowServer.Name = serverPacket.Name;

            //Set own name
            name = Server[Server.RecieverIndex].Name;
        }

        /// <summary>
        /// Announces that a sent packet caused an operation to fail.
        /// </summary>
        void FailOperation(OperationFailedPacket packet)
        {
            var causingPacket = packet.GetCausingPacket();

            //Invoke & log the failed operation
            OperationFailed?.Invoke(this, new OperationFailedEventArgs(causingPacket));
            Logger.WriteLog(shallowServer, true, packet.Reason?.Message + (packet.Reason is null ? "" : " (" + packet.Reason?.StackTrace + ")") ?? $"operation from packet of type {causingPacket.GetType().Name} failed", ConsoleColor.Red);
        }

        /// <summary>
        /// Announces that a packet sent to another <see cref="Client"/> caused an operation to fail.
        /// </summary>
        void FailOperation(OperationFailedPacket packet, string failingId)
        {
            var causingPacket = packet.GetCausingPacket();

            //Invoke & log the failed operation
            OperationFailed?.Invoke(this, new OperationFailedEventArgs(packet, Server[failingId]));
            Logger.WriteLog(failingId == ID ? null : CreateShallowClient(Server[failingId]), true, packet.Reason?.Message + (packet.Reason is null ? "" : " (" + packet.Reason?.StackTrace + ")") ?? $"operation from packet of type {causingPacket.GetType().Name} failed", ConsoleColor.Red);
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Creates a <see cref="Client"/> from a <see cref="ServerClientAbstract"/>.
        /// <para>
        /// The returned <see cref="Client"/> only contains the name and the ID of <paramref name="client"/>.
        /// </para>
        /// </summary>
        static Client CreateShallowClient(ServerClientAbstract client)
        {
            //Initialize shallow client
            var shallowClient = new Client(true, 0) { Name = client.Name };

            //Change the ID accordingly
            typeof(RemoteUnit).GetField("<ID>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(shallowClient, client.ID);

            return shallowClient;
        }

        #endregion
    }
}