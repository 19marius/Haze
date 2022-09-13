namespace System.Linq.Extensions
{
    /// <summary>
    /// Provides unique pseudo-random integers.
    /// </summary>
    public class ExclusiveInteger : ExclusiveRandom<int, int>
    {
        #region Constructor

        /// <summary>
        /// Creates a new instance of the <see cref="ExclusiveInteger"/> class.
        /// </summary>
        public ExclusiveInteger()
        {
        }

        #endregion

        #region Next Method

        /// <summary>
        /// Returns an unique random <see cref="int"/> within the specified bounds.
        /// <para>
        /// The yield can be equal to <paramref name="low"/> or <paramref name="high"/>.
        /// </para>
        /// </summary>
        public override int Next(int low, int high)
        {
            //Argument check
            if (low > high) throw new ArgumentException("Low limit cannot be higher than high limit.");
            if (Enumerable.Range(low, high - low + 1).Where(x => !selection.Contains(x)).Count() == 0) throw new InvalidOperationException($"All values in the range of {low} - {high} were already selected.");

            //Assign random values until valid
            int result;
            while (selection.Contains(result = rng.Next(low, high + 1))) { }
            selection.Add(result);

            return result;
        }

        /// <summary>
        /// Returns an unique random <see cref="int"/> which is less than or equal to <paramref name="high"/>.
        /// </summary>
        public override int Next(int high)
        {
            //Assign random values until valid
            int result;
            while (selection.Contains(result = rng.Next(int.MinValue, high + 1))) { }
            selection.Add(result);

            return result;
        }

        /// <summary>
        /// Returns an unique random <see cref="int"/>.
        /// </summary>
        public override int Next()
        {
            //Assign random values until valid
            int result;
            while (selection.Contains(result = rng.Next(int.MinValue, int.MaxValue))) { }
            selection.Add(result);

            return result;
        }

        #endregion
    }
}