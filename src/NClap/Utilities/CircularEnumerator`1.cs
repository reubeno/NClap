using System;
using System.Collections.Generic;

namespace NClap.Utilities
{
    /// <summary>
    /// Encapsulates a circular enumerator of a list.
    /// </summary>
    internal class CircularEnumerator<T>
    {
        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <param name="values">List to enumerate.</param>
        public CircularEnumerator(IReadOnlyList<T> values)
        {
            Values = values;
            CursorIndex = null;
        }

        /// <summary>
        /// The list being enumerated.
        /// </summary>
        public IReadOnlyList<T> Values { get; }

        /// <summary>
        /// The current index into the list.
        /// </summary>
        public int? CursorIndex { get; private set; }

        /// <summary>
        /// True if enumeration has started; false otherwise.
        /// </summary>
        public bool Started => CursorIndex.HasValue;

        /// <summary>
        /// The current item.
        /// </summary>
        public T CurrentItem
        {
            get
            {
                if (!CursorIndex.HasValue)
                {
                    throw new IndexOutOfRangeException();
                }

                return Values[CursorIndex.Value];
            }
        }

        /// <summary>
        /// Move the cursor one item forward.
        /// </summary>
        public void MoveNext()
        {
            if (!CursorIndex.HasValue)
            {
                if (Values.Count != 0)
                {
                    CursorIndex = 0;
                }

                return;
            }

            var index = CursorIndex.Value + 1;
            if (index >= Values.Count)
            {
                index %= Values.Count;
            }

            CursorIndex = index;
        }

        /// <summary>
        /// Move the cursor one item backward.
        /// </summary>
        public void MovePrevious()
        {
            if (!CursorIndex.HasValue)
            {
                if (Values.Count != 0)
                {
                    CursorIndex = Values.Count - 1;
                }

                return;
            }

            var index = CursorIndex.Value - 1;
            if (index < 0)
            {
                index += Values.Count;
            }

            CursorIndex = index;
        }
    }
}
