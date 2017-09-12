using System;

namespace NClap.Metadata
{
    /// <summary>
    /// Interface for an object to expose additional arguments that it does
    /// not directly contain.
    /// </summary>
    internal interface IArgumentProvider
    {
        /// <summary>
        /// Retrieve info for the object type that defines the arguments to be
        /// parsed.
        /// </summary>
        /// <returns>The defining type.</returns>
        Type GetTypeDefiningArguments();

        /// <summary>
        /// Retrieve a reference to the object into which parsed arguments
        /// should be stored.
        /// </summary>
        /// <returns>The object in question.</returns>
        object GetDestinationObject();
    }
}
