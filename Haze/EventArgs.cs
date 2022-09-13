using Haze.Abstractions;
using Haze.Packets;
using System;

namespace Haze
{
    /// <summary>
    /// Represents data for failed remote operations.
    /// </summary>
    public class OperationFailedEventArgs : EventArgs
    {
        /// <summary>
        /// The <see cref="Packet"/> that caused the operation to fail.
        /// </summary>
        public Packet CausingPacket { get; }

        /// <summary>
        /// Additional tags.
        /// </summary>
        public object[] Tags { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="OperationFailedEventArgs"/> class with the specified tags.
        /// </summary>
        internal OperationFailedEventArgs(Packet causingPacket, params object[] tags)
        {
            (CausingPacket, Tags) = (causingPacket, tags);
        }
    }

    /// <summary>
    /// Represents data for a <see cref="ServerClient"/> whose remote client was disconnected from the server they were connected to.
    /// </summary>
    public class ServerClientDisconnectedEventArgs : EventArgs
    {
        /// <summary>
        /// The <see cref="ServerClient"/> that was disconnected.
        /// </summary>
        public ServerClient Client { get; }

        /// <summary>
        /// The reason they were disconnected for.
        /// </summary>
        public string Reason { get; }

        /// <summary>
        /// Checks if the client was also invalidated when disconnected.
        /// <para>
        /// Invalidation is defined as the remote client's first packet not being the server key.
        /// </para>
        /// </summary>
        public bool Invalidated { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="ServerClientDisconnectedEventArgs"/> class with the specified properties.
        /// </summary>
        internal ServerClientDisconnectedEventArgs(ServerClient client, string reason, bool invalidated)
        {
            (Client, Reason, Invalidated) = (client, reason, invalidated);
        }
    }

    /// <summary>
    /// Represents data for a client which was disconnected from the server they were connected to.
    /// </summary>
    public class ClientDisconnectedEventArgs : EventArgs
    {
        /// <summary>
        /// The client that was disconnected.
        /// </summary>
        public ServerClientAbstract Client { get; }

        /// <summary>
        /// The reason they were disconnected for.
        /// </summary>
        public string Reason { get; }

        /// <summary>
        /// The time the client was connected to the server.
        /// </summary>
        public TimeSpan ConnectionTime { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="ServerClientDisconnectedEventArgs"/> class with the specified properties.
        /// </summary>
        internal ClientDisconnectedEventArgs(ServerClientAbstract client, string reason, TimeSpan time)
        {
            (Client, Reason, ConnectionTime) = (client, reason, time);
        }
    }

    /// <summary>
    /// Represents data for a <see cref="ServerClient"/> whose remote client has connected to the server.
    /// </summary>
    public class ServerClientConnectedEventArgs : EventArgs
    {
        /// <summary>
        /// The <see cref="ServerClient"/> whose client has connected to the server.
        /// </summary>
        public ServerClient Client { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="ClientConnectedEventArgs"/> class with the client that has connected to the server.
        /// </summary>
        internal ServerClientConnectedEventArgs(ServerClient client)
        {
            Client = client;
        }
    }

    /// <summary>
    /// Represents data for a client which has connected to the server.
    /// </summary>
    public class ClientConnectedEventArgs : EventArgs
    {
        /// <summary>
        /// The client that has connected to the server.
        /// </summary>
        public ServerClientAbstract Client { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="ClientConnectedEventArgs"/> class with the client that has connected to the server.
        /// </summary>
        internal ClientConnectedEventArgs(ServerClientAbstract client)
        {
            Client = client;
        }
    }

    /// <summary>
    /// Represents data for a <see cref="RemoteUnit"/> recieving a <see cref="Packet"/>.
    /// </summary>
    public class DataRecievedEventArgs : EventArgs
    {
        /// <summary>
        /// The recieved data.
        /// </summary>
        public Packet Data { get; }

        /// <summary>
        /// The time at which the data was recieved.
        /// </summary>
        public DateTime RecievalTime { get; }

        /// <summary>
        /// Additional tags.
        /// </summary>
        public object[] Tags { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="DataRecievedEventArgs"/> class with the recieved data, the time it was recieved at and optional tags.
        /// </summary>
        internal DataRecievedEventArgs(Packet data, DateTime recievalTime, params object[] tags)
        {
            (Data, RecievalTime, Tags) = (data, recievalTime, tags);
        }
    }
}
