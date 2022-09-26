using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq.Extensions;
using System.Net.Sockets;
using System.Collections;
using System.Diagnostics;
using Haze.Abstractions;
using Haze.Packets;
using Haze.Logging;
using System.Linq;
using System.Net;
using System;

namespace Haze
{
    /// <summary>
    /// Provides communication between clients.
    /// </summary>
    public partial class Server : RemoteUnit, IEnumerable<ServerClient>, IDisposable
    {
        #region Delegates

        /// <summary>
        /// Represents the method(s) that will be called when a sent <see cref="Packet"/> caused an operation to fail for the client at <paramref name="e"/>.Tags[<c>0</c>].
        /// </summary>
        public delegate void OperationFailedEventHandler(object sender, OperationFailedEventArgs e);

        /// <summary>
        /// Represents the method(s) that will be called when a <see cref="RemoteUnit"/> recieves data from another <see cref="RemoteUnit"/>.
        /// <para>
        /// <paramref name="e"/>.Tags[<c>0</c>] is equal to the index of the <see cref="ServerClient"/> that sent the data in the server list.
        /// </para>
        /// </summary>
        public delegate void DataRecievedEventHandler(object sender, DataRecievedEventArgs e);

        /// <summary>
        /// Represents the method(s) that will be called when a client connects and is validated to a <see cref="Server"/>.
        /// </summary>
        public delegate void ClientConnectedEventHandler(object sender, ServerClientConnectedEventArgs e);

        /// <summary>
        /// Represents the method(s) that will be called when a client gets disconnected from a <see cref="Server"/>, for any reason.
        /// </summary>
        public delegate void ClientDisconnectedEventHandler(object sender, ServerClientDisconnectedEventArgs e);

        #endregion

        #region Properties

        /// <summary>
        /// The <see cref="TcpListener"/> which listens for incoming connections.
        /// </summary>
        public TcpListener Listener { get; }

        /// <summary>
        /// The maximum number of clients this <see cref="Server"/> can hold.
        /// <para>
        /// When this value is changed, any clients that connected outside the new range will be disconnected.
        /// </para>
        /// </summary>
        public int MaxClients
        {
            get => maxClients;
            set
            {
                //Dispose required clients & resize the collection
                for (int i = value - 1; i < maxClients; i++) clients[i].Dispose();
                clients.Resize(value);

                maxClients = value;
            }
        }

        /// <summary>
        /// The current amount of clients in this <see cref="Server"/>.
        /// </summary>
        public int CurrentClients
        {
            get => clients.Count;
        }

        /// <summary>
        /// Checks if this <see cref="Server"/> reached the maximum client capacity.
        /// </summary>
        public bool IsFull
        {
            get => CurrentClients >= MaxClients;
        }

        /// <summary>
        /// This <see cref="Server"/>'s logger.
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
        /// The index of this <see cref="Server"/>'s instance.
        /// </summary>
        public int Instance { get; }

        /// <summary>
        /// Gets the port this <see cref="Server"/> is using to listen for connections.
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// The nickname of this <see cref="Server"/>.
        /// </summary>
        public override string Name { get; set; }

        /// <summary>
        /// Determines which commands cannot be executed.
        /// </summary>
        public override string[] DisabledCommands { get; } = Array.Empty<string>();

        /// <summary>
        /// The total time this <see cref="Server"/> has been opened for.
        /// </summary>
        public TimeSpan TotalTime
        {
            get => timer.Elapsed;
        }

        /// <summary>
        /// The time in milliseconds after a validation check is done for every new unchecked <see cref="ServerClient"/>.
        /// </summary>
        public int ValidationTime { get; set; } = 10000;

        #endregion

        #region Events

        /// <summary>
        /// Invoked when a <see cref="Packet"/> sent to a remote client caused an operation to fail.
        /// </summary>
        public event OperationFailedEventHandler OperationFailed;

        /// <summary>
        /// Invoked when this any client connected to this <see cref="Server"/> recieves data from the remote client.
        /// <para>
        /// The third parameter's first element will be the index of the <see cref="ServerClient"/> that recieved the specified data.
        /// </para>
        /// </summary>
        public event DataRecievedEventHandler DataRecieved;

