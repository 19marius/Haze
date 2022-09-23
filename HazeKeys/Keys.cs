using System.Collections.Generic;
using System.Linq.Extensions;
using System.Net.Sockets;
using Haze.Packets;
using System;

namespace Haze.Keys
{
    /// <summary>
    /// Represents a key type. Inheritors of this class need to have the <see cref="KeyTypeAttribute"/> attribute.
    /// </summary>
    [Serializable]
    public abstract class Key
    {
        /// <summary>
        /// The types that can create instances of a <see cref="Key"/>.
        /// </summary>
        public HashSet<Type> ValidTypes { get; }

        /// <summary>
        /// Ensures the derived type has the <see cref="KeyTypeAttribute"/> attribute.
        /// </summary>
        /// <exception cref="TypeLoadException"></exception>
        protected Key()
        {
            //Ensure inheritor has the KeyType attribute
            if (!Attribute.IsDefined(GetType(), typeof(KeyTypeAttribute))) throw new TypeLoadException($"The {GetType().Name} type is not marked as a key type.");

            //Get the valid types and check the caller
            ValidTypes = new HashSet<Type>((Attribute.GetCustomAttribute(GetType(), typeof(KeyTypeAttribute)) as KeyTypeAttribute).ValidTypes);
            if (!ValidTypes.Contains(TypeHelper.GetCallingType(3))) throw new TypeLoadException("Key class wasn't created by a valid type.");
        }
    }

    /// <summary>
    /// Ensures only a <see cref="Client"/> can connect to a <see cref="Server"/>.
    /// </summary>
    [KeyType(typeof(Client))]
    [Serializable]
    public class ServerKey : Key
    {
        /// <summary>
        /// The name of the client that created this <see cref="ServerKey"/>.
        /// </summary>
        public string ClientName { get; }

        /// <summary>
        /// Gets additional information about this object.
        /// </summary>
        public object Tag { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="ServerKey"/> class. Only a <see cref="Client"/> can do this.
        /// </summary>
        public ServerKey(string name)
        {
            ClientName = name;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ServerKey"/> class using an additional tag. Only a <see cref="Client"/> can do this.
        /// </summary>
        public ServerKey(string name, object tag)
        {
            ClientName = name;
            Tag = tag;
        }
    }

    /// <summary>
    /// Ensures a <see cref="Packet"/> can only be sent through the <see cref="Packet.Send(NetworkStream, Packet)"/> method.
    /// </summary>
    [KeyType(typeof(Packet))]
    [Serializable]
    public class PacketKey : Key
    {
        /// <summary>
        /// Creates a new instance of the <see cref="PacketKey"/> class. Only a <see cref="Packet"/> can do this.
        /// </summary>
        public PacketKey()
        {

        }
    }
}
