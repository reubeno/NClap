using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

namespace NClap.Types
{
    /// <summary>
    /// Static class used to access references to the canonical IArgumentType
    /// implementations for .NET built-in types.
    /// </summary>
    public static class ArgumentType
    {
        /// <summary>
        /// Describes System.Decimal.
        /// </summary>
        public static IArgumentType Decimal { get; } = SimpleArgumentType.Create(decimal.Parse);

        /// <summary>
        /// Describes System.Double.
        /// </summary>
        public static IArgumentType Double { get; }= SimpleArgumentType.Create(double.Parse);

        /// <summary>
        /// Describes System.Single.
        /// </summary>
        public static IArgumentType Float { get; } = SimpleArgumentType.Create(float.Parse);

        /// <summary>
        /// Describes System.Int64.
        /// </summary>
        public static IArgumentType Long { get; } = IntegerArgumentType.Create(long.Parse, signed: true);

        /// <summary>
        /// Describes System.UInt64.
        /// </summary>
        public static IArgumentType ULong { get; } = IntegerArgumentType.Create(ulong.Parse, signed: false);

        /// <summary>
        /// Describes System.Int32.
        /// </summary>
        public static IArgumentType Int { get; } = IntegerArgumentType.Create(int.Parse, signed: true);

        /// <summary>
        /// Describes System.UInt32.
        /// </summary>
        public static IArgumentType UInt { get; } = IntegerArgumentType.Create(uint.Parse, signed: false);

        /// <summary>
        /// Describes System.Int16.
        /// </summary>
        public static IArgumentType Short { get; } = IntegerArgumentType.Create(short.Parse, signed: true);

        /// <summary>
        /// Describes System.UInt16.
        /// </summary>
        public static IArgumentType UShort { get; } = IntegerArgumentType.Create(ushort.Parse, signed: false);

        /// <summary>
        /// Describes System.SByte.
        /// </summary>
        public static IArgumentType SByte { get; } = IntegerArgumentType.Create(sbyte.Parse, signed: true);

        /// <summary>
        /// Describes System.Byte.
        /// </summary>
        public static IArgumentType Byte { get; } = IntegerArgumentType.Create(byte.Parse, signed: false);

        /// <summary>
        /// Describes System.Char.
        /// </summary>
        public static IArgumentType Char { get; } = SimpleArgumentType.Create(char.Parse);

        /// <summary>
        /// Describes System.String.
        /// </summary>
        public static IArgumentType String { get; } = SimpleArgumentType.Create(s => s);

        /// <summary>
        /// Describes System.Guid.
        /// </summary>
        public static IArgumentType Guid { get; } = SimpleArgumentType.Create(System.Guid.Parse);

        /// <summary>
        /// Describes System.Uri.
        /// </summary>
        public static IArgumentType Uri { get; } = SimpleArgumentType.Create(s => new Uri(s));

        /// <summary>
        /// Describes System.DateTime.
        /// </summary>
        public static IArgumentType DateTime { get; } = SimpleArgumentType.Create(System.DateTime.Parse);

        /// <summary>
        /// Describes System.TimeSpan.
        /// </summary>
        public static IArgumentType TimeSpan { get; } = SimpleArgumentType.Create(System.TimeSpan.Parse);

        /// <summary>
        /// Describes System.IPAddress.
        /// </summary>
        public static IArgumentType IpAddress { get; } = SimpleArgumentType.Create(System.Net.IPAddress.Parse);

        /// <summary>
        /// Describes System.Text.RegularExpressions.Regex.
        /// </summary>
        public static IArgumentType Regex { get; } = SimpleArgumentType.Create(s => new System.Text.RegularExpressions.Regex(s));

        /// <summary>
        /// Describes FileSystemPath.
        /// </summary>
        public static IArgumentType FileSystemPath { get; } = SimpleArgumentType.Create(
            s => new FileSystemPath(s),
            Types.FileSystemPath.GetCompletions);

        /// <summary>
        /// Describes System.Boolean.
        /// </summary>
        public static IArgumentType Bool { get; } = BoolArgumentType.Create();

        /// <summary>
        /// Describes System.Int64.
        /// </summary>
        public static IArgumentType Int64 => Long;

        /// <summary>
        /// Describes System.UInt64.
        /// </summary>
        public static IArgumentType UInt64 => ULong;

        /// <summary>
        /// Describes System.Int32.
        /// </summary>
        public static IArgumentType Int32 => Int;

