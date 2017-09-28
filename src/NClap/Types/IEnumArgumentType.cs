using System.Collections.Generic;

namespace NClap.Types
{
    /// <summary>
    /// Interface for advertising a  type as being parseable
    /// using this assembly.  The implementation provides sufficient
    /// functionality for command-line parsing, generating usage help
    /// information, etc.  This interface should only be implemented
    /// by objects that describe .NET enum objects.
    /// </summary>
    public interface IEnumArgumentType : IArgumentType
    {
        /// <summary>
        /// Enumerate the values allowed for this enum.
        /// </summary>
        /// <returns>The values.</returns>
        IEnumerable<IArgumentValue> GetValues();
    }
}
