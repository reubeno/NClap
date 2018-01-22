using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using NClap.Utilities;

namespace NClap.ConsoleInput
{
#pragma warning disable PC001 // TODO: API not supported on all platforms
#pragma warning disable PC003 // TODO: Native API not available in UWP

    /// <summary>
    /// Stock implementation of the IConsoleInput and IConsoleOutput interfaces.
    /// </summary>
    internal class BasicConsole : IConsoleInput, IConsoleOutput
    {
        private const int _defaultCursorSize = 100;
        private const bool _defaultCursorVisibility = true;
        private const int _defaultCursorLeft = 0;
        private const int _defaultCursorTop = 0;
        private const int _defaultWidth = 80;
        private const int _defaultHeight = 25;
        private const ConsoleColor _defaultForegroundColor = ConsoleColor.White;
        private const ConsoleColor _defaultBackgroundColor = ConsoleColor.Black;
        private const bool _defaultTreatControlCAsInput = false;

        private readonly PropertyWithSimulatedFallback<int> _cursorSize = CreateProperty(
            () => Console.CursorSize, value => Console.CursorSize = value,
            _defaultCursorSize,
            size => size >= 0 && size <= 100);

        private readonly PropertyWithSimulatedFallback<bool> _cursorVisible = CreateProperty(
            () => Console.CursorVisible, value => Console.CursorVisible = value,
            _defaultCursorVisibility);

        private readonly PropertyWithSimulatedFallback<int> _cursorLeft = CreateProperty(
            () => Console.CursorLeft, value => Console.CursorLeft = value,
            _defaultCursorLeft,
            left => left >= 0);

        private readonly PropertyWithSimulatedFallback<int> _cursorTop = CreateProperty(
            () => Console.CursorTop, value => Console.CursorTop = value,
            _defaultCursorTop,
            top => top >= 0);

        private readonly PropertyWithSimulatedFallback<int> _windowWidth = CreateProperty(
            () => Console.WindowWidth, value => Console.WindowWidth = value,
            _defaultWidth,
            width => width > 0);

        private readonly PropertyWithSimulatedFallback<int> _windowHeight = CreateProperty(
            () => Console.WindowHeight, value => Console.WindowHeight = value,
            _defaultHeight,
            height => height > 0);

        private readonly PropertyWithSimulatedFallback<int> _bufferWidth = CreateProperty(
            () => Console.BufferWidth, value => Console.BufferWidth = value,
            _defaultWidth,
            width => width > 0);

        private readonly PropertyWithSimulatedFallback<int> _bufferHeight = CreateProperty(
            () => Console.BufferHeight, value => Console.BufferHeight = value,
            _defaultHeight,
            height => height > 0);

        private readonly PropertyWithSimulatedFallback<ConsoleColor> _foregroundColor = CreateProperty(
            () => Console.ForegroundColor, value => Console.ForegroundColor = value,
            _defaultForegroundColor,
            color => color >= ConsoleColor.Black && color <= ConsoleColor.White);

        private readonly PropertyWithSimulatedFallback<ConsoleColor> _backgroundColor = CreateProperty(
            () => Console.BackgroundColor, value => Console.BackgroundColor = value,
            _defaultBackgroundColor,
            color => color >= ConsoleColor.Black && color <= ConsoleColor.White);

        private readonly PropertyWithSimulatedFallback<bool> _treatControlCAsInput = CreateProperty(
            () => Console.TreatControlCAsInput, value => Console.TreatControlCAsInput = value,
            _defaultTreatControlCAsInput);

        /// <summary>
        /// Dummy constructor, present to prevent outside callers from
        /// constructing an instance of this class.
        /// </summary>
        protected BasicConsole()
        {
        }

        /// <summary>
        /// Public factory method.
        /// </summary>
        /// <returns>A basic console instance.</returns>
        public static BasicConsole Default { get; } =
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? new Windows.WindowsConsole() : new BasicConsole();

        /// <summary>
        /// For testing purposes, exposes a base instance implementation of the
        /// console, independent of the running OS platform.
        /// </summary>
        internal static BasicConsole BaseInstance => new BasicConsole();

        /// <summary>
        /// The size of the cursor, expressed as an integral percentage.
        /// Note that this property is not faithfully or completely implemented
        /// on all platforms.
        /// </summary>
        public int CursorSize
        {
            get => _cursorSize.Value;
            set => _cursorSize.Value = value;
        }

        /// <summary>
        /// True if the cursor is visible; false otherwise. Note that this
        /// property is not faithfully or completely implemented on all
        /// platforms.
        /// </summary>
        public bool CursorVisible
        {
            get => _cursorVisible.Value;
            set => _cursorVisible.Value = value;
        }

        /// <summary>
        /// True if Control-C is treated as a normal input character; false if
        /// it's specially handled.
        /// </summary>
        public bool TreatControlCAsInput
        {
            get => _treatControlCAsInput.Value;
            set => _treatControlCAsInput.Value = value;
        }

        /// <summary>
        /// The x-coordinate of the input cursor.
        /// </summary>
        public int CursorLeft
        {
            get => _cursorLeft.Value;
            set => _cursorLeft.Value = value;
        }

        /// <summary>
        /// The y-coordinate of the input cursor.
        /// </summary>
        public int CursorTop
        {
            get => _cursorTop.Value;
            set => _cursorTop.Value = value;
        }

