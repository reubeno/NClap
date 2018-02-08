using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using NClap.Exceptions;
using NClap.Metadata;
using NClap.Types;
using NClap.Utilities;

namespace NClap.Parser
{
    /// <summary>
    /// Describes a command-line argument.
    /// </summary>
    public sealed class ArgumentDefinition
    {
        private readonly HashSet<ArgumentDefinition> _conflictingArgs = new HashSet<ArgumentDefinition>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="member">Info for the member backing this argument.</param>
        /// <param name="attribute">Argument attribute on the field.</param>
        /// <param name="argSet">Argument set containing this argument.</param>
        /// <param name="defaultValue">Default value for the field.</param>
        /// <param name="containingArgument">Optionally provides a reference
        /// to the definition of the argument that "contains" these arguments.
        /// </param>
        public ArgumentDefinition(MemberInfo member,
            ArgumentBaseAttribute attribute,
            ArgumentSetDefinition argSet,
            object defaultValue = null,
            ArgumentDefinition containingArgument = null)
            : this(GetMutableMemberInfo(member), attribute, argSet, defaultValue, /*fixedDestination=*/null, containingArgument)
        {
        }

        /// <summary>
        /// Internal constructor.
        /// </summary>
        /// <param name="member">Field to describe.</param>
        /// <param name="attribute">Argument attribute on the field.</param>
        /// <param name="argSet">Argument set containing this argument.</param>
        /// <param name="defaultValue">Default value for the field.</param>
        /// <param name="fixedDestination">Optionally provides fixed parse destination object.</param>
        /// <param name="containingArgument">Optionally provides a reference
        /// to the definition of the argument that "contains" these arguments.
        /// </param>
        internal ArgumentDefinition(IMutableMemberInfo member,
            ArgumentBaseAttribute attribute,
            ArgumentSetDefinition argSet,
            object defaultValue = null,
            object fixedDestination = null,
            ArgumentDefinition containingArgument = null)
        {
            Member = member ?? throw new ArgumentNullException(nameof(member));
            Attribute = attribute ?? throw new ArgumentNullException(nameof(attribute));
            ContainingSet = argSet ?? throw new ArgumentNullException(nameof(argSet));
            ContainingArgument = containingArgument;
            FixedDestination = fixedDestination;
            IsPositional = attribute is PositionalArgumentAttribute;
            ArgumentType = Attribute.GetArgumentType(member.MemberType);
            CollectionArgumentType = AsCollectionType(ArgumentType);
            HasDefaultValue = attribute.ExplicitDefaultValue || attribute.DynamicDefaultValue;
            ValidationAttributes = GetValidationAttributes(ArgumentType, Member);

            LongName = GetLongName(attribute, argSet.Attribute, member.MemberInfo);
            ExplicitShortName = HasExplicitShortName(attribute);
            ShortName = GetShortNameOrNull(attribute, argSet.Attribute, member.MemberInfo);
            DefaultValue = GetDefaultValue(attribute, member, defaultValue);

            var nullableBase = Nullable.GetUnderlyingType(member.MemberType);

            if (CollectionArgumentType != null)
            {
                ValueType = CollectionArgumentType.ElementType;
            }
            else if (nullableBase != null)
            {
                // For nullable arguments, we use the wrapped type (T in
                // Nullable<T>) as the value type. Parsing an enum or int is the
                // same as parsing an enum? or int?, for example, since null can
                // only arise if the value was not provided at all.
                ValueType = Attribute.GetArgumentType(nullableBase);
            }
            else
            {
                ValueType = ArgumentType;
            }

            Debug.Assert(ValueType != null);

            if (Unique && !IsCollection)
            {
                throw new InvalidArgumentSetException(member, Strings.UniqueUsedOnNonCollectionArgument);
            }

            Debug.Assert(!string.IsNullOrEmpty(LongName));
        }

        /// <summary>
        /// The argument's long form name, if one exists; null otherwise.
        /// </summary>
        public string LongName { get; }

        /// <summary>
        /// The argument's short form name, if one exists; null otherwise.
        /// </summary>
        public string ShortName { get; private set; }

