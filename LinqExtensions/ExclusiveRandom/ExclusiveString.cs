namespace System.Linq.Extensions
{
    /// <summary>
    /// Provides unique pseudo-random <see cref="string"/>s.
    /// </summary>
    public class ExclusiveString : ExclusiveRandom<string, int>
    {
        /// <summary>
        /// Checks and modifies if this <see cref="ExclusiveString"/> can generate only alphanumeric characters.
        /// </summary>
        public bool IsAlphanueric { get; set; }

        #region Fields

        string alpha = "aAbBcCdDeEfFgGhHiIjJkKlLmMnNoOpPqQrRsStTuUvVwWxXyYzZ0123456789";

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new instance of the <see cref="ExclusiveString"/> class, specifying if the strings can only be alphanumeric.
        /// </summary>
        public ExclusiveString(bool alphanumeric)
        {
            IsAlphanueric = alphanumeric;
        }

        #endregion

        #region Next Method

        /// <summary>
        /// Returns an unique <see cref="string"/> with a randomized length.
        /// <para>
        /// The length ranges from <paramref name="minLength"/> to <paramref name="maxLength"/>.
        /// </para>
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public override string Next(int minLength, int maxLength)
        {
            //Check arguments & possibilities
            if (minLength > maxLength) throw new ArgumentException("Minimum length cannot be higher than the maximum length.");
            if (minLength <= 0) throw new ArgumentException("Minimum length cannot be smaller or equal to 0.");
            if (Enumerable.Range(minLength, maxLength - minLength + 1).Where(x => selection.Where(s => s.Length == x).Count() < Math.Pow(255, x)).Count() == 0) throw new InvalidOperationException($"All values whose lengths range from {minLength} to {maxLength} have been selected.");

            //Try getting values until valid
            while (true)
            {
                try
                {
                    return Next(rng.Next(minLength, maxLength));
                }
                catch
                { }
            }
        }

        /// <summary>
        /// Returns an unique random <see cref="string"/> with a speicifed length.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public override string Next(int length)
        {
            //Check argument & possiblities
            if (length <= 0) throw new ArgumentException("Length cannot be smaller or equal to 0.");
            if (selection.Where(x => x.Length == length).Count() >= Math.Pow(255, length)) throw new InvalidOperationException($"All values with the length of {length} have been selected.");

            //Assign random values until valid
            string value;
            while (selection.Contains(value = string.Join("", (new int[length]).Select(x => IsAlphanueric ? alpha[rng.Next(alpha.Length)] : (char)rng.Next(1, 256))))) { }
            selection.Add(value);

            return value;

        }

        /// <summary>
        /// Returns an unique random <see cref="string"/> with a randomized length.
        /// <para>
        /// The length ranges from 1 to 10000.
        /// </para>
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public override string Next()
        {
            return Next(1, 10000);
        }

        #endregion
    }
}