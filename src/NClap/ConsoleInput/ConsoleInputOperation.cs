namespace NClap.ConsoleInput
{
    /// <summary>
    /// Type of a console input operation.
    /// </summary>
    public enum ConsoleInputOperation
    {
        //
        // No op.
        //

        /// <summary>
        /// Do nothing.
        /// </summary>
        NoOp,

        //
        // Character insertion.
        //

        /// <summary>
        /// Process a character.
        /// </summary>
        ProcessCharacter,

        //
        // Standard readline operations.
        //

        /// <summary>
        /// Terminate (and accept) the current line of input.
        /// </summary>
        AcceptLine,

        /// <summary>
        /// Terminate the current input stream.
        /// </summary>
        EndOfFile,

        /// <summary>
        /// Move the input cursor to the beginning of the input line.
        /// </summary>
        BeginningOfLine,

        /// <summary>
        /// Move the input cursor to the end of the input line.
        /// </summary>
        EndOfLine,

        /// <summary>
        /// Move the input cursor forward by one character.
        /// </summary>
        ForwardChar,

        /// <summary>
        /// Move the input cursor backward by one character.
        /// </summary>
        BackwardChar,

        /// <summary>
        /// Clear the output screen without clearing the input line.  The input
        /// line should be re-displayed after the clearing.
        /// </summary>
        ClearScreen,

        /// <summary>
        /// Replace the contents of the current input buffer with the previous
        /// line in the input history.
        /// </summary>
        PreviousHistory,

        /// <summary>
        /// Replace the contents of the current input buffer with the next
        /// line in the input history.
        /// </summary>
        NextHistory,

        /// <summary>
        /// Cut the contents of the input buffer, starting from and including
        /// the character under the cursor, placing the removed contents in the
        /// implicit copy/paste buffer.
        /// </summary>
        KillLine,

        /// <summary>
        /// Delete the previous word from the input line.
        /// </summary>
        UnixWordRubout,

        /// <summary>
        /// Paste the current contents of the copy/paste buffer at the input
        /// cursor.
        /// </summary>
        Yank,

        /// <summary>
        /// Cancel the current input line without clearing it, and re-display
        /// the input prompt.
        /// </summary>
        Abort,

        /// <summary>
        /// Move the cursor forward to the beginning of the next word.
        /// </summary>
        ForwardWord,

        /// <summary>
        /// Move the cursor forward to the beginning of the previous word.
        /// </summary>
        BackwardWord,

        /// <summary>
        /// Replace the contents of the current input buffer with the oldest
        /// line in the input history.
        /// </summary>
        BeginningOfHistory,

        /// <summary>
        /// Replace the contents of the current input buffer with the youngest
        /// line in the input history.
        /// </summary>
        EndOfHistory,

        /// <summary>
        /// Replace the remainder of the current word, starting from and
        /// including the character under the input cursor, with its upcased
        /// counterpart.
        /// </summary>
        UpcaseWord,

        /// <summary>
        /// Replace the remainder of the current word, starting from and
        /// including the character under the input cursor, with its downcased
        /// counterpart.
        /// </summary>
        DowncaseWord,

        /// <summary>
        /// Replace the remainder of the current word, starting from and
        /// including the character under the input cursor, with its capitalized
        /// counterpart.  Only the first non-whitespace character should be
        /// upcased; all other characters should be downcased.
        /// </summary>
        CapitalizeWord,

        /// <summary>
        /// Delete the current word from the input buffer.
        /// </summary>
        KillWord,

        /// <summary>
        /// Display all possible completions of the current token.
        /// </summary>
        PossibleCompletions,

        /// <summary>
        /// Replace the current token with a whitespace-separated list of
        /// completions for the current token.
        /// </summary>
        InsertCompletions,

        /// <summary>
        /// Clear the input line.
        /// </summary>
        RevertLine,

        /// <summary>
        /// Prepend the current input line with an end-of-line comment
        /// character and cancel the current input line (without clearing it).
        /// </summary>
        InsertComment,

        /// <summary>
        /// Insert a tab character at the cursor.
        /// </summary>
        TabInsert,

        /// <summary>
        /// Delete the previous word from the input line.
        /// </summary>
        BackwardKillWord,

        //
        // Extensions
        //

        /// <summary>
        /// Replace the current token with the next available completion for it.
        /// </summary>
        CompleteTokenNext,

        /// <summary>
        /// Replace the current token with the previous completion for it.
        /// </summary>
        CompleteTokenPrevious,

        /// <summary>
        /// Delete the character before the input cursor.
        /// </summary>
        DeletePreviousChar,

        /// <summary>
        /// Delete the character under the input cursor.
        /// </summary>
        DeleteChar,

        /// <summary>
        /// Toggle the insert mode.
        /// </summary>
        ToggleInsertMode,

        //
        // Unimplemented standard readline operations
        //

        /// <summary>
        /// Prompt for a search term and replace the current input line with
        /// the previous line in history containing the term.
        /// </summary>
        ReverseSearchHistory,

        /// <summary>
        /// Prompt for a search term and replace the current input line with
        /// the next line in history containing the term.
        /// </summary>
        ForwardSearchHistory,

        /// <summary>
        /// Prompt for another keystroke and insert the quoted version of that
        /// keystroke at the input cursor.
        /// </summary>
        QuotedInsert,

        /// <summary>
        /// Transpose the character before the cursor and the character after
        /// the cursor, and move the cursor forward by one character.  If the
        /// cursor is at the end of the input line, then instead transpose the
        /// two characters before the cursor.  If the cursor is at the beginning
        /// of the input line, then do nothing.
        /// </summary>
        TransposeChars,

        /// <summary>
        /// Discard the contents of the input line.
        /// </summary>
        UnixLineDiscard,

        /// <summary>
        /// Undo the last change to the input buffer.
        /// </summary>
        Undo,

        /// <summary>
        /// Set mark.
        /// </summary>
        SetMark,

        /// <summary>
        /// Search for a character.
        /// </summary>
        CharacterSearch,

        /// <summary>
        /// Yank the last arg.
        /// </summary>
        YankLastArg,

        /// <summary>
        /// Perform a non-incremental reverse search through history.
        /// </summary>
        NonIncrementalReverseSearchHistory,

        /// <summary>
        /// Perform a non-incremental forward search through history.
        /// </summary>
        NonIncrementalForwardSearchHistory,

        /// <summary>
        /// Yank pop.
        /// </summary>
        YankPop,

        /// <summary>
        /// Tilde-expand the buffer.
        /// </summary>
        TildeExpand,

        /// <summary>
        /// Yank the Nth argument.
        /// </summary>
        YankNthArg,

        /// <summary>
        /// Search for a character backwards.
        /// </summary>
        CharacterSearchBackward
    }
}
