using System.Collections.Generic;

namespace System.Linq.Extensions
{
    /// <summary>
    /// Provides unique pseudo-random values of type <typeparamref name="T"/> based on values that implement <see cref="IComparable"/>.
    /// </summary>
    /// <typeparam name="T">The type of values this class will generated.</typeparam>
    /// <typeparam name="C">The <see cref="IComparable"/> this class will use to limit generated values.</typeparam>
    public abstract class ExclusiveRandom<T, C> where C : IComparable
    {
        #region Fields

        /// <summary>
        /// In a derived class, used when implementing the Next methods.
        /// </summary>
        protected Random rng = new Random();

        /// <summary>
        /// Represents all selected <typeparamref name="T"/> values for this <see cref="ExclusiveRandom{T, C}"/>.
        /// </summary>
        protected HashSet<T> selection = new HashSet<T>();

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new instance of the <see cref="ExclusiveRandom{T, C}"/> class.
        /// </summary>
        public ExclusiveRandom()
        {

        }

        #endregion

        #region Next Method

        /// <summary>
        /// Returns an unique random <typeparamref name="T"/> value within the specified <typeparamref name="C"/> bounds.
        /// </summary>
        public abstract T Next(C low, C high);

        /// <summary>
        /// Returns an unique random <typeparamref name="T"/> value lower or equal to <paramref name="high"/>.
        /// </summary>
        public abstract T Next(C high);

        /// <summary>
        /// Returns an unique random <typeparamref name="T"/> value.
        /// </summary>
        public abstract T Next();

        #endregion
    }
}