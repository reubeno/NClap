using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace NClap.ConsoleInput
{
    /// <summary>
    /// Encapsulates management of console command history.
    /// </summary>
    internal class ConsoleHistory : IConsoleHistory
    {
        private readonly List<string> _entries = new List<string>();
        private int _cursorIndex;

        /// <summary>
        /// Default constructor, which creates a console history without a bound
        /// on its entries.
        /// </summary>
        public ConsoleHistory() : this(null)
        {
        }

        /// <summary>
        /// Basic constructor that allows specifying a maximum entry count.
        /// </summary>
        /// <param name="maxEntryCount">Maximum entry count.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when
        /// <paramref name="maxEntryCount" /> is 0 or negative.</exception>
        public ConsoleHistory(int? maxEntryCount)
        {
            if (maxEntryCount.HasValue && (maxEntryCount.Value <= 0))
            {
                throw new ArgumentOutOfRangeException(nameof(maxEntryCount));
            }

            MaxEntryCount = maxEntryCount;
        }

        /// <summary>
        /// The maximum entry count, or null to indicate there is no maximum.
        /// </summary>
        public int? MaxEntryCount { get; }

        /// <summary>
        /// The count of entries in the history.
        /// </summary>
        public int EntryCount => _entries.Count;

        /// <summary>
        /// If the cursor is valid, the current entry in the history; null
        /// otherwise.
        /// </summary>
        public string CurrentEntry => _cursorIndex < _entries.Count ? _entries[_cursorIndex] : null;

        /// <summary>
        /// Add a new entry to the end of the history, and reset the history's
        /// cursor to that new entry.
        /// </summary>
        /// <param name="entry"></param>
        public void Add(string entry)
        {
            // Only add the entry to history if it contains non-whitespace.
            if (string.IsNullOrWhiteSpace(entry))
            {
                return;
            }

            // Remove an item if we need space.
            if (MaxEntryCount.HasValue && (_entries.Count >= MaxEntryCount.Value))
            {
                var entriesToRemove = _entries.Count - (MaxEntryCount.Value - 1);
                Debug.Assert(entriesToRemove >= 0);

                _entries.RemoveRange(0, entriesToRemove);
                _cursorIndex = _entries.Count;
            }

            _entries.Add(entry);
            _cursorIndex = _entries.Count;
        }

        /// <summary>
        /// Move the current history cursor by the specified offset.
        /// </summary>
        /// <param name="offset">Positive or negative offset to apply to the
        /// current cursor.</param>
        /// <returns>True on success; false if the move could not be made.
        /// </returns>
        public bool MoveCursor(int offset) => MoveCursor(SeekOrigin.Current, offset);

        /// <summary>
        /// Move the current history cursor by the specified offset.
        /// </summary>
        /// <param name="origin">Reference for movement.</param>
        /// <param name="offset">Positive or negative offset to apply to the
        /// specified origin.</param>
        /// <returns>True on success; false if the move could not be made.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="origin"/>
        /// is not a valid origin value.</exception>
        public bool MoveCursor(SeekOrigin origin, int offset)
        {
            int baseIndex;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    baseIndex = 0;
                    break;

                case SeekOrigin.Current:
                    baseIndex = _cursorIndex;
                    break;

                case SeekOrigin.End:
                    baseIndex = _entries.Count;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(origin));
            }

            var newIndex = baseIndex + offset;
            if ((newIndex < 0) || (newIndex > _entries.Count))
            {
                return false;
            }

            _cursorIndex = newIndex;
            return true;
        }
    }
}
