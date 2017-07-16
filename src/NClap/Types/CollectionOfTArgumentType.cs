using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NClap.Types
{
    /// <summary>
    /// ICollectionArgumentType implementation that describes types that
    /// implement the (generic) ICollection&lt;T&gt; interface.
    /// </summary>
    internal class CollectionOfTArgumentType : CollectionArgumentTypeBase
    {
        private readonly ConstructorInfo _constructor;
        private readonly MethodInfo _addMethod;

        /// <summary>
        /// Constructs a new implementation of ICollectionArgumentType for
        /// objects of the given type; the type in question must implement
        /// the (generic) ICollection&lt;T&gt; interface.
        /// </summary>
        /// <param name="type">Array type.</param>
        public CollectionOfTArgumentType(Type type)
            : base(type, GetElementType(type))
        {
            _constructor = Type.GetTypeInfo().GetConstructor(Array.Empty<Type>());
            if (_constructor == null)
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }

            var interfaceType = Type.GetTypeInfo().GetInterface(typeof(ICollection<>).Name);
            var interfaceMethods = Type.GetTypeInfo().GetRuntimeInterfaceMap(interfaceType).TargetMethods;
            var addMethods = interfaceMethods.Where(
                method => (method.Name == "Add") || method.Name.EndsWith(".Add", StringComparison.Ordinal));

            _addMethod = addMethods.Single();
        }

        /// <summary>
        /// Constructs a collection of the type described by this object,
        /// populated with objects from the provided input collection.
        /// </summary>
        /// <param name="objects">Objects to add to the collection.</param>
        /// <returns>Constructed collection.</returns>
        public override object ToCollection(IEnumerable objects)
        {
            if (objects == null)
            {
                throw new ArgumentNullException(nameof(objects));
            }

            var collection = _constructor.Invoke(Array.Empty<object>());
            foreach (var o in objects)
            {
                try
                {
                    _addMethod.Invoke(collection, new[] { o });
                }
                catch (TargetInvocationException ex)
                {
                    throw ex.InnerException;
                }
            }

            return collection;
        }

        /// <summary>
        /// Enumerates the items in the collection.  The input collection
        /// should be of the type described by this object.
        /// </summary>
        /// <param name="collection">Collection to enumerate.</param>
        /// <returns>The enumeration.</returns>
        public override IEnumerable ToEnumerable(object collection) => (ICollection)collection;

        /// <summary>
        /// Enumerates the objects contained within the provided collection;
        /// the collection must be of the type described by this object.
        /// </summary>
        /// <param name="collection">Collection to enumerate.</param>
        /// <returns>The enumerated objects.</returns>
        protected override IEnumerable<object> GetElements(object collection) =>
            ((ICollection)collection).Cast<object>();

        private static Type GetElementType(Type collectionType)
        {
            var interfaceType = collectionType.GetTypeInfo().GetInterface(typeof(ICollection<>).Name);
            if (interfaceType == null)
            {
                throw new ArgumentOutOfRangeException(nameof(collectionType));
            }

            return interfaceType.GetTypeInfo().GetGenericArguments().Single();
        }
    }
}