        /// <summary>
        /// Describes System.UInt32.
        /// </summary>
        public static IArgumentType UInt32 => UInt;

        /// <summary>
        /// Describes System.Int16.
        /// </summary>
        public static IArgumentType Int16 => Short;

        /// <summary>
        /// Describes System.UInt16.
        /// </summary>
        public static IArgumentType UInt16 => UShort;

        /// <summary>
        /// Describes System.Int8.
        /// </summary>
        public static IArgumentType Int8 => SByte;

        /// <summary>
        /// Describes System.UInt8.
        /// </summary>
        public static IArgumentType UInt8 => Byte;

        /// <summary>
        /// Describes System.Boolean.
        /// </summary>
        public static IArgumentType Boolean => Bool;

        /// <summary>
        /// Describes System.Single.
        /// </summary>
        public static IArgumentType Single => Float;

        private static readonly Dictionary<Type, IArgumentType> s_builtInTypes = new Dictionary<Type, IArgumentType>();

        /// <summary>
        /// Static constructor, responsible for internally registering all
        /// known, built-in types.
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.ReadabilityRules", "SA1121:UseBuiltInTypeAlias", Justification = "StyleCop is confused")]
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "It's best to register the types separate from their construction")]
        static ArgumentType()
        {
            IArgumentType[] types =
            {
                Decimal,
                Double,
                Float,
                Long,
                ULong,
                Int,
                UInt,
                Short,
                UShort,
                SByte,
                Byte,

                String,
                Char,
                Bool,

                Guid,
                Uri,
                DateTime,
                TimeSpan,
                IpAddress,
                Regex,

                FileSystemPath,
            };

            foreach (var type in types)
            {
                s_builtInTypes.Add(type.Type, type);
            }
        }
        
        /// <summary>
        /// Retrieves the registered, stock IArgumentType implementation that
        /// describes the specified type.  Throws an exception if no such
        /// implementation could be found.
        /// </summary>
        /// <param name="type">Type to look up.</param>
        /// <returns>The object that describes the specified type.</returns>
        public static IArgumentType GetType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            IArgumentType argType;
            if (!TryGetType(type, out argType))
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Strings.TypeNotSupported, type.Name));
            }

            return argType;
        }

        /// <summary>
        /// Tries to retrieve the registered, stock IArgumentType implementation
        /// that describes the specified type.
        /// </summary>
        /// <param name="type">Type to look up.</param>
        /// <param name="argType">On success, receives the object that
        /// describes the specified type; receives null otherwise.</param>
        /// <returns>True on success; false otherwise.</returns>
        public static bool TryGetType(Type type, out IArgumentType argType)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            // See if it's a registered, well-known type.
            if (TryGetBuiltInType(type, out argType))
            {
                return true;
            }

            // Or possibly a type that directly implements IArgumentType itself.
            if (type.GetInterfaces().Contains(typeof(IArgumentType)))
            {
                var constructor = type.GetConstructor(new Type[] { });
                if (constructor == null)
                {
                    argType = null;
                    return false;
                }

                argType = (IArgumentType)constructor.Invoke(new object[] { });
                return true;
            }

            // Specially handle all enum types.
            if (type.IsEnum)
            {
                argType = EnumArgumentType.Create(type);
                return true;
            }

            // And arrays.
            if (type.IsArray)
            {
                argType = new ArrayArgumentType(type);
                return true;
            }

            // Handle all types that implement the generic ICollection<T>
            // interface.
            if (type.GetInterface(typeof(ICollection<>).Name) != null)
            {
                argType = new CollectionOfTArgumentType(type);
                return true;
            }

            // Specially handle KeyValuePair and Tuple types.
            if (type.IsGenericType)
            {
                var genericTy = type.GetGenericTypeDefinition();
                
                if (genericTy == typeof(KeyValuePair<,>))
                {
                    argType = new KeyValuePairArgumentType(type);
                    return true;
                }

                if (type.GetInterface("ITuple") != null)
                {
                    argType = new TupleArgumentType(type);
                    return true;
                }
            }

            // See if it's a nullable wrapper of a type we can handle.
            var underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                return TryGetType(underlyingType, out argType);
            }

            argType = null;
            return false;
        }

        private static bool TryGetBuiltInType(Type type, out IArgumentType argType) =>
            s_builtInTypes.TryGetValue(type, out argType);
    }
}
