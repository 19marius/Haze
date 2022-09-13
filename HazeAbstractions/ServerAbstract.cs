using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

namespace Haze.Abstractions
{
    /// <summary>
    /// Represents an abstraction of the server a <see cref="Client"/> connects to.
    /// </summary>
    [Serializable]
    public class ServerAbstract : IEnumerable<ServerClientAbstract>
    {
        #region Properties

        /// <summary>
        /// The maximum number of clients the <see cref="Server"/> can hold.
        /// </summary>
        public int MaxClients
        {
            get => maxClients;
        }

        /// <summary>
        /// The current amount of clients in the <see cref="Server"/>.
        /// </summary>
        public int CurrentClients
        {
            get => currClients;
        }

        /// <summary>
        /// The nickname of the <see cref="Server"/>.
        /// </summary>
        public string Name
        {
            get => name;
        }

        /// <summary>
        /// The index of the client that will recieve this <see cref="ServerAbstract"/>.
        /// </summary>
        public int RecieverIndex 
        {
            get => index;
        }

        #endregion

        #region Indexers

        /// <summary>
        /// Gets the client at the specified index.
        /// </summary>
        /// <returns>A <see cref="ServerClientAbstract"/> which provides basic information about the client.</returns>
        public ServerClientAbstract this[int index]
        {
            get => clients[index];
        }

        /// <summary>
        /// Gets the client with the specified ID.
        /// </summary>
        public ServerClientAbstract this[string id]
        {
            get => idClients[id];
        }

        #endregion

        #region Fields

        int maxClients, currClients, index;
        ServerClientAbstract[] clients;
        Dictionary<string, ServerClientAbstract> idClients = new Dictionary<string, ServerClientAbstract>();
        Dictionary<string, int> indexClients = new Dictionary<string, int>();
        string name;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new instance of the <see cref="Server"/> class with specific information about it.
        /// </summary>
        public ServerAbstract(int maxClients, int currentClients, string name, int index, params ServerClientAbstract[] clients)
        {
            ChangeServerInformation(maxClients, currentClients, name, index, clients);
        }

        #endregion

        /// <summary>
        /// Checks if this <see cref="ServerAbstract"/> contains a client with the specified ID.
        /// </summary>
        public bool ContainsClient(string id)
        {
            return idClients.ContainsKey(id);
        }

        /// <summary>
        /// Gets the index of the client in this <see cref="ServerAbstract"/> with the specified ID.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public int IndexOf(string id)
        {
            if (!indexClients.ContainsKey(id)) throw new ArgumentException("No client corresponds to id " + id + ".");

            return indexClients[id];
        }

        /// <summary>
        /// Changes all properties of this <see cref="ServerAbstract"/>.
        /// </summary>
        public void ChangeServerInformation(int maxClients, int currentClients, string name, int index, params ServerClientAbstract[] clients)
        {
            (this.maxClients, currClients, this.clients, this.name, this.index) = (maxClients, currentClients, clients, name, index);
            
            //Store the clients by ID
            idClients = clients?.ToDictionary(x => x.ID);
            indexClients = clients is null ? null : Enumerable.Range(0, clients.Length).ToDictionary(x => clients[x].ID);
        }

        /// <summary>
        /// If Name is <see langword="null"/> or empty, returns the value of the base implementation.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.IsNullOrEmpty(Name) ? base.ToString() : Name;
        }

        #region IEnumerable Methods

        /// <summary>
        /// Returns an enumerator that iterates through this <see cref="Server"/>'s clients.
        /// </summary>
        public IEnumerator<ServerClientAbstract> GetEnumerator()
        {
            return (IEnumerator<ServerClientAbstract>)clients.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Operators

        /// <summary>
        /// Returns the string representation of a <see cref="ServerAbstract"/>.
        /// </summary>
        public static implicit operator string(ServerAbstract server)
        {
            return server.ToString();
        }

        #endregion
    }
}