        /// <summary>
        /// True indicates that the argument's short form name was explicitly
        /// specified; false indicates that the name was defaulted.
        /// </summary>
        public bool ExplicitShortName { get; }

        /// <summary>
        /// True if the argument has a default value, false otherwise.
        /// </summary>
        public bool HasDefaultValue { get; }

        /// <summary>
        /// The default value to associate with the argument if one is not
        /// specified on the command line being parsed.
        /// </summary>
        public object DefaultValue { get; }

        /// <summary>
        /// The effective default value associated with the argument.  This
        /// is equivalent to DefaultValue if HasDefaultValue is true.
        /// </summary>
        public object EffectiveDefaultValue
        {
            get
            {
                var defaultValue = HasDefaultValue ? DefaultValue : null;
                if ((defaultValue == null) && !IsCollection)
                {
                    defaultValue = ArgumentType.Type.GetDefaultValue();
                }

                return defaultValue;
            }
        }

        /// <summary>
        /// True indicates that this argument consumes the remainder of the
        /// command line being parsed; false indicates that it only consumes
        /// a single command-line token.
        /// </summary>
        public bool TakesRestOfLine => Attribute.Flags.HasFlag(ArgumentFlags.RestOfLine);

        /// <summary>
        /// True indicates that this argument is required to be present; false
        /// indicates that it's optional.
        /// </summary>
        public bool IsRequired => Attribute.Flags.HasFlag(ArgumentFlags.Required);

        /// <summary>
        /// True indicates that this argument may be specified multiple times
        /// on the command line; false indicates that it may only be specified
        /// once.
        /// </summary>
        public bool AllowMultiple =>
            Attribute.Flags.HasFlag(ArgumentFlags.Multiple) ||
            (Attribute.Flags.HasFlag(ArgumentFlags.Optional) && IsCollection);

        /// <summary>
        /// True indicates that each instance of this argument must be unique;
        /// false indicates that this restriction is not applicable.
        /// </summary>
        public bool Unique => Attribute.Flags.HasFlag(ArgumentFlags.Unique);

        /// <summary>
        /// True if the argument is a collection argument; false if it's a
        /// scalar.
        /// </summary>
        public bool IsCollection => CollectionArgumentType != null;

        /// <summary>
        /// True indicates that this argument should not be mentioned by
        /// usage help information; false indicates that it should be included.
        /// </summary>
        public bool Hidden => Attribute.Hidden;

        /// <summary>
        /// The argument's static metadata.
        /// </summary>
        public ArgumentBaseAttribute Attribute { get; }

        /// <summary>
        /// Optional reference to the definition of the argument that "contains"
        /// this argument.
        /// </summary>
        public ArgumentDefinition ContainingArgument { get; }

        /// <summary>
        /// The argument set containing this argument.
        /// </summary>
        public ArgumentSetDefinition ContainingSet { get; }

        /// <summary>
        /// The object member bound to this argument.
        /// </summary>
        public IMutableMemberInfo Member { get; }

        /// <summary>
        /// Type of the argument.
        /// </summary>
        public IArgumentType ArgumentType { get; }

        /// <summary>
        /// Optionally indicates the destination object to which this is fixed.
        /// </summary>
        public object FixedDestination { get; }

        /// <summary>
        /// The type of values for this argument; for collection-backed arguments, this
        /// is the type of elements of the collection.
        /// </summary>
        public IArgumentType ValueType { get; }

        /// <summary>
        /// If this argument is backed by a collection, provides the collection argument
        /// type; otherwise, null.
        /// </summary>
        public ICollectionArgumentType CollectionArgumentType { get; }

        /// <summary>
        /// True if this argument is positional; false otherwise.
        /// </summary>
        public bool IsPositional { get; }

        /// <summary>
        /// Enumerates all validation attributes for this one.
        /// </summary>
        public IReadOnlyList<ArgumentValidationAttribute> ValidationAttributes { get; }

        /// <summary>
        /// Enumerates all arguments that conflict with this one.
        /// </summary>
        public IEnumerable<ArgumentDefinition> ConflictingArgs => _conflictingArgs;

        /// <summary>
        /// String summary of object.
        /// </summary>
        /// <returns>String.</returns>
        public override string ToString() => base.ToString();