        /// <summary>
        /// The width, in characters, of the window associated with the
        /// console.
        /// </summary>
        public int WindowWidth
        {
            get => _windowWidth.Value;
            set => _windowWidth.Value = value;
        }

        /// <summary>
        /// The width, in height, of the window associated with the
        /// console.
        /// </summary>
        public int WindowHeight
        {
            get => _windowHeight.Value;
            set => _windowHeight.Value = value;
        }

        /// <summary>
        /// The width, in characters, of the logical buffer associated with the
        /// console.
        /// </summary>
        public int BufferWidth
        {
            get => _bufferWidth.Value;
            set => _bufferWidth.Value = value;
        }

        /// <summary>
        /// The height, in characters, of the logical buffer associated with the
        /// console.
        /// </summary>
        public int BufferHeight
        {
            get => _bufferHeight.Value;
            set => _bufferHeight.Value = value;
        }

        /// <summary>
        /// The console's foreground color.
        /// </summary>
        public ConsoleColor ForegroundColor
        {
            get => _foregroundColor.Value;
            set => _foregroundColor.Value = value;
        }

        /// <summary>
        /// The console's background color.
        /// </summary>
        public ConsoleColor BackgroundColor
        {
            get => _backgroundColor.Value;
            set => _backgroundColor.Value = value;
        }

        /// <summary>
        /// Reads a key press from the console.
        /// </summary>
        /// <param name="suppressEcho">True to suppress auto-echoing the key's
        /// character; false to echo it as normal.</param>
        /// <returns>Info about the press.</returns>
        public ConsoleKeyInfo ReadKey(bool suppressEcho) => Console.ReadKey(suppressEcho);

        /// <summary>
        /// Moves the cursor to the specified position.
        /// </summary>
        /// <param name="left">The new x-coordinate.</param>
        /// <param name="top">The new y-coordinate.</param>
        /// <returns>True if the move could be made; false if the requested
        /// move was invalid.</returns>
        public bool SetCursorPosition(int left, int top)
        {
            if (left < 0) return false;
            if (top < 0) return false;
            if (left >= WindowWidth) return false;
            if (top >= WindowHeight) return false;

            try
            {
                Console.SetCursorPosition(left, top);
            }
            catch (Exception ex) when (IsExceptionAcceptable(ex))
            {
            }

            return true;
        }

        /// <summary>
        /// Indicates if the console's buffer is scrollable.
        /// </summary>
        public virtual bool IsScrollable => false;

        /// <summary>
        /// Scrolls the bottom-most lines of the console's buffer upward within
        /// the buffer by the specified number of lines, effectively freeing up
        /// the specified number of lines.  The cursor is adjusted appropriately
        /// upward by the same number of lines.
        /// </summary>
        /// <param name="lineCount">The number of lines by which to scroll the
        /// contents.</param>
        /// <exception cref="Win32Exception">Thrown when an internal error
        /// occurs.</exception>
        /// <exception cref="NotSupportedException">Thrown when scrolling is not
        /// supported.</exception>
        public virtual void ScrollContents(int lineCount) => throw new NotSupportedException();

        /// <summary>
        /// Clears the console without moving the cursor.
        /// </summary>
        public void Clear()
        {
            try
            {
                Console.Clear();
            }
            catch (Exception ex) when (IsExceptionAcceptable(ex))
            {
            }
        }

        /// <summary>
        /// Writes colored text to the console.
        /// </summary>
        /// <param name="text">The text to write.</param>
        public void Write(ColoredMultistring text)
        {
            foreach (var value in text.Content)
            {
                Write(value);
            }
        }

        /// <summary>
        /// Writes colored text to the console.
        /// </summary>
        /// <param name="text">The text to write.</param>
        public void Write(ColoredString text)
        {
            if (!text.ForegroundColor.HasValue && !text.BackgroundColor.HasValue)
            {
                Write(text.Content);
                return;
            }

            var originalForegroundColor = ForegroundColor;
            var originalBackgroundColor = BackgroundColor;

            try
            {
                if (text.ForegroundColor.HasValue)
                {
                    ForegroundColor = text.ForegroundColor.Value;
                }

                if (text.BackgroundColor.HasValue)
                {
                    BackgroundColor = text.BackgroundColor.Value;
                }

                Write(text.Content);
            }
            finally
            {
                ForegroundColor = originalForegroundColor;
                BackgroundColor = originalBackgroundColor;
            }
        }

        /// <summary>
        /// Writes text to the console.
        /// </summary>
        /// <param name="text">The text to write.</param>
        public void Write(string text) => Console.Write(text);

        /// <summary>
        /// Writes a line of text to the console.
        /// </summary>
        /// <param name="text">The line of text to write.</param>
        public void WriteLine(string text) => Console.WriteLine(text);

        private static bool IsExceptionAcceptable(Exception ex) =>
            ex is PlatformNotSupportedException || ex is IOException;

        private static PropertyWithSimulatedFallback<T> CreateProperty<T>(Func<T> getter, Action<T> setter, T initialFallbackValue, Predicate<T> validator = null) =>
            new PropertyWithSimulatedFallback<T>(
                getter,
                setter,
                IsExceptionAcceptable,
                initialFallbackValue,
                validator);
    }
}
