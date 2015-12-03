using System;
using System.Collections.Generic;
using System.Globalization;

namespace NClap.ConsoleInput
{
    delegate IEnumerable<string> ConsoleCompletionHandler(IEnumerable<string> tokens, int tokenIndex);

    /// <summary>
    /// Encapsulates logic for an advanced console reader.
    /// </summary>
    internal class ConsoleReader : IConsoleReader
    {
        private readonly int _defaultCursorSize;

        // Operation history
        private ConsoleInputOperation? _lastOp;

        /// <summary>
        /// Primary constructor.
        /// </summary>
        /// <param name="lineInput">Line input engine.</param>
        /// <param name="consoleInput">Interface for interacting with the input
        /// console; a default implementation is used if this parameter is null.
        /// </param>
        /// <param name="consoleOutput">Interface for interacting with the output
        /// console; a default implementation is used if this parameter is null.
        /// </param>
        /// <param name="keyBindingSet">The key bindings to use in the reader.
        /// Default bindings are used if this parameter is null.</param>
        public ConsoleReader(IConsoleLineInput lineInput, IConsoleInput consoleInput = null, IConsoleOutput consoleOutput = null, IReadOnlyConsoleKeyBindingSet keyBindingSet = null)
        {
            if (lineInput == null)
            {
                throw new ArgumentNullException(nameof(lineInput));
            }

            LineInput = lineInput;
            ConsoleInput = consoleInput ?? BasicConsoleInputAndOutput.Default;
            ConsoleOutput = consoleOutput ?? BasicConsoleInputAndOutput.Default;
            KeyBindingSet = keyBindingSet ?? ConsoleKeyBindingSet.Default;

            _defaultCursorSize = ConsoleOutput.CursorSize;
        }

        /// <summary>
        /// The console being used for input.
        /// </summary>
        public IConsoleInput ConsoleInput { get; }

        /// <summary>
        /// The console being used for output.
        /// </summary>
        public IConsoleOutput ConsoleOutput { get; }

        /// <summary>
        /// The inner line input object.
        /// </summary>
        public IConsoleLineInput LineInput { get; }

        /// <summary>
        /// The console key bindings used by this console reader.
        /// </summary>
        public IReadOnlyConsoleKeyBindingSet KeyBindingSet { get; }

        /// <summary>
        /// The beginning-of-line comment character.
        /// </summary>
        public char? CommentCharacter { get; set; } = null;

        /// <summary>
        /// Reads a line of input text from the underlying console.
        /// </summary>
        /// <returns>The line of text, or null if the end of input was
        /// encountered.</returns>
        public string ReadLine()
        {
            var cursorWasVisible = ConsoleOutput.CursorVisible;
            var ctrlCWasInput = ConsoleInput.TreatControlCAsInput;

            try
            {
                ConsoleInputOperationResult consoleKeyResult;

                ConsoleOutput.CursorVisible = true;
                ConsoleInput.TreatControlCAsInput = true;

                UpdateCursorSize();

                do
                {
                    // Grab a new key press event.
                    var key = ConsoleInput.ReadKey(true);

#if VERBOSE
                    // Log information about the key press.
                    LogKey(key);
#endif

                    // Process the event.
                    try
                    {
                        consoleKeyResult = ProcessKey(key);
                    }
                    catch (NotImplementedException)
                    {
                        consoleKeyResult = ConsoleInputOperationResult.Normal;
                    }
                }
                while (consoleKeyResult == ConsoleInputOperationResult.Normal);

                switch (consoleKeyResult)
                {
                    case ConsoleInputOperationResult.EndOfInputStream:
                        return null;

                    case ConsoleInputOperationResult.EndOfInputLine:
                        ConsoleOutput.WriteLine(string.Empty);
                        break;
                }

                var result = LineInput.Contents;

                // Add to history.
                LineInput.SaveToHistory();

                // Reset temporary state
                LineInput.ClearLine(true);

                return result;
            }
            finally
            {
                ConsoleOutput.CursorVisible = cursorWasVisible;
                ConsoleInput.TreatControlCAsInput = ctrlCWasInput;
                ConsoleOutput.CursorSize = _defaultCursorSize;
            }
        }

