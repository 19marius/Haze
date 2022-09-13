using System.Linq.Extensions;
using Haze.Commands;
using Haze.Logging;
using System;

namespace Haze
{
    /// <summary>
    /// Represents the base class for all types that communicate remotely.
    /// </summary>
    public abstract partial class RemoteUnit : CommandExecuter
    {
        #region Constants

        /// <summary>
        /// The maximum data buffer size a <see cref="RemoteUnit"/> can send or recieve.
        /// </summary>
        public const int BufferSize = 12288;

        #endregion

        #region Fields

        static ExclusiveString stringRng = new ExclusiveString(true);

        #endregion

        #region Properties

        /// <summary>
        /// The color a <see cref="RemoteUnit"/> will appear in the console.
        /// </summary>
        public ConsoleColor Color
        {
            get; protected set;
        } = ConsoleColor.White;

        /// <summary>
        /// The custom name of this <see cref="RemoteUnit"/>.
        /// </summary>
        public abstract string Name { get; set; }

        /// <summary>
        /// This <see cref="RemoteUnit"/>'s logger.
        /// </summary>
        public abstract Logger Logger { get; }

        /// <summary>
        /// The randomlt generated instance ID of this <see cref="RemoteUnit"/>
        /// </summary>
        public string ID { get; }

        #endregion

        #region Constructor

        internal RemoteUnit() 
        {
            ID = stringRng.Next(32);
        }

        #endregion

        /// <summary>
        /// If Name is <see langword="null"/> or empty, returns the ID.
        /// </summary>
        public override string ToString()
        {
            return string.IsNullOrEmpty(Name) ? ID : Name;
        }

        #region Operators

        /// <summary>
        /// Returns the string representation of a <see cref="RemoteUnit"/>.
        /// </summary>
        public static implicit operator string(RemoteUnit unit)
        {
            return unit.ToString();
        }

        #endregion
    }
}
