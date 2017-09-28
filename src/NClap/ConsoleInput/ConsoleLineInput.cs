using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NClap.Utilities;

namespace NClap.ConsoleInput
{
    /// <summary>
    /// Encapsulates logic for advanced console line input.
    /// </summary>
    internal class ConsoleLineInput : IConsoleLineInput
    {
        // Completion cache
        private TokenCompletionSet _lastCompletions;
        private CircularEnumerator<string> _completionEnumerator;

        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <param name="consoleOutput">Interface for interacting with the output
        /// console.</param>
        /// <param name="buffer">Console input buffer to use.</param>
        /// <param name="history">Console history object to use.</param>
        /// <param name="completionHandler">Optionally provides completion
        /// handler.</param>
        public ConsoleLineInput(IConsoleOutput consoleOutput, IConsoleInputBuffer buffer, IConsoleHistory history, ConsoleCompletionHandler completionHandler)
        {
            ConsoleOutput = consoleOutput;
            Buffer = buffer;
            History = history;
            CompletionHandler = completionHandler;
        }

        /// <summary>
        /// The object's output console.
        /// </summary>
        public IConsoleOutput ConsoleOutput { get; }

        /// <summary>
        /// The object's completion handler.
        /// </summary>
        public ConsoleCompletionHandler CompletionHandler { get; }

        /// <summary>
        /// The object's history.
        /// </summary>
        public IConsoleHistory History { get; }

        /// <summary>
        /// The object's buffer.
        /// </summary>
        public IConsoleInputBuffer Buffer { get; }

        /// <summary>
        /// True if insertion mode is enabled; false otherwise.
        /// </summary>
        public bool InsertMode { get; set; } = true;

        /// <summary>
        /// The string to be displayed when prompting for input.
        /// </summary>
        public ColoredString Prompt { get; set; } = string.Empty;

        /// <summary>
        /// The current contents of the buffer.
        /// </summary>
        public string Contents => Buffer.Contents;

        /// <summary>
        /// The current contents of the paste buffer.
        /// </summary>
        public string PasteBuffer { get; private set; }

        /// <summary>
        /// True if the cursor is at the end of the buffer; false otherwise.
        /// </summary>
        public bool AtEnd => Buffer.CursorIsAtEnd;

        /// <summary>
        /// Move the cursor backward by the specified number of characters.
        /// </summary>
        /// <param name="count">Number of characters to move backward.</param>
        /// <returns>True if the move could be made; false if the requested move
        /// was invalid.</returns>
        public bool MoveCursorBackward(int count) => MoveConsoleAndBufferCursors(SeekOrigin.Current, -1 * count);

        /// <summary>
        /// Move the cursor forward by the specified number of characters.
        /// </summary>
        /// <param name="count">Number of characters to move forward.</param>
        /// <returns>True if the move could be made; false if the requested move
        /// was invalid.</returns>
        public bool MoveCursorForward(int count) => MoveConsoleAndBufferCursors(SeekOrigin.Current, count);

        /// <summary>
        /// Move the cursor to the start of the input buffer.
        /// </summary>
        public void MoveCursorToStart() => MoveConsoleAndBufferCursors(SeekOrigin.Begin, 0);

        /// <summary>
        /// Move the cursor to the end of the input buffer.
        /// </summary>
        public void MoveCursorToEnd() => MoveConsoleAndBufferCursors(SeekOrigin.End, 0);

        /// <summary>
        /// Move the cursor back one word.
        /// </summary>
        public void MoveCursorBackwardOneWord() => MoveConsoleAndBufferCursors(SeekOrigin.Begin, FindIndexOfLastWord());

        /// <summary>
        /// Move the cursor forward one word.
        /// </summary>
        public void MoveCursorForwardOneWord() => MoveConsoleAndBufferCursors(SeekOrigin.Begin, FindIndexOfNextWord());

        /// <summary>
        /// Delete the character under the cursor.
        /// </summary>
        public void Delete()
        {
            if (Buffer.Remove())
            {
                SyncBufferToConsole(Buffer.CursorIndex, Buffer.Length - Buffer.CursorIndex, 1);
            }
        }