        /// <summary>
        /// Registers an Argument that conflicts with the one described by this
        /// object.  If the specified argument has already previously been
        /// registered, then this operation is a no-op.
        /// </summary>
        /// <param name="arg">The conflicting argument.</param>
        public void AddConflictingArgument(ArgumentDefinition arg)
        {
            if (arg == this)
            {
                throw new ArgumentOutOfRangeException(nameof(arg));
            }

            _conflictingArgs.Add(arg);
        }

        /// <summary>
        /// Formats the argument into a string.
        /// </summary>
        /// <param name="value">Value to format.</param>
        /// <param name="suppressArgNames">True to suppress argument names;
        /// false to leave them in.</param>
        /// <returns>The formatted string.</returns>
        public IEnumerable<string> Format(object value, bool suppressArgNames = false)
        {
            if (CollectionArgumentType != null)
            {
                foreach (var item in CollectionArgumentType.ToEnumerable(value))
                {
                    if (suppressArgNames)
                    {
                        yield return CollectionArgumentType.ElementType.Format(item);
                    }
                    else
                    {
                        yield return Format(CollectionArgumentType.ElementType, item);
                    }
                }
            }
            else if (suppressArgNames)
            {
                yield return ArgumentType.Format(value);
            }
            else
            {
                yield return Format(ArgumentType, value);
            }
        }

        /// <summary>
        /// Generates syntax help information for this argument.
        /// </summary>
        /// <param name="detailed">true to return detailed information,
        /// including full argument type information; false to return abridged
        /// information.</param>
        /// <returns>The help content in string form.</returns>
        [Obsolete("This method is no longer implemented and will be removed from future releases.")]
#pragma warning disable CA1822 // Mark members as static
        public string GetSyntaxSummary(bool detailed = true) => string.Empty;
#pragma warning restore CA1822 // Mark members as static

        /// <summary>
        /// Retrieves the value associated with this argument in the provided
        /// containing object.
        /// </summary>
        /// <param name="containingValue">The containing object.</param>
        /// <returns>The value associated with this argument's field.</returns>
        public object GetValue(object containingValue)
        {
            if (FixedDestination != null)
            {
                containingValue = FixedDestination;
            }

            return Member.GetValue(containingValue);
        }

        /// <summary>
        /// Checks whether this argument requires an option argument.
        /// </summary>
        /// <returns>true if it's required, false if it's optional.</returns>
        public bool RequiresOptionArgument => !IsEmptyStringValid();

        /// <summary>
        /// Clears the short name associated with this argument.
        /// </summary>
        public void ClearShortName() => ShortName = null;

        /// <summary>
        /// Tries to retrieve the name of the given type associated with this
        /// argument.  If no such name of that type exists, returns null.
        /// </summary>
        /// <param name="nameType">The type of name to retrieve.</param>
        /// <returns>The given name, or null if no such name exists.</returns>
        public string GetName(ArgumentNameType nameType)
        {
            switch (nameType)
            {
                case ArgumentNameType.ShortName:
                    return ShortName;
                case ArgumentNameType.LongName:
                    return LongName;
                default:
                    throw new ArgumentOutOfRangeException(nameof(nameType));
            }
        }

        private static IReadOnlyList<ArgumentValidationAttribute> GetValidationAttributes(IArgumentType argType, IMutableMemberInfo memberInfo)
        {
            var member = memberInfo.MemberInfo;

            var collectionArgType = AsCollectionType(argType);
            var argTypeToCheck = (collectionArgType != null) ? collectionArgType.ElementType : argType;

            var attributes = member.GetAttributes<ArgumentValidationAttribute>().ToList();
            foreach (var attrib in attributes.Where(attrib => !attrib.AcceptsType(argTypeToCheck)))
            {
                throw new InvalidArgumentSetException(memberInfo, string.Format(
                    CultureInfo.CurrentCulture,
                    Strings.BadValidationAttribute,
                    attrib.GetType().Name,
                    argType.Type.Name));
            }

            return attributes;
        }

        private static ICollectionArgumentType AsCollectionType(IArgumentType type)
        {
            if (type is ArgumentTypeExtension extension)
            {
                type = extension.InnerType;
            }

            return type as ICollectionArgumentType;
        }

