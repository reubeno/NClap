using System.Collections.Generic;
using System.Linq;
using NClap.Utilities;

namespace NClap.Parser
{
    /// <summary>
    /// Describes help information for a command-line argument set.
    /// </summary>
    internal sealed class ArgumentSetUsageInfo
    {
        private readonly List<ArgumentUsageInfo> _allParameters = new List<ArgumentUsageInfo>();
        private readonly List<string> _examples = new List<string>();

        /// <summary>
        /// Description of argument set.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Logo to use.
        /// </summary>
        public ColoredMultistring Logo { get; set; } = ColoredMultistring.FromString(AssemblyUtilities.GetLogo());

        /// <summary>
        /// Preferred prefix for short named arguments, or null if no such prefix exists.
        /// </summary>
        public string DefaultShortNamePrefix { get; set; }

        /// <summary>
        /// List of usage information for all parameters.
        /// </summary>
        public IReadOnlyList<ArgumentUsageInfo> AllParameters => _allParameters;

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
        public IReadOnlyList<string> Examples => _examples;

        /// <summary>
        /// Composes basic syntax info.
        /// </summary>
        /// <param name="includeOptionalParameters">Whether or not to include a summary
        /// of optional parameters.</param>
        /// <returns>The composed info.</returns>
        public string GetBasicSyntax(bool includeOptionalParameters = true)
        {
            IEnumerable<ArgumentUsageInfo> args;

            args = includeOptionalParameters ? AllParameters : RequiredParameters;

            var syntax = args.Select(h =>
            {
                if (h.IsSelectedCommand())
                {
                    return h.ArgumentType.Format(h.CurrentValue);
                }

                return h.Syntax;
            });

            return string.Join(" ", syntax);
        }

        /// <summary>
        /// Adds info for a parameter.
        /// </summary>
        /// <param name="info">Info to add.</param>
        public void AddParameter(ArgumentUsageInfo info) => _allParameters.Add(info);

        /// <summary>
        /// Adds info for 0 or more parameters.
        /// </summary>
        /// <param name="info">Info to add.</param>
        public void AddParameters(IEnumerable<ArgumentUsageInfo> info) => _allParameters.AddRange(info);

        /// <summary>
        /// Adds an example.
        /// </summary>
        /// <param name="example">Example to add.</param>
        public void AddExample(string example) => _examples.Add(example);

        /// <summary>
        /// Adds 0 or more examples.
        /// </summary>
        /// <param name="examples">Examples to add.</param>
        public void AddExamples(IEnumerable<string> examples) => _examples.AddRange(examples);
    }
}