        /// <summary>
        /// Delete the character before the cursor.
        /// </summary>
        public void DeletePrecedingChar()
        {
            if (!Buffer.RemoveCharBeforeCursor())
            {
                return;
            }

            MoveConsoleCursorBackward();
            SyncBufferToConsole(Buffer.CursorIndex, Buffer.Length - Buffer.CursorIndex, 1);
        }

        /// <summary>
        /// Delete the characters from the beginning of the last word in the
        /// buffer and the current cursor.
        /// </summary>
        public void DeleteBackwardThroughLastWord()
        {
            var index = FindIndexOfLastWord();
            var originalCursorIndex = Buffer.CursorIndex;

            MoveConsoleAndBufferCursors(SeekOrigin.Begin, index);
            var removeCount = originalCursorIndex - Buffer.CursorIndex;

            Buffer.Remove(removeCount);
            SyncBufferToConsole(Buffer.CursorIndex, Buffer.Length - Buffer.CursorIndex, removeCount);
        }

        /// <summary>
        /// Delete the characters from the current cursor up to, but not
        /// including, the start of the next word.
        /// </summary>
        public void DeleteForwardToNextWord()
        {
            var index = FindIndexOfNextWord();
            var originalCursorIndex = Buffer.CursorIndex;
            var removeCount = index - originalCursorIndex;

            Buffer.Remove(removeCount);
            SyncBufferToConsole(Buffer.CursorIndex, Buffer.Length - Buffer.CursorIndex, removeCount);
        }

        /// <summary>
        /// Clear the input buffer and reset the cursor to the beginning of the
        /// buffer.
        /// </summary>
        /// <param name="clearBufferOnly">True to clear the buffer only; false
        /// to clear the buffer and reset the cursor.</param>
        public void ClearLine(bool clearBufferOnly)
        {
            if (!clearBufferOnly)
            {
                MoveConsoleCursorBackward(Buffer.CursorIndex);
                SyncBufferToConsole(0, 0, Buffer.Length);
            }

            Buffer.Clear();
        }

        /// <summary>
        /// Clear the screen buffer, and redisplay the prompt and current
        /// contents of the input buffer.
        /// </summary>
        public void ClearScreen()
        {
            ConsoleOutput.Clear();
            ConsoleOutput.SetCursorPosition(0, 0);
            DisplayInputLine();
        }

        /// <summary>
        /// Insert a character at the cursor without moving the cursor.
        /// </summary>
        /// <param name="value">The character to insert.</param>
        public void Insert(char value)
        {
            var originalCursorIndex = Buffer.CursorIndex;

            Buffer.Insert(value);
            SyncBufferToConsole(originalCursorIndex, Buffer.Length - originalCursorIndex);
        }

        /// <summary>
        /// Insert a string at the cursor without moving the cursor.
        /// </summary>
        /// <param name="value">The string to insert.</param>
        public void Insert(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var originalCursorIndex = Buffer.CursorIndex;
            var originalLength = Buffer.Length;

            Buffer.Insert(value);
            SyncBufferToConsole(
                originalCursorIndex,
                value.Length + (originalLength - originalCursorIndex));
        }

        /// <summary>
        /// Replace the character under the cursor.
        /// </summary>
        /// <param name="value">The character to place.</param>
        public void Replace(char value)
        {
            var originalCursorIndex = Buffer.CursorIndex;

            Buffer.Replace(value);
            SyncBufferToConsole(originalCursorIndex, 1);
        }

        /// <summary>
        /// Replace the entire contents of the input buffer with the previous
        /// line in the input history.
        /// </summary>
        public void ReplaceWithLastLineInHistory() =>
            ReplaceWithLineInHistory(SeekOrigin.Current, -1);

        /// <summary>
        /// Replace the entire contents of the input buffer with the next
        /// line in the input history.
        /// </summary>
        public void ReplaceWithNextLineInHistory() =>
            ReplaceWithLineInHistory(SeekOrigin.Current, 1);

        /// <summary>
        /// Replace the entire contents of the input buffer with the oldest
        /// line in the input history.
        /// </summary>
        public void ReplaceWithOldestLineInHistory() =>
            ReplaceWithLineInHistory(SeekOrigin.Begin, 0);

        /// <summary>
        /// Replace the entire contents of the input buffer with the youngest
        /// line in the input history.
        /// </summary>
        public void ReplaceWithYoungestLineInHistory() =>
            ReplaceWithLineInHistory(SeekOrigin.End, -1);

