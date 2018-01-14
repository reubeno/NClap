using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NClap.ConsoleInput
{
    /// <summary>
    /// Read-only abstract interface for querying a console key binding set.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "[Legacy]")]
    public interface IReadOnlyConsoleKeyBindingSet : IReadOnlyDictionary<ConsoleKeyInfo, ConsoleInputOperation>
    {
    }
}
