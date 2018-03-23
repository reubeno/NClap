using System;
using System.Text;

namespace NClap.ConsoleInput
{
    /// <summary>
    /// Console status bar, used only for internal testing.
    /// </summary>
    internal class ConsoleStatusBar
    {
        private const ConsoleColor ForegroundColor = ConsoleColor.Yellow;
        private const ConsoleColor BackgroundColor = ConsoleColor.Magenta;

        private readonly StringBuilder _contents = new StringBuilder();

        /// <summary>
        /// Constructor.
        /// </summary>
        public ConsoleStatusBar()
        {
            Reset();
        }

        /// <summary>
        /// Append the given value to the current contents.
        /// </summary>
        /// <param name="value">Value to append.</param>
        public void Append(string value)
        {
            _contents.Append(value);
            Update();
        }

        /// <summary>
        /// Stores the given value in the bar.
        /// </summary>
        /// <param name="value">Value to store.</param>
        public void Set(string value)
        {
            _contents.Clear();
            Append(value);
        }

        /// <summary>
        /// Resets the contents of the status bar.
        /// </summary>
        public void Reset()
        {
            _contents.Clear();
            Update();
        }

        /// <summary>
        /// Whether or not the status bar is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        private void Update()
        {
            if (!Enabled)
            {
                return;
            }

            Write(_contents.ToString());
        }

        private static void Write(string value)
        {
            var console = BasicConsole.Default;

            var x = console.CursorLeft;
            var y = console.CursorTop;
            var fgColor = console.ForegroundColor;
            var bgColor = console.BackgroundColor;

            try
            {
                console.SetCursorPosition(0, 0);
                console.ForegroundColor = ForegroundColor;
                console.BackgroundColor = BackgroundColor;

                if (value.Length < console.BufferWidth)
                {
                    value += new string(' ', console.BufferWidth - value.Length);
                }

                console.Write(value);
            }
            finally
            {
                console.ForegroundColor = fgColor;
                console.BackgroundColor = bgColor;
                console.SetCursorPosition(x, y);
            }
        }
    }
}
