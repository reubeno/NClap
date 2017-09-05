using System;
using System.Collections.Generic;
using System.Linq;
using NClap.Utilities;

namespace NClap.Parser
{
    internal abstract class HelpFormatter
    {
        public int MaxWidth { get; set; } = 80;
        public ConsoleColor HeaderForegroundColor { get; set; } = ConsoleColor.Yellow;
        public ConsoleColor ParameterMetadataForegroundColor { get; set; } = ConsoleColor.DarkCyan;
        public UsageInfoOptions Options { get; set; } = UsageInfoOptions.Default;
        public bool UseColor => Options.HasFlag(UsageInfoOptions.UseColor);

        public abstract ColoredMultistring Format(ArgumentSetUsageInfo info);

        protected ColoredMultistring Format(IEnumerable<Section> sections)
        {
            // Start composing the content.
            var builder = new ColoredMultistringBuilder();

            // Generate and sequence sections.
            bool firstSection = true;
            foreach (var section in sections)
            {
                if (!firstSection)
                {
                    builder.AppendLine();
                }

                // Add header.
                if (!string.IsNullOrEmpty(section.Name))
                {
                    var header = new ColoredString(section.Name, UseColor ? new ConsoleColor?(HeaderForegroundColor) : null);
                    builder.AppendLine(header);
                }

                // Add content.
                foreach (var entry in section.Entries)
                {
                    var text = (ColoredMultistring)entry.Wrap(
                        MaxWidth,
                        indent: section.BodyIndentWidth,
                        hangingIndent: section.HangingIndentWidth);

                    if (UseColor)
                    {
                        builder.AppendLine(text);
                    }
                    else
                    {
                        builder.AppendLine(text.ToString());
                    }
                }

                firstSection = false;
            }

            return builder.ToMultistring();
        }
        
        // We add logic here to trim out a single pair of enclosing
        // square brackets if it's present -- it's just noisy here.
        // The containing section already makes it sufficiently clear
        // whether the parameter is required or optional.
        //
        // TODO: Make this logic more generic, and put it elsewhere.
        protected string SimplifyParameterSyntax(string s) =>
            s.TrimStart('[')
             .TrimEnd(']', '*', '+');

        protected class Section
        {
            public const int DefaultIndent = 4;

            public Section(string name, IEnumerable<ColoredMultistring> entries)
            {
                Name = name;
                Entries = entries.ToList();
            }

            public Section(string name, IEnumerable<ColoredString> entries) :
                this(name, entries.Select(e => (ColoredMultistring)e))
            {
            }

            public Section(string name, IEnumerable<string> entries) : this(name, entries.Select(e => (ColoredString)e))
            {
            }

            public Section(string name, ColoredString entry) : this(name, new[] { entry })
            {
            }

            public int BodyIndentWidth { get; set; } = DefaultIndent;
            public int HangingIndentWidth { get; set; } = 0;
            public string Name { get; }
            public IReadOnlyList<ColoredMultistring> Entries { get; set; }
        }
    }
}
