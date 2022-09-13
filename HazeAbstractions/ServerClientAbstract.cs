using System;

#region Warnings
 
#pragma warning disable CS1591 //Contructor of struct isn't described
 
#endregion

namespace Haze.Abstractions
{
    /// <summary>
    /// Represents a server's client, proving limited information about it.
    /// </summary>
    [Serializable]
    public struct ServerClientAbstract
    {
        /// <summary>
        /// The client's custom nickname.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The client's randomly generated ID.
        /// </summary>
        public string ID { get; }

        public ServerClientAbstract(string id)
        {
            ID = id;
            Name = "";
        }

        /// <summary>
        /// If Name is <see langword="null"/> or empty, returns the ID.
        /// </summary>
        public override string ToString()
        {
            return string.IsNullOrEmpty(Name) ? ID : Name;
        }

        #region Operators

        /// <summary>
        /// Returns the string representation of a <see cref="ServerClientAbstract"/>.
        /// </summary>
        /// <param name="client"></param>
        public static implicit operator string(ServerClientAbstract client)
        {
            return client.ToString();
        }

        #endregion
    }
}