        internal ConsoleInputOperationResult ProcessKey(ConsoleKeyInfo key)
        {
            ConsoleInputOperation op;
            if (!KeyBindingSet.TryGetValue(key, out op))
            {
                op = ConsoleInputOperation.Char;
            }

            var result = Process(op, key);

            _lastOp = op;

            return result;
        }

        internal ConsoleInputOperationResult Process(ConsoleInputOperation op, ConsoleKeyInfo key)
        {
            var result = ConsoleInputOperationResult.Normal;

            switch (op)
            {
                case ConsoleInputOperation.NoOp:
                    break;

                case ConsoleInputOperation.Char:
                    ProcessCharacterKey(key.KeyChar);
                    break;

                //
                // Standard readline operations.
                //

                case ConsoleInputOperation.AcceptLine:
                    result = ConsoleInputOperationResult.EndOfInputLine;
                    break;
                case ConsoleInputOperation.EndOfFile:
                    if (string.IsNullOrEmpty(LineInput.Contents))
                    {
                        ConsoleOutput.WriteLine(string.Empty);
                        result = ConsoleInputOperationResult.EndOfInputStream;
                    }
                    break;
                case ConsoleInputOperation.BeginningOfLine:
                    LineInput.MoveCursorToStart();
                    break;
                case ConsoleInputOperation.EndOfLine:
                    LineInput.MoveCursorToEnd();
                    break;
                case ConsoleInputOperation.ForwardChar:
                    LineInput.MoveCursorForward(1);
                    break;
                case ConsoleInputOperation.BackwardChar:
                    LineInput.MoveCursorBackward(1);
                    break;
                case ConsoleInputOperation.ClearScreen:
                    LineInput.ClearScreen();
                    break;
                case ConsoleInputOperation.PreviousHistory:
                    LineInput.ReplaceWithLastLineInHistory();
                    break;
                case ConsoleInputOperation.NextHistory:
                    LineInput.ReplaceWithNextLineInHistory();
                    break;
                case ConsoleInputOperation.KillLine:
                    LineInput.CutToEnd();
                    break;
                case ConsoleInputOperation.UnixWordRubout:
                    LineInput.DeleteBackwardThroughLastWord();
                    break;
                case ConsoleInputOperation.Yank:
                    LineInput.Paste();
                    break;
                case ConsoleInputOperation.Abort:
                    LineInput.ClearLine(true);
                    result = ConsoleInputOperationResult.EndOfInputLine;
                    break;
                case ConsoleInputOperation.ForwardWord:
                    LineInput.MoveCursorForwardOneWord();
                    break;
                case ConsoleInputOperation.BackwardWord:
                    LineInput.MoveCursorBackwardOneWord();
                    break;
                case ConsoleInputOperation.BeginningOfHistory:
                    LineInput.ReplaceWithOldestLineInHistory();
                    break;
                case ConsoleInputOperation.EndOfHistory:
                    LineInput.ReplaceWithYoungestLineInHistory();
                    break;
                case ConsoleInputOperation.UpcaseWord:
                    LineInput.TransformCurrentWord(word => word.ToUpper(CultureInfo.CurrentCulture));
                    break;
                case ConsoleInputOperation.DowncaseWord:
                    LineInput.TransformCurrentWord(word => word.ToLower(CultureInfo.CurrentCulture));
                    break;
                case ConsoleInputOperation.CapitalizeWord:
                    LineInput.TransformCurrentWord(Capitalize);
                    break;
                case ConsoleInputOperation.KillWord:
                    LineInput.DeleteForwardToNextWord();
                    break;
                case ConsoleInputOperation.PossibleCompletions:
                    LineInput.DisplayAllCompletions();
                    break;
                case ConsoleInputOperation.InsertCompletions:
                    LineInput.ReplaceCurrentTokenWithAllCompletions();
                    break;
                case ConsoleInputOperation.RevertLine:
                    LineInput.ClearLine(false);
                    break;
                case ConsoleInputOperation.InsertComment:
                    if (CommentCharacter.HasValue)
                    {
                        LineInput.MoveCursorToStart();
                        LineInput.Insert(CommentCharacter.Value);

                        return ConsoleInputOperationResult.EndOfInputLine;
                    }
                    break;
                case ConsoleInputOperation.TabInsert:
                    ProcessCharacterKey('\t');
                    break;
                case ConsoleInputOperation.BackwardKillWord:
                    LineInput.DeleteBackwardThroughLastWord();
                    break;

                //
                // Extensions:
                //

                case ConsoleInputOperation.CompleteTokenNext:
                {
                    var lastOpWasCompleteToken =
                        _lastOp.HasValue &&
                        ((_lastOp.Value == ConsoleInputOperation.CompleteTokenNext) ||
                            (_lastOp.Value == ConsoleInputOperation.CompleteTokenPrevious));

                    ProcessTabKeyPress(false, lastOpWasCompleteToken);
                    break;
                }

                case ConsoleInputOperation.CompleteTokenPrevious:
                {
                    var lastOpWasCompleteToken =
                        _lastOp.HasValue &&
                        ((_lastOp.Value == ConsoleInputOperation.CompleteTokenNext) ||
                            (_lastOp.Value == ConsoleInputOperation.CompleteTokenPrevious));

                    ProcessTabKeyPress(true, lastOpWasCompleteToken);
                    break;
                }

                case ConsoleInputOperation.DeletePreviousChar:
                    LineInput.DeletePrecedingChar();
                    break;
                case ConsoleInputOperation.DeleteChar:
                    LineInput.Delete();
                    break;
                case ConsoleInputOperation.ToggleInsertMode:
                    LineInput.InsertMode = !LineInput.InsertMode;
                    UpdateCursorSize();
                    break;

                //
                // Unimplemented standard readline operations:
                //

                case ConsoleInputOperation.ReverseSearchHistory:
                    // TODO: implement ReverseSearchHistory
                case ConsoleInputOperation.ForwardSearchHistory:
                    // TODO: implement ForwardSearchHistory
                case ConsoleInputOperation.QuotedInsert:
                    // TODO: implement QuotedInsert
                case ConsoleInputOperation.TransposeChars:
                    // TODO: implement TransposeChars
                case ConsoleInputOperation.UnixLineDiscard:
                    // TODO: implement UnixLineDiscard
                case ConsoleInputOperation.Undo:
                    // TODO: implement Undo
                case ConsoleInputOperation.SetMark:
                    // TODO: implement SetMark
                case ConsoleInputOperation.CharacterSearch:
                    // TODO: implement CharacterSearch
                case ConsoleInputOperation.YankLastArg:
                    // TODO: implement YankLastArg
                case ConsoleInputOperation.NonIncrementalReverseSearchHistory:
                    // TODO: implement NonIncrementalReverseSearchHistory
                case ConsoleInputOperation.NonIncrementalForwardSearchHistory:
                    // TODO: implement NonIncrementalForwardSearchHistory
                case ConsoleInputOperation.YankPop:
                    // TODO: implement YankPop
                case ConsoleInputOperation.TildeExpand:
                    // TODO: implement TildeExpand
                case ConsoleInputOperation.YankNthArg:
                    // TODO: implement YankNthArg
                case ConsoleInputOperation.CharacterSearchBackward:
                    // TODO: implement CharacterSearchBackward
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(op));
            }

