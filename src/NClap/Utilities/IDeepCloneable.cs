namespace NClap.Utilities
{
    /// <summary>
    /// An interface representing an item that is deeply cloneable.
    /// </summary>
    /// <typeparam name="T">The type of the clone.</typeparam>
    public interface IDeepCloneable<T>
    {
        /// <summary>
        /// Creates a deep clone of the item, where no data references are shared.
        /// Changes made to the clone do not affect the original, and vice versa.
        /// </summary>
        /// <returns>The clone.</returns>
        T DeepClone();
    }
}
