using System.Collections.Generic;

namespace NClap.Types
{
    /// <summary>
    /// Enum argument type provider.
    /// </summary>
    public interface IEnumArgumentTypeProvider
    {
        /// <summary>
        /// Retrieves types being provided.
        /// </summary>
        /// <returns>Enumeration of types.</returns>
        IEnumerable<IEnumArgumentType> GetTypes();
    }
}
