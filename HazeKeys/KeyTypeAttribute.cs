using System;

namespace Haze.Keys
{
    /// <summary>
    /// Determines if a class will be used as a key. Classes with this attribute will only be able to be instantiated by certain types, and must derive from <see cref="Key"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class KeyTypeAttribute : Attribute
    {
        /// <summary>
        /// The types that can create an instance of a class with this attribute.
        /// </summary>
        public Type[] ValidTypes { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="KeyTypeAttribute"/> class with the types that can instantiate a class with this attribute.
        /// </summary>
        public KeyTypeAttribute(params Type[] validTypes)
        {
            ValidTypes = validTypes;
        }
    }
}
