using System;

using NClap.Utilities;

namespace NClap.ConsoleInput
{
    /// <summary>
    /// Abstract interface for interacting with an object that manages
    /// console line input.
    /// </summary>
    public interface IConsoleLineInput
    {
        /// <summary>
        /// True if insertion mode is enabled; false otherwise.
        /// </summary>
        bool InsertMode { get; set; }

        /// <summary>
        /// The string to be displayed when prompting for input.
        /// </summary>
        ColoredString Prompt { get; set; }

        /// <summary>
        /// The current contents of the buffer.
        /// </summary>
        string Contents { get; }

        /// <summary>
        /// The current contents of the paste buffer.
        /// </summary>
        string PasteBuffer { get; }

        /// <summary>
        /// True if the cursor is at the end of the buffer; false otherwise.
        /// </summary>
        bool AtEnd { get; }

        /// <summary>
        /// Move the cursor backward by the specified number of characters.
        /// </summary>
        /// <param name="count">Number of characters to move backward.</param>
        /// <returns>True if the move could be made; false if the requested move
        /// was invalid.</returns>
        bool MoveCursorBackward(int count);

        /// <summary>
        /// Move the cursor forward by the specified number of characters.
        /// </summary>
        /// <param name="count">Number of characters to move forward.</param>
        /// <returns>True if the move could be made; false if the requested move
        /// was invalid.</returns>
        bool MoveCursorForward(int count);

        /// <summary>
        /// Move the cursor to the start of the input buffer.
        /// </summary>
        void MoveCursorToStart();

        /// <summary>
        /// Move the cursor to the end of the input buffer.
        /// </summary>
        void MoveCursorToEnd();

        /// <summary>
        /// Move the cursor back one word.
        /// </summary>
        void MoveCursorBackwardOneWord();

        /// <summary>
        /// Move the cursor forward one word.
        /// </summary>
        void MoveCursorForwardOneWord();

        /// <summary>
        /// Delete the character under the cursor.
        /// </summary>
        void Delete();

        /// <summary>
        /// Delete the character before the cursor.
        /// </summary>
        void DeletePrecedingChar();

        /// <summary>
        /// Delete the characters from the beginning of the last word in the
        /// buffer and the current cursor.
        /// </summary>
        void DeleteBackwardThroughLastWord();

        /// <summary>
        /// Delete the characters from the current cursor up to, but not
        /// including, the start of the next word.
        /// </summary>
        void DeleteForwardToNextWord();

        /// <summary>
        /// Clear the input buffer and reset the cursor to the beginning of the
        /// buffer.
        /// </summary>
        void ClearLine(bool clearBufferOnly);

        /// <summary>
        /// Clear the screen buffer, and redisplay the prompt and current
        /// contents of the input buffer.
        /// </summary>
        void ClearScreen();

        /// <summary>
        /// Insert a character at the cursor without moving the cursor.
        /// </summary>
        /// <param name="value">The character to insert.</param>
        void Insert(char value);

        /// <summary>
        /// Insert a string at the cursor without moving the cursor.
        /// </summary>
        /// <param name="value">The string to insert.</param>
        void Insert(string value);

        /// <summary>
        /// Replace the character under the cursor.
        /// </summary>
        /// <param name="value">The character to place.</param>
        void Replace(char value);

        /// <summary>
        /// Replace the entire contents of the input buffer with the previous
        /// line in the input history.
        /// </summary>
        void ReplaceWithLastLineInHistory();

        /// <summary>
        /// Replace the entire contents of the input buffer with the next
        /// line in the input history.
        /// </summary>
        void ReplaceWithNextLineInHistory();

        /// <summary>
        /// Replace the entire contents of the input buffer with the oldest
        /// line in the input history.
        /// </summary>
        void ReplaceWithOldestLineInHistory();

        /// <summary>
        /// Replace the entire contents of the input buffer with the youngest
        /// line in the input history.
        /// </summary>
        void ReplaceWithYoungestLineInHistory();
        
        /// <summary>
        /// Save the contents of the current buffer to the input history.
        /// </summary>
        void SaveToHistory();

        /// <summary>
        /// Remove the contents of the buffer from the cursor to the end of the
        /// buffer, and place them in the paste buffer.
        /// </summary>
        void CutToEnd();

        /// <summary>
        /// Insert the contents of the paste buffer at the cursor.
        /// </summary>
        void Paste();

        /// <summary>
        /// Replace the current token in the input buffer with the previous
        /// completion.
        /// </summary>
        /// <param name="lastOperationWasCompletion">True if the last input
        /// operation was a completion operation; false otherwise.</param>
        void ReplaceCurrentTokenWithPreviousCompletion(bool lastOperationWasCompletion);

        /// <summary>
        /// Replace the current token in the input buffer with the next
        /// completion.
        /// </summary>
        /// <param name="lastOperationWasCompletion">True if the last input
        /// operation was a completion operation; false otherwise.</param>
        void ReplaceCurrentTokenWithNextCompletion(bool lastOperationWasCompletion);

        /// <summary>
        /// Replace the current token in the input buffer with all possible
        /// completions for it.
        /// </summary>
        void ReplaceCurrentTokenWithAllCompletions();

        /// <summary>
        /// Displays all completions without modifying the input buffer.
        /// </summary>
        void DisplayAllCompletions();

        /// <summary>
        /// Transforms the current word by passing it through the provided
        /// function.
        /// </summary>
        /// <param name="transformation">Function to apply.</param>
        void TransformCurrentWord(Func<string, string> transformation);

        /// <summary>
        /// Displays the input prompt.
        /// </summary>
        void DisplayPrompt();
    }
}