using System;
using System.Collections.Generic;
using System.Reflection;
using NClap.Metadata;
using NClap.Utilities;

namespace NClap.Types
{
    /// <summary>
    /// Implementation to describe verb groups.
    /// </summary>
    internal class VerbGroupArgumentType : ArgumentTypeBase
    {
        private readonly Type _verbTypeType;
        private readonly IArgumentType _verbArgType;

        public VerbGroupArgumentType(Type type) : base(type)
        {
            if (!type.GetTypeInfo().IsGenericType)
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }

            if (type.GetGenericTypeDefinition() != typeof(VerbGroup<>))
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }

            var typeParams = type.GetTypeInfo().GetGenericArguments();
            if (typeParams.Length != 1)
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }

            _verbTypeType = typeParams[0];
            if (!_verbTypeType.GetTypeInfo().IsEnum)
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }

            if (!ArgumentType.TryGetType(_verbTypeType, out _verbArgType))
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        protected override object Parse(ArgumentParseContext context, string stringToParse)
        {
            if (!_verbArgType.TryParse(context, stringToParse, out object verbType))
            {
                throw new ArgumentOutOfRangeException(nameof(stringToParse));
            }

            var verbTypeName = _verbTypeType.GetTypeInfo().GetEnumName(verbType);
            var verbTypeField = _verbTypeType.GetTypeInfo().GetField(verbTypeName);
            var verbAttrib = verbTypeField.GetSingleAttribute<VerbAttribute>();

            var implementingType = verbAttrib.GetImplementingType(_verbTypeType);
            if (implementingType == null)
            {
                throw new ArgumentOutOfRangeException(nameof(stringToParse));
            }

            var verbConstructor = implementingType.GetTypeInfo().GetConstructor(Array.Empty<Type>());
            if (verbConstructor == null)
            {
                throw new ArgumentOutOfRangeException(nameof(stringToParse));
            }

            var verb = verbConstructor.Invoke(Array.Empty<object>()) as IVerb;
            if (verb == null)
            {
                throw new ArgumentOutOfRangeException(nameof(stringToParse));
            }

            var verbGroupConstructor = Type.GetTypeInfo().GetConstructor(new[] { _verbTypeType, typeof(IVerb) });
            if (verbGroupConstructor == null)
            {
                throw new ArgumentOutOfRangeException(nameof(stringToParse));
            }

            var group = verbGroupConstructor.Invoke(new[] { verbType, verb });
            if (group == null)
            {
                throw new ArgumentOutOfRangeException(nameof(stringToParse));
            }

            return group;
        }

        public override string DisplayName => _verbArgType.DisplayName;

        public override IEnumerable<IArgumentType> DependentTypes => new[] { _verbArgType };
    }
}
