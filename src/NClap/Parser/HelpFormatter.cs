using System.Collections.Generic;
using System.Linq;
using NClap.Help;
using NClap.Utilities;

namespace NClap.Parser
{
    /// <summary>
    /// Abstract base class for a usage help formatter.
    /// </summary>
    internal abstract class HelpFormatter
    {
        public ArgumentSetHelpOptions Options { get; set; } = new ArgumentSetHelpOptions();

        public abstract ColoredMultistring Format(ArgumentSetUsageInfo info);

        protected ColoredMultistring Format(IEnumerable<Section> sections)
        {
            // Start composing the content.
            var builder = new ColoredMultistringBuilder();

            // Generate and sequence sections.
            var firstSection = true;
            foreach (var section in sections)
            {
                if (!firstSection)
                {
                    for (var i = 0; i < Options.BlankLinesBetweenSections; ++i)
                    {
                        builder.AppendLine();
                    }
                }

                // Add header.
                if ((Options.SectionHeaders?.Include ?? false) && !string.IsNullOrEmpty(section.Name))
                {
                    if (Options.UseColor)
                    {
                        builder.AppendLine(new ColoredString(section.Name, Options.SectionHeaders.Color));
                    }
                    else
                    {
                        builder.AppendLine(section.Name);
                    }
                }

                // Add content.
                foreach (var entry in section.Entries)
                {
                    var text = (ColoredMultistring)entry.Wrap(
                        Options.MaxWidth.GetValueOrDefault(ArgumentSetHelpOptions.DefaultMaxWidth),
                        blockIndent: section.BodyIndentWidth,
                        hangingIndent: section.HangingIndentWidth);

                    if (Options.UseColor)
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
        protected static string SimplifyParameterSyntax(string s) =>
            s.TrimStart('[')
             .TrimEnd(']', '*', '+');

        protected class Section
        {
            public Section(ArgumentSetHelpOptions options, ArgumentMetadataHelpOptions itemOptions, IEnumerable<ColoredMultistring> entries, string name = null)
            {
                Entries = entries.ToList();
                Name = name ?? itemOptions.HeaderTitle;
                BodyIndentWidth = itemOptions.BlockIndent.GetValueOrDefault(options.SectionEntryBlockIndentWidth);
                HangingIndentWidth = itemOptions.HangingIndent.GetValueOrDefault(options.SectionEntryHangingIndentWidth);
            }

            public Section(ArgumentSetHelpOptions options, ArgumentMetadataHelpOptions itemOptions, ColoredMultistring entry) :
                this(options, itemOptions, new[] { entry })
            {
            }

            public Section(ArgumentSetHelpOptions options, ArgumentMetadataHelpOptions itemOptions, IEnumerable<ColoredString> entries) :
                this(options, itemOptions, entries.Select(e => (ColoredMultistring)e))
            {
            }

            public Section(ArgumentSetHelpOptions options, ArgumentMetadataHelpOptions itemOptions, IEnumerable<string> entries) :
                this(options, itemOptions, entries.Select(e => (ColoredString)e))
            {
            }

            public Section(ArgumentSetHelpOptions options, ArgumentMetadataHelpOptions itemOptions, ColoredString entry) :
                this(options, itemOptions, new[] { entry })
            {
            }

            public int BodyIndentWidth { get; set; }
            public int HangingIndentWidth { get; set; }
            public string Name { get; }
            public IReadOnlyList<ColoredMultistring> Entries { get; set; }
        }
    }
}
