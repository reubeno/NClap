using System.Collections;

namespace NClap.Types
{
    /// <summary>
    /// Interface for advertising a collection type as being parseable
    /// using this assembly.  The implementation provides sufficient
    /// functionality for command-line parsing, generating usage help
    /// information, etc.  This interface should only be implemented
    /// by objects that describe .NET collection objects.
    /// </summary>
    public interface ICollectionArgumentType : IArgumentType
    {
        /// <summary>
        /// Argument type of elements in the collection described by this
        /// object.
        /// </summary>
        IArgumentType ElementType { get; }

        /// <summary>
        /// Constructs a collection of the type described by this object,
        /// populated with objects from the provided input collection.
        /// </summary>
        /// <param name="objects">Objects to add to the collection.</param>
        /// <returns>Constructed collection.</returns>
        object ToCollection(IEnumerable objects);

        /// <summary>
        /// Enumerates the items in the collection.  The input collection
        /// should be of the type described by this object.
        /// </summary>
        /// <param name="collection">Collection to enumerate.</param>
        /// <returns>The enumeration.</returns>
        IEnumerable ToEnumerable(object collection);
    }
}
