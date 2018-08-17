﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NClap.Exceptions;
using NClap.Types;

namespace NClap.Metadata
{
    /// <summary>
    /// Abstract base class for logic shared between NamedArgumentAttribute and
    /// PositionalArgumentAttribute.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Needs to be array so it functions as an attribute parameter")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public abstract class ArgumentBaseAttribute : Attribute
    {
        private object _defaultValue;
        private string _longName;
        private string[] _conflictingMemberNames = Array.Empty<string>();

        /// <summary>
        /// Primary constructor.
        /// </summary>
        /// <param name="flags">Specifies the error checking to be done on the
        /// argument.</param>
        protected ArgumentBaseAttribute(ArgumentFlags flags)
        {
            Flags = flags;
        }

        /// <summary>
        /// Default element separators.
        /// </summary>
        internal static string[] DefaultElementSeparators { get; } = new[] { "," };

        /// <summary>
        /// The error checking to be done on the argument.
        /// </summary>
        public ArgumentFlags Flags { get; internal set; }

        /// <summary>
        /// The long name of the argument; null indicates that the "default"
        /// long name should be used.  The long name for every argument must
        /// be unique.  It is an error to specify a long name of string.Empty.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations", Justification = "[Legacy]")]
        public string LongName
        {
            get
            {
                var value = _longName;

                if ((value != null) && (value.Length == 0))
                {
                    throw new InvalidArgumentSetException(string.Format(
                        CultureInfo.CurrentCulture,
                        Strings.InvalidArgumentLongName));
                }

                return value;
            }

            set => _longName = value;
        }

        /// <summary>
        /// The default value of the argument.
        /// </summary>
        public object DefaultValue
        {
            get => _defaultValue;
            set
            {
                ExplicitDefaultValue = true;
                _defaultValue = value;
            }
        }

        /// <summary>
        /// Returns true if the argument has a dynamic default value.  If the
        /// argument has a dynamic default value, then during parsing, values
        /// already present in the corresponding field in the destination
        /// object will be treated as the "dynamic default" value for this
        /// argument.
        /// </summary>
        public bool DynamicDefaultValue { get; set; }

        /// <summary>
        /// The description of the argument, exposed via help/usage information.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// True indicates that this argument should be hidden from usage help
        /// text; false indicates that it should be included.
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// The names of the members that this member conflicts with.  Command-
        /// line arguments will fail to parse if they specify a value for this
        /// member as well as for any of the members referenced by this
        /// property.
        /// </summary>
        public string[] ConflictsWith
        {
            get => _conflictingMemberNames;
            set => _conflictingMemberNames = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Options for parsing numeric arguments.
        /// </summary>
        public NumberOptions NumberOptions { get; set; }

        /// <summary>
        /// True to allow "empty" arguments (e.g. empty strings); false to
        /// consider them invalid.
        /// </summary>
        public bool AllowEmpty { get; set; }

        /// <summary>
        /// Optionally specifies the strings that may be used as element
        /// separators for multiple elements that have been expressed in the
        /// same token.  Only relevant for parsing collection types, and
        /// ignored otherwise.
        /// </summary>
        public string[] ElementSeparators { get; set; } = DefaultElementSeparators;

        /// <summary>
        /// Optionally provides an implementation of <see cref="IArgumentType"/>
        /// that should be used for this argument instead of the default
        /// <see cref="IArgumentType"/> implementation associated with it.
        /// </summary>
        internal Type ArgumentType { get; set; }

        /// <summary>
        /// Optionally provides a type that implements <see cref="IStringParser"/>, and which
        /// should be used for parsing strings for this argument instead of the
        /// default <see cref="IArgumentType"/> class associated with the field/property's
        /// type.
        /// </summary>
        public Type Parser { get; set; }

        /// <summary>
        /// Optionally provides a type that implements <see cref="IObjectFormatter"/>, and
        /// which should be used for formatting objects for this argument
        /// instead of the default <see cref="IArgumentType"/> class associated with the
        /// field/property's type.
        /// </summary>
        public Type Formatter { get; set; }

        /// <summary>
        /// Optionally provides a type that implements <see cref="IStringCompleter"/>,
        /// and which should be used for generating string completions for this
        /// argument instead of the default <see cref="IArgumentType"/> class associated with
        /// the field/property's type.
        /// </summary>
        public Type Completer { get; set; }

        /// <summary>
        /// Returns true if the argument has an explicit default value.
        /// </summary>
        internal bool ExplicitDefaultValue { get; private set; }
    }
}