        /// <summary>
        /// Save the contents of the current buffer to the input history.
        /// </summary>
        public void SaveToHistory() => History.Add(Contents);

        /// <summary>
        /// Remove the contents of the buffer from the cursor to the end of the
        /// buffer, and place them in the paste buffer.
        /// </summary>
        public void CutToEnd()
        {
            var charsToCut = Buffer.Length - Buffer.CursorIndex;

            PasteBuffer = new string(Buffer.Read(charsToCut));
            SyncBufferToConsole(0, 0, charsToCut);
            Buffer.Truncate();
        }

        /// <summary>
        /// Insert the contents of the paste buffer at the cursor and move the
        /// cursor to the end of the pasted characters.
        /// </summary>
        public void Paste()
        {
            if (PasteBuffer == null)
            {
                return;
            }

            Insert(PasteBuffer);
            MoveConsoleAndBufferCursors(SeekOrigin.Current, PasteBuffer.Length);
        }

        /// <summary>
        /// Replace the current token in the input buffer with the previous
        /// completion.
        /// </summary>
        /// <param name="lastOperationWasCompletion">True if the last input
        /// operation was a completion operation; false otherwise.</param>
        public void ReplaceCurrentTokenWithPreviousCompletion(bool lastOperationWasCompletion) =>
            ReplaceCurrentTokenWithCompletion(true, lastOperationWasCompletion);

        /// <summary>
        /// Replace the current token in the input buffer with the next
        /// completion.
        /// </summary>
        /// <param name="lastOperationWasCompletion">True if the last input
        /// operation was a completion operation; false otherwise.</param>
        public void ReplaceCurrentTokenWithNextCompletion(bool lastOperationWasCompletion) =>
            ReplaceCurrentTokenWithCompletion(false, lastOperationWasCompletion);

        /// <summary>
        /// Replace the current token in the input buffer with all possible
        /// completions for it.
        /// </summary>
        public void ReplaceCurrentTokenWithAllCompletions()
        {
            if (CompletionHandler == null)
            {
                return;
            }

            var completions = TokenCompletionSet.Create(Buffer.Contents, Buffer.CursorIndex, CompletionHandler);
            if (completions.Empty)
            {
                return;
            }

            var replacementText = string.Concat(completions.Completions.Select(completion => completion + " "));
            var tokenStartIndex = completions.OriginalToken.InnerStartingOffset;
            var tokenLength = completions.OriginalToken.InnerLength;

            MoveConsoleAndBufferCursors(SeekOrigin.Begin, tokenStartIndex);
            Buffer.Remove(tokenLength);
            Buffer.Insert(replacementText);

            SyncBufferToConsole(tokenStartIndex, replacementText.Length);
            MoveConsoleAndBufferCursors(SeekOrigin.Begin, tokenStartIndex + replacementText.Length);
        }

        /// <summary>
        /// Displays all completions without modifying the input buffer.
        /// </summary>
        public void DisplayAllCompletions()
        {
            if (CompletionHandler == null)
            {
                return;
            }

            var completions = TokenCompletionSet.Create(Buffer.Contents, Buffer.CursorIndex, CompletionHandler);
            if (completions.Empty)
            {
                return;
            }

            MoveConsoleCursorToNextLine();
            DisplayInColumns(completions.Completions);
            DisplayInputLine();
        }

        /// <summary>
        /// Transforms the current word by passing it through the provided
        /// function.  Does not move the cursor.
        /// </summary>
        /// <param name="transformation">Function to apply.</param>
        public void TransformCurrentWord(Func<string, string> transformation)
        {
            if (transformation == null)
            {
                throw new ArgumentNullException(nameof(transformation));
            }

            var word = new string(Buffer.Read(FindIndexOfNextWord() - Buffer.CursorIndex));
            var transformedWord = transformation(word);

            Buffer.Remove(word.Length);
            Buffer.Insert(transformedWord);

            var extraSpaces = 0;
            if (transformedWord.Length < word.Length)
            {
                extraSpaces = word.Length - transformedWord.Length;
            }

            SyncBufferToConsole(Buffer.CursorIndex, Buffer.Length - Buffer.CursorIndex, extraSpaces);
        }

