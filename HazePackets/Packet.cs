using System.Linq.Extensions;
using System.Net.Sockets;
using Haze.Abstractions;
using Haze.Keys;
using System;

namespace Haze.Packets
{
    /// <summary>
    /// The base class for encapsulating data to be sent to and from clients.
    /// </summary>
    [Serializable]
    public class Packet
    {
        /// <summary>
        /// The time at which this <see cref="Packet"/> was sent.
        /// <para>
        /// Until sending, represents the time at which this <see cref="Packet"/> was created.
        /// </para>
        /// </summary>
        public DateTime SendTime
        {
            get => time;
        }

        /// <summary>
        /// Gets or sets an <see cref="object"/> that provides additional information about this <see cref="Packet"/>.
        /// </summary>
        public object Tag { get; set; }

        #region Fields

        /// <summary>
        /// Contains the <see cref="object"/> that was serialized when this <see cref="Packet"/> was created.
        /// </summary>
        readonly protected byte[] data;
        internal DateTime time = DateTime.Now;

        #endregion

        /// <summary>
        /// Creates a new instance of the <see cref="Packet"/> class, serializing an <see cref="object"/> into it.
        /// </summary>
        public Packet(object obj)
        {
            time = DateTime.Now;
            data = obj?.Serialize(true) ?? null;
        }

        /// <summary>
        /// Unpacks the <see cref="object"/> serialized into this instance.
        /// </summary>
        public object Unpack()
        {
            return data?.Deserialize(true);
        }

        /// <summary>
        /// Unpacks the <see cref="object"/> serialized into this instance to another <see cref="object"/>.
        /// <para>
        /// If the packed data is <see langword="null"/>, then <paramref name="obj"/> does not change.
        /// </para>
        /// </summary>
        public void UnpackTo(ref object obj)
        {
            if (data != null) obj = data.Deserialize(true);
        }

        /// <summary>
        /// Sends a <see cref="Packet"/> through a <see cref="NetworkStream"/>, updating the <see cref="Packet"/>'s send time.
        /// <para>
        /// This method can only be used by inheritors of <see cref="RemoteUnit"/>.
        /// </para>
        /// </summary>
        public static void Send(NetworkStream stream, Packet packet)
        {
            if (TypeHelper.GetCallingType(2).BaseType != typeof(RemoteUnit)) throw new MethodAccessException("Packet.Send method was not called by a RemoteUnit.");
            if (packet is null) throw new ArgumentNullException("packet");

            packet.time = DateTime.UtcNow;
            var bytes = new object[] { packet, new PacketKey() }.Serialize(true);
            stream.Write(bytes, 0, bytes.Length);
        }
    }

    /// <summary>
    /// Encapsulates a <see cref="string"/> object.
    /// </summary>
    [Serializable]
    public class StringPacket : Packet
    {
        /// <summary>
        /// Creates a new instance of the <see cref="StringPacket"/> class, serializing a <see cref="string"/> into it.
        /// </summary>
        public StringPacket(string str) : base(str) { }

        /// <summary>
        /// Unpacks the <see cref="string"/> serialized into this instance.
        /// </summary>
        public new string Unpack()
        {
            return (string)base.Unpack();
        }

        /// <summary>
        /// Unpacks the <see cref="string"/> serialized into this instance to another <see cref="string"/>.
        /// </summary>
        public void UnpackTo(ref string str)
        {
            str = (string)base.Unpack();
        }
    }

