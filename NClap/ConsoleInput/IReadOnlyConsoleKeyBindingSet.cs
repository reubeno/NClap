using System;
using System.Collections.Generic;

namespace NClap.ConsoleInput
{
    /// <summary>
    /// Read-only abstract interface for querying a console key binding set.
    /// </summary>
    public interface IReadOnlyConsoleKeyBindingSet : IReadOnlyDictionary<ConsoleKeyInfo, ConsoleInputOperation>
    {
    }
}