        /// <summary>
        /// Invoked when a client connects and is validated to this <see cref="Server"/>.
        /// </summary>
        public event ClientConnectedEventHandler ClientConnected;

        /// <summary>
        /// Invoked when a client gets disconnected from this <see cref="Server"/>, for any reason.
        /// </summary>
        public event ClientDisconnectedEventHandler ClientDisconnected;

        #endregion

        #region Indexers

        /// <summary>
        /// Gets the client at the specified index.
        /// </summary>
        /// <returns>A <see cref="ServerClient"/> to handle manipulation of the client.</returns>
        public ServerClient this[int index] => clients[index];

        /// <summary>
        /// Gets the client with the specified ID.
        /// </summary>
        public ServerClient this[string id] => idClients[id];

        #endregion

        #region Fields

        static int instances;

        bool disposed;
        int maxClients;
        LinkedList<ServerClient> queue = new LinkedList<ServerClient>();
        ObservableCollection<ServerClient> clients = new ObservableCollection<ServerClient>();
        Dictionary<string, ServerClient> idClients = new Dictionary<string, ServerClient>();
        Dictionary<string, int> indexClients = new Dictionary<string, int>();
        Stopwatch timer = new Stopwatch();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of the <see cref="Server"/> on the specified port.
        /// </summary>
        public Server(int port)
        {
            Color = ConsoleColor.Magenta;
            (Name, Listener, Instance, Logger) = ("", new TcpListener(IPAddress.Any, port), instances++, new Logger(this));
        }

        /// <summary>
        /// Initializes an instance of the <see cref="Server"/> on the specified port and the specified nickname.
        /// </summary>
        public Server(int port, string name)
        {
            Color = ConsoleColor.Magenta;
            (Name, Listener, Instance, Logger) = (name, new TcpListener(IPAddress.Any, port), instances++, new Logger(this));
        }

        #endregion

        /// <summary>
        /// Begins listening for incoming connections with the specified client limit.
        /// </summary>
        public void Start(int maxClients)
        {
            //Declarations
            MaxClients = maxClients;
            clients.CollectionChanged += CheckConnectionBarrier;

            //Start the server
            Listener.Start();
            Listener.BeginAcceptTcpClient(ConnectedCallback, Listener);
            Logger.WriteLog(null, false, "server started successfully", ConsoleColor.Green);
            timer.Start();
        }

        /// <summary>
        /// Releases all resources used by this <see cref="Server"/> and closes it.
        /// </summary>
        public void Dispose()
        {
            //Ensure Dispose can only be called once
            if (!disposed)
            {
                //Stop the timer & log the final client tally
                timer.Stop();
                Logger.WriteLog(null, false, "server is being disposed. total time: " + TotalTime);

                //Dispose of the Logger & the TcpListener
                Listener.Stop();
                Logger.Dispose();

                //Remove event handlers
                clients.CollectionChanged -= CheckConnectionBarrier;
                for (int i = 0; i < clients.Count; i++)
                {
                    clients[i].Dispose();
                    EventHelper.ClearInvocationList(clients[i], "Disconnected");
                }

                //Clear the list
                clients.Clear();
                disposed = true;
            }
        }

        /// <summary>
        /// Disconnects a client from the client list.
        /// </summary>
        public void RemoveClient(int index)
        {
            clients[index].Dispose();
        }

        #region SendUpdates Method

        /// <summary>
        /// Sends every client in the server a <see cref="ServerUpdatePacket"/>.
        /// </summary>
        public void SendUpdates()
        {
            for (int i = 0; i < clients.Count; i++) clients[i].Send(new ServerUpdatePacket(MaxClients, CurrentClients, Name, i, clients.Select(x => new ServerClientAbstract(x.ID) { Name = x.Name }).ToArray()));
        }

        /// <summary>
        /// Sends every client in the server at each specified index a <see cref="ServerUpdatePacket"/>.
        /// </summary>
        public void SendUpdates(int[] indexes)
        {
            for (int i = 0; i < indexes.Length; i++) clients[indexes[i]].Send(new ServerUpdatePacket(MaxClients, CurrentClients, Name, i, clients.Select(x => new ServerClientAbstract(x.ID) { Name = x.Name }).ToArray()));
        }