    /// <summary>
    /// Represents a client disconnection packet, mainly sent by a server to a client.
    /// </summary>
    [Serializable]
    public class DisconnectPacket : Packet
    {
        /// <summary>
        /// The reason for the disconnection. The default reason is "Disconnected by server".
        /// </summary>
        public string Reason { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="DisconnectPacket"/> class with a specified reason.
        /// </summary>
        public DisconnectPacket(string reason) : base(null) 
        {
            Reason = reason;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="DisconnectPacket"/> class.
        /// </summary>
        public DisconnectPacket() : base(null)
        {
            Reason = "Disconnected by server";
        }
    }

    /// <summary>
    /// Represents the approval of a client as a packet.
    /// </summary>
    [Serializable]
    public class WelcomePacket : Packet
    {
        /// <summary>
        /// Creates a new instance of the <see cref="WelcomePacket"/> class, serializing server data into it.
        /// </summary>
        public WelcomePacket(ServerUpdatePacket serverInfo) : base(serverInfo) 
        {
        }
    }

    /// <summary>
    /// Encapsulates updated server data.
    /// </summary>
    [Serializable]
    public class ServerUpdatePacket : Packet
    {
        /// <summary>
        /// The maximum number of clients a server can hold.
        /// </summary>
        public int MaxClients { get; }

        /// <summary>
        /// The current number of clients a server holds.
        /// </summary>
        public int CurrentClients { get; }

        /// <summary>
        /// The nickname of the server.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The index of the client to recieve 
        /// </summary>
        public int RecieverIndex { get; }

        /// <summary>
        /// A list of the server's clients.
        /// </summary>
        public ServerClientAbstract[] Clients { get; }

        /// <summary>
        /// Represents the client that connected to the server, if the update was sent because a client connected.
        /// </summary>
        public ServerClientAbstract? NewClient { get; } = null;

        /// <summary>
        /// Resprents the client that got disconnected from the server, if the update was sent because a client was disconnected.
        /// </summary>
        public ServerClientAbstract? DisconnectedClient { get; } = null;

        /// <summary>
        /// The amount of time a disconnected client spent in the server.
        /// </summary>
        public TimeSpan? ConnectionTime { get; } = null;

        /// <summary>
        /// The reason the client got disconnected from the server, if the update was sent because a client was disconnected.
        /// </summary>
        public string DisconnectReason { get; } = null;

        /// <summary>
        /// Creates a new instance of the <see cref="ServerUpdatePacket"/> class with the specified server data.
        /// <para>
		/// This type of <see cref="Packet"/> doesn't serialize any data, as the server information is not serialized.
		/// </para>
		/// </summary>
        public ServerUpdatePacket(int maxClients, int currentClients, string name, int recieverIndex, params ServerClientAbstract[] clientList) : base(null)
        {
            (MaxClients, CurrentClients, Clients, Name, RecieverIndex) = (maxClients, currentClients, clientList, name, recieverIndex);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ServerUpdatePacket"/> class with the specified server data.
        /// <para>
        /// This type of <see cref="Packet"/> doesn't serialize any data, as the server information is not serialized.
        /// </para>
        /// </summary>
        public ServerUpdatePacket(int maxClients, int currentClients, string name, int recieverIndex, ServerClientAbstract newClient, params ServerClientAbstract[] clientList) : base(null)
        {
            (MaxClients, CurrentClients, Clients, Name, NewClient, RecieverIndex) = (maxClients, currentClients, clientList, name, newClient, recieverIndex);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ServerUpdatePacket"/> class with the specified server data.
        /// <para>
        /// This type of <see cref="Packet"/> doesn't serialize any data, as the server information is not serialized.
        /// </para>
        /// </summary>
        public ServerUpdatePacket(int maxClients, int currentClients, string name, int recieverIndex, ServerClientAbstract discClient, string reason, TimeSpan time, params ServerClientAbstract[] clientList) : base(null)
        {
            (MaxClients, CurrentClients, Clients, Name, DisconnectedClient, DisconnectReason, RecieverIndex, ConnectionTime) = (maxClients, currentClients, clientList, name, discClient, reason, recieverIndex, time);
        }
    }

    /// <summary>
    /// Represents the update of a client's name.
    /// </summary>
    [Serializable]
    public class NameUpdatePacket : Packet
    {
        /// <summary>
        /// Creates a new instance of the <see cref="NameUpdatePacket"/> class with the specified name.
        /// </summary>
        public NameUpdatePacket(string name) : base(name) 
        {
        }
    }

    /// <summary>
    /// Represents a <see cref="Packet"/> usually used for measuring the total time data is sent and recieved.
    /// </summary>
    [Serializable]
    public class PingPacket : Packet
    {
        /// <summary>
        /// Creates a new instance of the <see cref="PingPacket"/> class.
        /// </summary>
        public PingPacket() : base(null)
        {

        }

        /// <summary>
        /// Creates a new instance of the <see cref="PingPacket"/> class with a set upload speed.
        /// <para>
        /// This contructor can only be called by a <see cref="ServerClient"/> when responding to pings.
        /// </para>
        /// </summary>
        public PingPacket(long processTime) : base(processTime)
        {
            //if (TypeHelper.GetCallingType(2) != typeof(ServerClient)) throw new MethodAccessException("Packet.Send method was not called by a ServerClient.");
        }
    }

    /// <summary>
    /// Represents a <see cref="Packet"/> that is to be sent from a client to another.
    /// </summary>
    [Serializable]
    public class PeerPacket : Packet
    {
        /// <summary>
        /// The InstanceID of the client to recieve this <see cref="Packet"/>.
        /// </summary>
        public string RecieverID { get; }

        /// <summary>
        /// The InstanceID of the client to have sent this <see cref="Packet"/>.
        /// </summary>
        public string SenderID { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="PeerPacket"/> class, specifying the InstanceID of the recieving client and the data to be sent to it.
        /// </summary>
        public PeerPacket(string senderId, string recieverId, Packet content) : base(content)
        {
            (SenderID, RecieverID) = (senderId, recieverId);
        }
    }

    /// <summary>
    /// Represents a failed operation.
    /// </summary>
    [Serializable]
    public class OperationFailedPacket : Packet
    {
        /// <summary>
        /// The <see cref="Exception"/> that should've originally been thrown when the operation failed.
        /// <para>
        /// If no <see cref="Exception"/> should've been thrown, this value is <see langword="null"/>.
        /// </para>
        /// </summary>
        public Exception Reason { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="OperationFailedPacket"/> class from the <see cref="Packet"/> that caused the operation to fail.
        /// </summary>
        public OperationFailedPacket(Packet packet) : base(packet)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="OperationFailedPacket"/> class from the <see cref="Packet"/> that caused the operation to fail and the <see cref="Exception"/> that should've originally been thrown.
        /// </summary>
        public OperationFailedPacket(Packet packet, Exception exception) : base(packet)
        {
            Reason = exception;
        }

        /// <summary>
        /// Returns the <see cref="Packet"/> that caused the failed operation.
        /// </summary>
        public Packet GetCausingPacket()
        {
            return (Packet)Unpack();
        }
    }
}
