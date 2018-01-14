using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NClap.Utilities;

namespace NClap.Types
{
    /// <summary>
    /// ICollectionArgumentType implementation that describes T[] types.
    /// </summary>
    internal class ArrayArgumentType : CollectionArgumentTypeBase
    {
        /// <summary>
        /// Constructs a new implementation of ICollectionArgumentType for
        /// arrays with the given type.
        /// </summary>
        /// <param name="type">Array type.</param>
        public ArrayArgumentType(Type type)
            : base(type, GetElementType(type))
        {
            Debug.Assert(type.IsArray);
        }

        /// <summary>
        /// Constructs a collection of the type described by this object,
        /// populated with objects from the provided input collection.
        /// </summary>
        /// <param name="objects">Objects to add to the collection.</param>
        /// <returns>Constructed collection.</returns>
        public override object ToCollection(IEnumerable objects)
        {
            if (objects == null) throw new ArgumentNullException(nameof(objects));
            return objects.ToArray(ElementType.Type);
        }

        /// <summary>
        /// Enumerates the items in the collection.  The input collection
        /// should be of the type described by this object.
        /// </summary>
        /// <param name="collection">Collection to enumerate.</param>
        /// <returns>The enumeration.</returns>
        public override IEnumerable ToEnumerable(object collection) => (Array)collection;

        /// <summary>
        /// Enumerates the objects contained within the provided collection;
        /// the collection must be of the type described by this object.
        /// </summary>
        /// <param name="collection">Collection to enumerate.</param>
        /// <returns>The enumerated objects.</returns>
        protected override IEnumerable<object> GetElements(object collection) =>
            ((Array)collection).Cast<object>();

        private static Type GetElementType(Type arrayType)
        {
            var elementType = arrayType.GetElementType();
            if (elementType == null)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayType));
            }

            return elementType;
        }
    }
}