        #endregion

        #region Send Method

        /// <summary>
        /// Packs and sends an <see cref="object"/> to all clients connected to this <see cref="Server"/>.
        /// </summary>
        public void Send(object obj)
        {
            for (int i = 0; i < clients.Count; i++) clients[i].Send(obj);
        }

        /// <summary>
        /// Sends a <see cref="Packet"/> to all clients connected to this <see cref="Server"/>.
        /// </summary>
        public void Send(Packet packet)
        {
            for (int i = 0; i < clients.Count; i++) clients[i].Send(packet);
        }

        /// <summary>
        /// Packs and sends an <see cref="object"/> to the client at the specified index.
        /// </summary>
        public void Send(int index, object obj)
        {
            clients[index].Send(obj);
        }

        /// <summary>
        /// Sends a <see cref="Packet"/> to the client at the specified index.
        /// </summary>
        public void Send(int index, Packet packet)
        {
            clients[index].Send(packet);
        }

        #endregion

        /// <summary>
        /// Checks if this <see cref="Server"/> contains a client with the specified ID.
        /// </summary>
        public bool ContainsClient(string id)
        {
            return idClients.ContainsKey(id);
        }

        /// <summary>
        /// Gets the index of the client in this <see cref="Server"/> with the specified ID.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public int IndexOf(string id)
        {
            if (!indexClients.ContainsKey(id)) throw new ArgumentException("No client corresponds to id " + id + ".");

            return indexClients[id];
        }

        #region Private Methods

        /// <summary>
        /// Called when an incoming connection is intercepted.
        /// </summary>
        void ConnectedCallback(IAsyncResult connectionResult)
        {
            //Client number check
            if (clients.Count + queue.Count < MaxClients)
            {
                //Add the client to the queue
                var client = queue.AddLast(new ServerClient(this, Listener.EndAcceptTcpClient(connectionResult)));
                Logger.WriteLog(client.Value, true, "incoming connection from " + client.Value.Address + ", " + (ValidationTime / 1000f) + "s validation time, added to the queue", ConsoleColor.DarkGreen);

                //Assign event callbacks
                client.Value.queueNode = client;
                client.Value.Connected += OnClientConnect;
                client.Value.Disconnected += OnClientDisconnect;

                //Listen for other clients
                Listener.BeginAcceptTcpClient(ConnectedCallback, Listener);
            }
        }

        /// <summary>
        /// Called when a <see cref="ServerClient"/> connects to the server.
        /// </summary>
        object OnClientConnect(object sender, ServerClientConnectedEventArgs args)
        {
            var client = (ServerClient)sender;

            //Set the client's index
            client.Tags = new object[] { clients.Count };

            //Remove from the queue & add store client and its id
            queue.Remove(client.queueNode);
            client.queueNode = null;

            clients.Add(client);
            idClients.Add(client.ID, client);
            indexClients.Add(client.ID, clients.Count - 1);

            //Assign & invoke events
            ClientConnected?.Invoke(this, args);
            client.DataRecieved += (s, dataArgs) => DataRecieved?.Invoke(this, new DataRecievedEventArgs(dataArgs.Data, dataArgs.RecievalTime, client.Tags[0]));
            client.OperationFailed += (s, failArgs) => OperationFailed?.Invoke(this, new OperationFailedEventArgs(failArgs.CausingPacket, client.Tags[0]));

            //Log the connection
            Logger.WriteLog(client, false, client.Address + " connected", ConsoleColor.Green);

            return null;
        }

