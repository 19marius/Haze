using System.Collections.Generic;
using System.Collections;
using System;

namespace MessengerClient.Text
{
    /// <summary>
    /// Represents a collection of formatted text effect groups.
    /// </summary>
    public class FormattedTextEffectGroupCollection : ICollection<FormattedTextEffectGroup>, IList<FormattedTextEffectGroup>, IEnumerable<FormattedTextEffectGroup>, IReadOnlyCollection<FormattedTextEffectGroup>, IReadOnlyList<FormattedTextEffectGroup>
    {
        #region Properties

        /// <summary>
        /// Gets the number of elements contained in the <see cref="FormattedTextEffectGroupCollection"/>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="FormattedTextEffectGroupCollection"/>.
        /// </returns>
        public int Count
        {
            get => effects.Count;
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="FormattedTextEffectGroupCollection"/> is read-only.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the <see cref="FormattedTextEffectGroupCollection"/> is read-only; otherwise, <see langword="false"/>.
        /// </returns>
        public bool IsReadOnly { get; set; }

        #endregion

        public FormattedTextEffectGroup this[int index]
        {
            get => effects[index];
            set
            {
                if (IsReadOnly) throw new NotSupportedException("Collection is in a read-only state.");

                //Replace in the list
                var oldItem = effects[index];
                effects[index] = value;

                //Replace in the hash set
                if (!effects.Contains(oldItem)) hashEffects.Remove(oldItem);
                hashEffects.Add(value);
            }
        }

        #region Fields

        List<FormattedTextEffectGroup> effects;
        HashSet<FormattedTextEffectGroup> hashEffects;

        #endregion

        /// <summary>
        /// Creates an empty instance of the <see cref="FormattedTextEffectGroupCollection"/> class.
        /// </summary>
        public FormattedTextEffectGroupCollection()
        {
            effects = new List<FormattedTextEffectGroup>();
            hashEffects = new HashSet<FormattedTextEffectGroup>();
        }

        /// <summary>
        /// Creates an instance of the <see cref="FormattedTextEffectGroupCollection"/> class as a wrapper for the specified list.
        /// </summary>
        public FormattedTextEffectGroupCollection(IList<FormattedTextEffectGroup> list)
        {
            effects = new List<FormattedTextEffectGroup>(list);
            hashEffects = new HashSet<FormattedTextEffectGroup>(list);
        }

        /// <summary>
        /// Creates an instance of the <see cref="FormattedTextEffectGroupCollection"/> class that contains elements copied from the specified collection.
        /// </summary>
        public FormattedTextEffectGroupCollection(IEnumerable<FormattedTextEffectGroup> list)
        {
            effects = new List<FormattedTextEffectGroup>(list);
            hashEffects = new HashSet<FormattedTextEffectGroup>(list);
        }

        /// <summary>
        /// Adds an item to the <see cref="FormattedTextEffectGroupCollection"/>.
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        public void Add(FormattedTextEffectGroup item)
        {
            if (IsReadOnly) throw new NotSupportedException("Collection is in a read-only state.");

            effects.Add(item);
            hashEffects.Add(item);
        }

        /// <summary>
        /// Removes all items from the <see cref="FormattedTextEffectGroupCollection"/>.
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        public void Clear()
        {
            if (IsReadOnly) throw new NotSupportedException("Collection is in a read-only state.");

            effects.Clear();
            hashEffects.Clear();
        }

        /// <summary>
        /// Determines whether the <see cref="FormattedTextEffectGroupCollection"/> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="FormattedTextEffectGroupCollection"/>.</param>
        /// <returns><see langword="true"/> if item is found in the <see cref="FormattedTextEffectGroupCollection"/>; otherwise, <see langword="false"/>.</returns>
        public bool Contains(FormattedTextEffectGroup item)
        {
            return hashEffects.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="FormattedTextEffectGroupCollection"/> to an <see cref="Array"/>, starting at a particular <see cref="Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional System.Array that is the destination of the elements copied from <see cref="FormattedTextEffectGroupCollection"/>. The <see cref="Array"/> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="ArgumentException"/>
        public void CopyTo(FormattedTextEffectGroup[] array, int arrayIndex)
        {
            effects.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="FormattedTextEffectGroupCollection"/>.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="FormattedTextEffectGroupCollection"/>.</param>
        /// <returns>The index of item if found in the list; otherwise, -1.</returns>
        public int IndexOf(FormattedTextEffectGroup item)
        {
            return effects.IndexOf(item);
        }

        /// <summary>
        /// Inserts an item to the <see cref="FormattedTextEffectGroupCollection"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="FormattedTextEffectGroupCollection"/>.</param>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void Insert(int index, FormattedTextEffectGroup item)
        {
            if (IsReadOnly) throw new NotSupportedException("Collection is in a read-only state.");

            effects.Insert(index, item);
            hashEffects.Add(item);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="FormattedTextEffectGroupCollection"/>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="FormattedTextEffectGroupCollection"/>.</param>
        /// <returns><see langword="true"/> if item was successfully removed from the <see cref="FormattedTextEffectGroupCollection"/>; otherwise, <see langword="false"/>. This method also returns <see langword="false"/> if item is not found in the original <see cref="FormattedTextEffectGroupCollection"/>.</returns>
        /// <exception cref="NotSupportedException"></exception>
        public bool Remove(FormattedTextEffectGroup item)
        {
            if (IsReadOnly) throw new NotSupportedException("Collection is in a read-only state.");
            if (!Contains(item)) return false;

            var removed = effects.Remove(item);
            if (!effects.Contains(item)) hashEffects.Remove(item);
            return removed;
        }

        /// <summary>
        /// Removes the <see cref="FormattedTextEffectGroupCollection"/> item at the specified index.
        /// </summary>
        /// <param name="index">he zero-based index of the item to remove.</param>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void RemoveAt(int index)
        {
            if (IsReadOnly) throw new NotSupportedException("Collection is in a read-only state.");

            var item = effects[index];
            effects.RemoveAt(index);
            if (!effects.Contains(item)) hashEffects.Remove(item);
        }

        #region IEnumerable Methods

        public IEnumerator<FormattedTextEffectGroup> GetEnumerator()
        {
            return effects.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