        /// <summary>
        /// Displays the input prompt.
        /// </summary>
        public void DisplayPrompt()
        {
            if (!string.IsNullOrEmpty(Prompt))
            {
                ConsoleOutput.Write(Prompt);
            }
        }

        private void DisplayInputLine()
        {
            DisplayPrompt();
            SyncBufferToConsole(0, Buffer.Length);
            MoveConsoleCursorForward(Buffer.CursorIndex);
        }

        internal void DisplayInColumns(IReadOnlyList<string> values)
        {
            ConsoleOutput.Write(StringUtilities.FormatInColumns(values, ConsoleOutput.BufferWidth));
        }

        private void ReplaceCurrentTokenWithCompletion(bool reverseOrder, bool lastOperationWasCompletion)
        {
            if (CompletionHandler == null)
            {
                return;
            }

            // If we can't pull a completion from the cache, then generate new ones.
            if (!lastOperationWasCompletion || (_lastCompletions == null))
            {
                _lastCompletions = TokenCompletionSet.Create(Buffer.Contents, Buffer.CursorIndex, CompletionHandler);
                _completionEnumerator = CircularEnumerator.Create(_lastCompletions.Completions);
            }

            // Bail if no completions are available.
            if (_lastCompletions.Empty)
            {
                return;
            }

            // Find the existing token length.
            var existingTokenLength = _completionEnumerator.Started
                ? _completionEnumerator.CurrentItem.Length
                : _lastCompletions.OriginalToken.InnerLength;

            // Find the existing token start.
            var existingTokenStart =
                _lastCompletions.OriginalToken.InnerStartingOffset;

            // Select the new completion.
            if (reverseOrder)
            {
                _completionEnumerator.MovePrevious();
            }
            else
            {
                _completionEnumerator.MoveNext();
            }

            // Select the completion.
            var completion = _completionEnumerator.CurrentItem;

            // Replace the current token in the buffer with the completion.
            MoveConsoleAndBufferCursors(SeekOrigin.Begin, existingTokenStart);
            Buffer.Remove(existingTokenLength);
            Buffer.Insert(completion);

            // Rewrite the input text.
            SyncBufferToConsole(
                existingTokenStart,
                Buffer.Length - existingTokenStart,
                (existingTokenLength > completion.Length) ? existingTokenLength - completion.Length : 0);

            MoveConsoleAndBufferCursors(SeekOrigin.Current, completion.Length);
        }

        private int FindIndexOfLastWord()
        {
            var index = Buffer.CursorIndex;
            if (index <= 0)
            {
                return index;
            }

            while ((index > 0) && char.IsWhiteSpace(Buffer[index - 1]))
            {
                --index;
            }

            while ((index > 0) && !char.IsWhiteSpace(Buffer[index - 1]))
            {
                --index;
            }

            return index;
        }

        private int FindIndexOfNextWord()
        {
            var index = Buffer.CursorIndex;
            if (index >= Buffer.Length)
            {
                return index;
            }

            while ((index < Buffer.Length) && char.IsWhiteSpace(Buffer[index]))
            {
                ++index;
            }

            while ((index < Buffer.Length) && !char.IsWhiteSpace(Buffer[index]))
            {
                ++index;
            }

            return index;
        }

        private bool MoveConsoleAndBufferCursors(SeekOrigin origin, int deltaFromOrigin)
        {
            var result = false;

            int movementDelta;
            if (Buffer.MoveCursor(origin, deltaFromOrigin, out movementDelta))
            {
                result = MoveConsoleCursor(movementDelta);
                if (!result)
                {
                    Buffer.MoveCursor(SeekOrigin.Current, movementDelta * -1);
                }
            }

            return result;
        }

        private void MoveConsoleCursorToNextLine()
        {
            ConsoleOutput.WriteLine(string.Empty);
        }

        private bool MoveConsoleCursor(int offset)
        {
            return offset < 0
                ? MoveConsoleCursorBackward(-1 * offset)
                : MoveConsoleCursorForward(offset);
        }