        /// <summary>
        /// Called when a <see cref="ServerClient"/> gets disconnected from the server.
        /// </summary>
        void OnClientDisconnect(object sender, ServerClientDisconnectedEventArgs args)
        {
            var client = (ServerClient)sender;

            //Check for disconnection validation
            if (!args.Invalidated)
            {
                //Update indexes
                int clientIndex = (int)client.Tags[0];
                for (int i = clientIndex; i < clients.Count; i++)
                {
                    clients[i].Tags[0] = ((int)clients[i].Tags[0]) - 1;
                    indexClients[clients[i].ID]--;
                }

                //Remove client from enumerables
                idClients.Remove(client.ID);
                indexClients.Remove(client.ID);
                clients.RemoveAt(clientIndex);
                ClientDisconnected?.Invoke(this, args);
            }

            //Remove from the queue
            else queue.Remove(client.queueNode);

            //Log the disconnection
            Logger.WriteLog(client, true, (args.Invalidated ? "invalidated" : "disconnected") + (string.IsNullOrEmpty(args.Reason) ? "" : " (" + args.Reason + ")"), args.Invalidated ? ConsoleColor.Red : ConsoleColor.Yellow);
        }

        /// <summary>
        /// Called when the client list was modified to allow new connections if the <see cref="Server"/> was full.
        /// </summary>
        void CheckConnectionBarrier(object sender, NotifyCollectionChangedEventArgs args)
        {
            bool added = args.Action == NotifyCollectionChangedAction.Add;

            //Get the right client
            ServerClient client = added ? clients[clients.Count - 1] : (ServerClient)args.OldItems[0];

            //Check if a client connected
            if (added) SendUpdates(Enumerable.Range(0, clients.Count - 1).ToArray(), new ServerClientAbstract(client.ID) { Name = client.Name });
            
            //Or if a client was disconnected
            else if (args.Action != NotifyCollectionChangedAction.Replace) SendUpdates(new ServerClientAbstract(client.ID) { Name = client.Name }, client.dcr, client.ConnectionTime);

            //Check if a client was removed before accepting
            if (!added && clients.Count < MaxClients) Listener.BeginAcceptTcpClient(ConnectedCallback, Listener);
        }

        #region SendUpdates Method

        /// <summary>
        /// Sends every client in the server a <see cref="ServerUpdatePacket"/> which specifies the connection of a new client.
        /// </summary>
        void SendUpdates(ServerClientAbstract newClient)
        {
            for (int i = 0; i < clients.Count; i++) clients[i].Send(new ServerUpdatePacket(MaxClients, CurrentClients, Name, i, newClient, clients.Select(x => new ServerClientAbstract(x.ID) { Name = x.Name }).ToArray()));
        }

        /// <summary>
        /// Sends every client in the server a <see cref="ServerUpdatePacket"/> which specifies the discnnnection of a client.
        /// </summary>
        void SendUpdates(ServerClientAbstract discClient, string reason, TimeSpan time)
        {
            for (int i = 0; i < clients.Count; i++) clients[i].Send(new ServerUpdatePacket(MaxClients, CurrentClients, Name, i, discClient, reason, time, clients.Select(x => new ServerClientAbstract(x.ID) { Name = x.Name }).ToArray()));
        }

        /// <summary>
        /// Sends every client in the server at each specified index a <see cref="ServerUpdatePacket"/> which specifies the connection of a new client.
        /// </summary>
        void SendUpdates(int[] indexes, ServerClientAbstract newClient)
        {
            for (int i = 0; i < indexes.Length; i++) clients[indexes[i]].Send(new ServerUpdatePacket(MaxClients, CurrentClients, Name, indexes[i], newClient, clients.Select(x => new ServerClientAbstract(x.ID) { Name = x.Name }).ToArray()));
        }

        /// <summary>
        /// Sends every client in the server at each specified index a <see cref="ServerUpdatePacket"/> which specifies the disconnection of a client.
        /// </summary>
        void SendUpdates(int[] indexes, ServerClientAbstract discClient, string reason, TimeSpan time)
        {
            for (int i = 0; i < indexes.Length; i++) clients[indexes[i]].Send(new ServerUpdatePacket(MaxClients, CurrentClients, Name, indexes[i], discClient, reason, time, clients.Select(x => new ServerClientAbstract(x.ID) { Name = x.Name }).ToArray()));
        }

        #endregion

        #endregion

        #region IEnumerable Methods

        /// <summary>
        /// Returns an enumerator that iterates through this <see cref="Server"/>'s clients.
        /// </summary>
        public IEnumerator<ServerClient> GetEnumerator()
        {
            return clients.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}