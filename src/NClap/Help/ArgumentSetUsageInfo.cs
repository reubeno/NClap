using System;
using System.Collections.Generic;
using System.Linq;
using NClap.Metadata;
using NClap.Parser;
using NClap.Types;
using NClap.Utilities;

namespace NClap.Help
{
    /// <summary>
    /// Describes help information for a command-line argument set.
    /// </summary>
    internal sealed class ArgumentSetUsageInfo
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="argSet">Argument set.</param>
        /// <param name="destination">Destination object.</param>
        public ArgumentSetUsageInfo(ArgumentSetDefinition argSet, object destination)
        {
            Set = argSet;
            Destination = destination;
            AllParameters = GetParameters(argSet, destination);
        }

        private static IReadOnlyList<ArgumentUsageInfo> GetParameters(ArgumentSetDefinition argSet, object destination)
        {
            var parameters = new List<ArgumentUsageInfo>();

            // Enumerate positional arguments first, in position order.
            foreach (var arg in argSet.PositionalArguments.Where(a => !a.Hidden))
            {
                var currentValue = (destination != null) ? arg.GetValue(destination) : null;
                parameters.Add(new ArgumentUsageInfo(arg, currentValue));
            }

            // Enumerate named arguments next, in case-insensitive sort order.
            var stringComparer = argSet.Attribute.CaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
            foreach (var arg in argSet.NamedArguments
                                    .Where(a => !a.Hidden)
                                    .OrderBy(a => a.LongName, stringComparer))
            {
                var currentValue = (destination != null) ? arg.GetValue(destination) : null;
                parameters.Add(new ArgumentUsageInfo(arg, currentValue));
            }

            // TODO: Add an extra item for answer files, if that is supported on this
            // argument set.

            return parameters;
        }

        /// <summary>
        /// Argument set.
        /// </summary>
        public ArgumentSetDefinition Set { get; }

        /// <summary>
        /// Destination object.
        /// </summary>
        public object Destination { get; }

        /// <summary>
        /// Description of argument set.
        /// </summary>
        public string Description
        {
            get
            {
                // Special case: in case we have a selected command.
                var lastSelectedCommand = AllParameters.LastOrDefault(parameter => parameter.IsSelectedCommand());
                if (lastSelectedCommand != null &&
                    lastSelectedCommand.CurrentValue is ICommandGroup group &&
                    group.HasSelection &&
                    ArgumentType.TryGetType(group.Selection.GetType(), out IArgumentType argType) &&
                    argType is IEnumArgumentType enumArgType &&
                    enumArgType.TryGetValue(group.Selection, out IArgumentValue value) &&
                    !string.IsNullOrEmpty(value.Description))
                {
                    return value.Description;
                }

                return Set.Attribute.Description;
            }
        }

        /// <summary>
        /// Logo to use.
        /// </summary>
        public ColoredMultistring Logo =>
            Set.Attribute.LogoString ?? ColoredMultistring.FromString(AssemblyUtilities.GetLogo());

        /// <summary>
        /// Preferred prefix for long named arguments, or null if no such prefix exists.
        /// </summary>
        public string DefaultLongNamePrefix => Set.Attribute.NamedArgumentPrefixes.FirstOrDefault();

        /// <summary>
        /// Preferred prefix for short named arguments, or null if no such prefix exists.
        /// </summary>
        public string DefaultShortNamePrefix => Set.Attribute.ShortNameArgumentPrefixes.FirstOrDefault();

        /// <summary>
        /// List of usage information for all parameters.
        /// </summary>
        public IReadOnlyList<ArgumentUsageInfo> AllParameters { get; }

        /// <summary>
        /// Usage information for required parameters only.
        /// </summary>
        public IEnumerable<ArgumentUsageInfo> RequiredParameters => AllParameters.Where(p => p.Required);

        /// <summary>
        /// Usage information for optional parameters only.
        /// </summary>
        public IEnumerable<ArgumentUsageInfo> OptionalParameters => AllParameters.Where(p => !p.Required);

        /// <summary>
        /// Examples.
        /// </summary>
        public IReadOnlyList<string> Examples => Set.Attribute.Examples ?? Array.Empty<string>();

        /// <summary>
        /// Composes basic syntax info.
        /// </summary>
        /// <param name="options">Options for generating syntax info.</param>
        /// <returns>The composed info.</returns>
        public string GetBasicSyntax(ArgumentSyntaxHelpOptions options)
        {
            IEnumerable<ArgumentUsageInfo> args;

            args = options.IncludeOptionalArguments ? AllParameters : RequiredParameters;

            var syntax = args.Select(arg =>
            {
                if (arg.IsSelectedCommand())
                {
                    return arg.ArgumentType.Format(arg.CurrentValue);
                }

                return arg.Arg.GetSyntaxSummary(detailed: false);
            });

            return string.Join(" ", syntax);
        }
    }
}