        private bool MoveConsoleCursorForward(int count = 1)
        {
            Debug.Assert(count >= 0);

            var cursorLeft = ConsoleOutput.CursorLeft;
            var cursorTop = ConsoleOutput.CursorTop;
            var bufferWidth = ConsoleOutput.BufferWidth;
            var bufferHeight = ConsoleOutput.BufferHeight;

            cursorLeft += count;

            if (cursorLeft >= bufferWidth)
            {
                var lineDelta = cursorLeft / bufferWidth;
                cursorTop += lineDelta;
                cursorLeft -= lineDelta * bufferWidth;
            }

            Debug.Assert(cursorLeft >= 0);
            Debug.Assert(cursorLeft < bufferWidth);

            if (cursorTop >= bufferHeight)
            {
                return false;
            }

            return ConsoleOutput.SetCursorPosition(cursorLeft, cursorTop);
        }

        private bool MoveConsoleCursorBackward(int count = 1)
        {
            Debug.Assert(count >= 0);

            var cursorLeft = ConsoleOutput.CursorLeft;
            var cursorTop = ConsoleOutput.CursorTop;
            var bufferWidth = ConsoleOutput.BufferWidth;

            cursorLeft -= count;

            if (cursorLeft < 0)
            {
                var lineDelta = cursorLeft / bufferWidth;
                cursorTop += lineDelta;
                cursorLeft -= lineDelta * bufferWidth;

                if (cursorLeft < 0)
                {
                    --cursorTop;
                    cursorLeft += bufferWidth;
                }
            }

            Debug.Assert(cursorLeft >= 0);
            Debug.Assert(cursorLeft < bufferWidth);

            if (cursorTop < 0)
            {
                return false;
            }

            return ConsoleOutput.SetCursorPosition(cursorLeft, cursorTop);
        }

        private void SyncBufferToConsole(int startIndex, int length, int extraSpaces = 0)
        {
            // Snap info about the buffer size.
            var bufferHeight = ConsoleOutput.BufferHeight;
            var bufferWidth = ConsoleOutput.BufferWidth;
            var bufferSize = bufferHeight * bufferWidth;

            // Snap info about the console state.
            var cursorLeft = ConsoleOutput.CursorLeft;
            var cursorTop = ConsoleOutput.CursorTop;
            var cursorOffset = cursorTop * bufferWidth + cursorLeft;

            // If the length is bigger than the buffer itself...
            if (length + extraSpaces > bufferSize)
            {
                throw new NotImplementedException();
            }

            // If there's not enough room left in the buffer, then scroll the
            // contents of the buffer first.
            if (cursorOffset + length + extraSpaces >= bufferSize)
            {
                var spillOverLength = cursorOffset + length + extraSpaces - (bufferSize - 1);
                var spillOverLines = spillOverLength / bufferWidth;

                if (spillOverLength % bufferWidth != 0)
                {
                    ++spillOverLines;
                }

                ConsoleOutput.ScrollContents(spillOverLines);

                // Update console state.
                cursorLeft = ConsoleOutput.CursorLeft;
                cursorTop = ConsoleOutput.CursorTop;
                cursorOffset = cursorTop * bufferWidth + cursorLeft;
            }

            // Allocate a fresh buffer so we can concatenate some chars on the
            // end.
            var printBuffer = new char[length + extraSpaces];
            Buffer.ReadAt(startIndex, printBuffer, 0, length);
            for (var i = 0; i < extraSpaces; i++)
            {
                printBuffer[length + i] = ' ';
            }

            // Write out the chars to the buffer and then revert the cursor
            // position.
            ConsoleOutput.Write(new string(printBuffer));
            ConsoleOutput.SetCursorPosition(cursorLeft, cursorTop);
        }

        private void ReplaceWithLineInHistory(SeekOrigin origin, int offset)
        {
            if (!History.MoveCursor(origin, offset))
            {
                return;
            }

            var historicLine = History.CurrentEntry;
            if (historicLine == null)
            {
                return;
            }

            var extraSpaces = 0;
            if (Buffer.Length > historicLine.Length)
            {
                extraSpaces = Buffer.Length - historicLine.Length;
            }

            MoveConsoleAndBufferCursors(SeekOrigin.Begin, 0);
            Buffer.Truncate();

            Buffer.Insert(historicLine);

            SyncBufferToConsole(0, Buffer.Length, extraSpaces);
            MoveConsoleAndBufferCursors(SeekOrigin.End, 0);
        }
    }
}
