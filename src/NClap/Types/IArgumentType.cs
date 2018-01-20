using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NClap.Types
{
    /// <summary>
    /// Interface for advertising a type as being parseable using this
    /// assembly.  The implementation provides sufficient functionality
    /// for command-line parsing, generating usage help information,
    /// etc.
    /// </summary>
    public interface IArgumentType : IObjectFormatter, IStringParser, IStringCompleter
    {
        /// <summary>
        /// The type's human-readable (display) name.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// The Type object associated with values described by this interface.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "[Legacy]")]
        Type Type { get; }

        /// <summary>
        /// A summary of the concrete syntax required to indicate a value of
        /// the type described by this interface (e.g. "&gt;Int32&lt;").
        /// </summary>
        string SyntaxSummary { get; }

        /// <summary>
        /// Enumeration of all types that this type depends on / includes.
        /// </summary>
        IEnumerable<IArgumentType> DependentTypes { get; }
    }
}