        private static string GetLongName(ArgumentBaseAttribute attribute, ArgumentSetAttribute argSetAttribute, MemberInfo member)
        {
            if (attribute.LongName != null)
            {
                return attribute.LongName;
            }

            var longName = member.Name;

            if (argSetAttribute.NameGenerationFlags.HasFlag(ArgumentNameGenerationFlags.GenerateHyphenatedLowerCaseLongNames))
            {
                longName = longName.ToHyphenatedLowerCase();
            }

            return longName;
        }

        private static string GetShortNameOrNull(ArgumentBaseAttribute attribute, ArgumentSetAttribute argSetAttribute, MemberInfo member)
        {
            if (attribute is PositionalArgumentAttribute)
            {
                return null;
            }

            if (HasExplicitShortName(attribute))
            {
                var namedAttribute = (NamedArgumentAttribute)attribute;
                return namedAttribute.ShortName.Length == 0 ? null : namedAttribute.ShortName;
            }

            var longName = GetLongName(attribute, argSetAttribute, member);
            var shortName = longName.Substring(0, 1);

            if (argSetAttribute.NameGenerationFlags.HasFlag(ArgumentNameGenerationFlags.PreferLowerCaseForShortNames))
            {
                shortName = shortName.ToLower();
            }

            return shortName;
        }

        private static object GetDefaultValue(ArgumentBaseAttribute attribute, IMutableMemberInfo member, object defaultFieldValue)
        {
            object value;
            if (attribute.DynamicDefaultValue)
            {
                value = defaultFieldValue;
            }
            else if (attribute.ExplicitDefaultValue)
            {
                value = attribute.DefaultValue;
            }
            else
            {
                try
                {
                    // N.B. This will fail if it's a reflection-only type.
                    value = member.MemberType.GetDefaultValue();
                }
                catch (InvalidOperationException)
                {
                    value = null;
                }
            }

            // Validate the value's type.
            if (value != null && !member.MemberType.GetTypeInfo().IsAssignableFrom(value.GetType()))
            {
                // See if it's implicitly convertible.
                if (!member.MemberType.IsImplicitlyConvertibleFrom(value))
                {
                    throw new InvalidArgumentSetException(member, string.Format(
                        CultureInfo.CurrentCulture,
                        Strings.DefaultValueIsOfWrongType,
                        member.MemberInfo.Name,
                        value.GetType().Name,
                        member.MemberType.Name));
                }
            }

            return value;
        }

        private static bool HasExplicitShortName(ArgumentBaseAttribute attribute)
        {
            var argAttrib = attribute as NamedArgumentAttribute;
            return argAttrib?.ShortName != null;
        }

        /// <summary>
        /// Checks if the empty string is a valid value for this argument.
        /// </summary>
        /// <returns>true if it is valid; false otherwise.</returns>
        internal bool IsEmptyStringValid()
        {
            var parseState = new ArgumentParser(ContainingSet, this, CommandLineParserOptions.Quiet(), /*destination=*/null);

            return ArgumentType.TryParse(parseState.ParseContext, string.Empty, out object parsedEmptyString) &&
            parseState.TryValidateValue(
                parsedEmptyString,
                new ArgumentValidationContext(parseState.ParseContext.FileSystemReader),
                reportInvalidValue: false);
        }

        private string Format(IObjectFormatter type, object value)
        {
            var formattedValue = type.Format(value);

            if (IsPositional)
            {
                return formattedValue;
            }

            return string.Concat(
                ContainingSet.Attribute.NamedArgumentPrefixes.FirstOrDefault() ?? string.Empty,
                LongName,
                ContainingSet.Attribute.ArgumentValueSeparators.FirstOrDefault(),
                formattedValue);
        }

        private static IMutableMemberInfo GetMutableMemberInfo(MemberInfo member)
        {
            if (member == null)
            {
                throw new ArgumentNullException(nameof(member));
            }

            switch (member)
            {
                case FieldInfo fi:
                    return new MutableFieldInfo(fi);
                case PropertyInfo pi:
                    return new MutablePropertyInfo(pi);
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
