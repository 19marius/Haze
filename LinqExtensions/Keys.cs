using Haze.Keys;

namespace System.Linq.Extensions
{
    /// <summary>
    /// Used by the <see cref="ByteHelper"/> class to encrypt and decrypt serialized objects.
    /// </summary>
    [KeyType(typeof(ByteHelper))]
    [Serializable]
    class SerializationKey : Key
    {
        #region Fields

        object[] controls;

        #endregion

        /// <summary>
        /// Creates a new instance of the <see cref="SerializationKey"/> class. Only the <see cref="ByteHelper"/> type can do this.
        /// </summary>
        public SerializationKey(params object[] controls)
        {
            //Controls are used to increase the complexity of the instance's bytes
            this.controls = controls;
        }
    }
}
