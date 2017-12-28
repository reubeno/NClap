using System.Collections.Generic;
using System.Linq;
using NClap.Utilities;

namespace NClap.Parser
{
    /// <summary>
    /// Describes help information for a command-line argument set.
    /// </summary>
    internal class ArgumentSetUsageInfo
    {
        private readonly List<ArgumentUsageInfo> _allParameters = new List<ArgumentUsageInfo>();
        private readonly List<string> _examples = new List<string>();

        public string Description { get; set; }

        public ColoredMultistring Logo { get; set; } = ColoredMultistring.FromString(AssemblyUtilities.GetLogo());

        public string Remarks { get; set; }

        public string DefaultShortNamePrefix { get; set; }

        public IReadOnlyList<ArgumentUsageInfo> AllParameters => _allParameters;
        public IEnumerable<ArgumentUsageInfo> RequiredParameters => AllParameters.Where(p => p.Required);
        public IEnumerable<ArgumentUsageInfo> OptionalParameters => AllParameters.Where(p => !p.Required);

        public IReadOnlyList<string> Examples => _examples;

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

        public void AddParameter(ArgumentUsageInfo info) => _allParameters.Add(info);
        public void AddParameters(IEnumerable<ArgumentUsageInfo> info) => _allParameters.AddRange(info);

        public void AddExample(string example) => _examples.Add(example);
        public void AddExamples(IEnumerable<string> examples) => _examples.AddRange(examples);
    }
}