            return result;
        }

        internal static string Capitalize(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            var c = char.ToUpper(value[0], CultureInfo.CurrentCulture);
            return c + value.Substring(1).ToLower(CultureInfo.CurrentCulture);
        }

        private void ProcessCharacterKey(char value)
        {
            if (LineInput.InsertMode || LineInput.AtEnd)
            {
                LineInput.Insert(value);
            }
            else
            {
                LineInput.Replace(value);
            }

            LineInput.MoveCursorForward(1);
        }

        private void ProcessTabKeyPress(bool previous, bool lastOpWasCompleteToken)
        {
            if (previous)
            {
                LineInput.ReplaceCurrentTokenWithPreviousCompletion(lastOpWasCompleteToken);
            }
            else
            {
                LineInput.ReplaceCurrentTokenWithNextCompletion(lastOpWasCompleteToken);
            }
        }

        private void UpdateCursorSize() =>
            ConsoleOutput.CursorSize = LineInput.InsertMode ? _defaultCursorSize : 100;

#if false
        private void LogKey(ConsoleKeyInfo key)
        {
            var x = ConsoleOutput.CursorLeft;
            var y = ConsoleOutput.CursorTop;

            var fgColor = ConsoleOutput.ForegroundColor;
            var bgColor = ConsoleOutput.BackgroundColor;

            try
            {
                ConsoleOutput.SetCursorPosition(0, 0);
                ConsoleOutput.ForegroundColor = ConsoleColor.Yellow;
                ConsoleOutput.BackgroundColor = ConsoleColor.DarkBlue;

                var builder = new StringBuilder();
                builder.AppendFormat("[Key: {{{0, -12}}}] ", key.Key);

                if (char.IsControl(key.KeyChar))
                {
                    builder.AppendFormat("[Char: 0x{0:X}] ", (int)key.KeyChar);
                }
                else if (key.KeyChar != (char)0)
                {
                    builder.AppendFormat("[Char: '{0}' : 0x{1:X}] ", key.KeyChar, (int)key.KeyChar);
                }

                var modifiers = (ConsoleModifiers)0;
                var translationModifiers = (ConsoleModifiers)0;
                var modifierNames = new List<string>();

                if (key.Modifiers.HasFlag(ConsoleModifiers.Control))
                {
                    modifiers |= ConsoleModifiers.Control;
                    modifierNames.Add("Ctrl");
                }

                if (key.Modifiers.HasFlag(ConsoleModifiers.Alt))
                {
                    modifiers |= ConsoleModifiers.Alt;
                    modifierNames.Add("Alt");
                }

                if (key.Modifiers.HasFlag(ConsoleModifiers.Shift))
                {
                    modifiers |= ConsoleModifiers.Shift;
                    translationModifiers |= ConsoleModifiers.Shift;
                }

                var translatedToChars = false;
                if (modifiers.HasFlag(ConsoleModifiers.Alt) || modifiers.HasFlag(ConsoleModifiers.Control))
                {
                    var chars = InputUtilities.GetChars(key.Key, translationModifiers);
                    if (chars.Length > 0)
                    {
                        var charsAsString = new string(chars);
                        builder.AppendFormat("[{0}+{1}]", string.Join("+", modifierNames), charsAsString);
                        translatedToChars = true;
                    }
                }

                if (!translatedToChars)
                {
                    if (key.Modifiers.HasFlag(ConsoleModifiers.Shift)) modifierNames.Add("Shift");

                    if (modifierNames.Count > 0)
                    {
                        builder.AppendFormat("[{0}+{1}]", string.Join("+", modifierNames), key.Key);
                    }
                }

                if (builder.Length < ConsoleOutput.BufferWidth)
                {
                    builder.Append(new string(' ', ConsoleOutput.BufferWidth - builder.Length));
                }

                ConsoleOutput.Write(builder.ToString());
            }
            finally
            {
                ConsoleOutput.ForegroundColor = fgColor;
                ConsoleOutput.BackgroundColor = bgColor;
                ConsoleOutput.SetCursorPosition(x, y);
            }
        }
#endif
    }
}
