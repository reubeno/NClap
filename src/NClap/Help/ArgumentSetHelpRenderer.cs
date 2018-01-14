using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NClap.Parser;
using NClap.Types;
using NClap.Utilities;

namespace NClap.Help
{
    /// <summary>
    /// Help formatter.
    /// </summary>
    internal sealed class ArgumentSetHelpRenderer
    {
        private readonly ArgumentSetHelpOptions _options;

        public ArgumentSetHelpRenderer(ArgumentSetHelpOptions options)
        {
            _options = options;
        }

        public ColoredMultistring Format(ArgumentSetUsageInfo info) =>
            Format(GenerateSections(info));

        private ColoredMultistring Format(IEnumerable<Section> sections)
        {
            // Start composing the content.
            var builder = new ColoredMultistringBuilder();

            // Generate and sequence sections.
            var firstSection = true;
            foreach (var section in sections)
            {
                if (!firstSection)
                {
                    for (var i = 0; i < _options.BlankLinesBetweenSections; ++i)
                    {
                        builder.AppendLine();
                    }
                }

                // Add header.
                if ((_options.SectionHeaders?.Include ?? false) && !string.IsNullOrEmpty(section.Name))
                {
                    if (_options.UseColor)
                    {
                        builder.AppendLine(new ColoredString(section.Name, _options.SectionHeaders.Color));
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
                        _options.MaxWidth.GetValueOrDefault(ArgumentSetHelpOptions.DefaultMaxWidth),
                        blockIndent: section.BodyIndentWidth,
                        hangingIndent: section.HangingIndentWidth);

                    if (_options.UseColor)
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

        private IReadOnlyList<Section> GenerateSections(ArgumentSetUsageInfo info)
        {
            var sections = new List<Section>();

            if (_options.Logo?.Include ?? false &&
                info.Logo != null && !info.Logo.IsEmpty())
            {
                sections.Add(new Section(_options, _options.Logo, info.Logo));
            }

            // Append basic usage info.
            if (_options.Syntax?.Include ?? false)
            {
                var basicSyntax = new List<ColoredString>();

                if (!string.IsNullOrEmpty(_options.Name))
                {
                    basicSyntax.Add(new ColoredString(_options.Name, _options.Syntax?.CommandNameColor));
                    basicSyntax.Add(" ");
                }

                basicSyntax.Add(info.GetBasicSyntax(includeOptionalParameters: true));

                sections.Add(new Section(_options, _options.Syntax, new[] { new ColoredMultistring(basicSyntax) }));
            }

            if ((_options.Description?.Include ?? false) && !string.IsNullOrEmpty(info.Description))
            {
                sections.Add(new Section(_options, _options.Description, info.Description));
            }

            // If needed, get help info for enum values.
            IReadOnlyDictionary<ArgumentUsageInfo, List<IEnumArgumentType>> inlineDocumented = null;
            IEnumerable<IEnumArgumentType> separatelyDocumented = null;
            if (_options.EnumValues?.Include ?? false)
            {
                GetEnumsToDocument(info, out inlineDocumented, out separatelyDocumented);
            }

            // If desired (and present), append "REQUIRED PARAMETERS" section.
            if ((_options.Arguments?.RequiredArguments?.Include ?? false) &&
                info.RequiredParameters.Any())
            {
                var entries = GetAndFormatParameterEntries(_options.Arguments.RequiredArguments, info.RequiredParameters, info, inlineDocumented).ToList();
                if (entries.Count > 0)
                {
                    sections.Add(new Section(_options, _options.Arguments.RequiredArguments, entries));
                }
            }

            // If desired (and present), append "OPTIONAL PARAMETERS" section.
            if ((_options.Arguments?.OptionalArguments?.Include ?? false) &&
                info.OptionalParameters.Any())
            {
                var entries = GetAndFormatParameterEntries(_options.Arguments.OptionalArguments, info.OptionalParameters, info, inlineDocumented).ToList();
                if (entries.Count > 0)
                {
                    sections.Add(new Section(_options, _options.Arguments.OptionalArguments, entries));
                }
            }

            // If needed, provide help for shared enum values.
            if (separatelyDocumented != null)
            {
                foreach (var enumType in separatelyDocumented)
                {
                    var maxWidth = _options.MaxWidth.GetValueOrDefault(ArgumentSetHelpOptions.DefaultMaxWidth);
                    maxWidth -= _options.EnumValues.BlockIndent.GetValueOrDefault(_options.SectionEntryBlockIndentWidth);

                    var enumValueEntries = GetEnumValueEntries(maxWidth, enumType).ToList();
                    if (enumValueEntries.Count > 0)
                    {
                        sections.Add(new Section(_options,
                            _options.EnumValues,
                            enumValueEntries,
                            name: string.Format(Strings.UsageInfoEnumValueHeaderFormat, enumType.DisplayName)));
                    }
                }
            }

            // If present, append "EXAMPLES" section.
            if ((_options.Examples?.Include ?? false) && info.Examples.Any())
            {
                sections.Add(new Section(_options, _options.Examples, info.Examples));
            }

            return sections;
        }

        private static void GetEnumsToDocument(ArgumentSetUsageInfo setInfo,
            out IReadOnlyDictionary<ArgumentUsageInfo, List<IEnumArgumentType>> inlineDocumented,
            out IEnumerable<IEnumArgumentType> separatelyDocumented)
        {
            var enumTypeMap = GetAllArgumentTypeMap(setInfo, t => t is IEnumArgumentType);

            var id = new Dictionary<ArgumentUsageInfo, List<IEnumArgumentType>>();
            inlineDocumented = id;

            foreach (var pair in enumTypeMap.Where(e => e.Value.Count == 1))
            {
                var newKey = pair.Value.Single();
                if (!id.TryGetValue(newKey, out List<IEnumArgumentType> types))
                {
                    types = new List<IEnumArgumentType>();
                    id[newKey] = types;
                }

                types.Add((IEnumArgumentType)pair.Key);
            }

            separatelyDocumented = enumTypeMap
                .Where(e => e.Value.Count > 1)
                .Select(e => (IEnumArgumentType)e.Key);
        }

        private IEnumerable<ColoredMultistring> GetEnumValueEntries(
            int currentMaxWidth,
            IEnumArgumentType type)
        {
            var values = type.GetValues()
                .Where(v => !v.Disallowed && !v.Hidden)
                .OrderBy(v => v.DisplayName)
                .ToList();

            // See if we can collapse the values onto one entry.
            if (values.All(v => string.IsNullOrEmpty(v.Description) && v.ShortName == null))
            {
                return new[] { new ColoredMultistring($"{{{string.Join(", ", values.Select(v => v.DisplayName))}}}") };
            }
            else
            {
                var entries = values.Select(GetEnumValueInfo);
                return FormatParameterEntries(currentMaxWidth, entries);
            }
        }

        private ParameterEntry GetEnumValueInfo(IArgumentValue value)
        {
            var syntaxBuilder = new ColoredMultistringBuilder();

            if (_options.Arguments.ShortName == ArgumentShortNameHelpMode.IncludeWithLongName &&
                !string.IsNullOrEmpty(value.ShortName))
            {
                syntaxBuilder.Append(new ColoredString(value.ShortName, _options.Arguments?.ArgumentNameColor));
                syntaxBuilder.Append(", ");
            }

            syntaxBuilder.Append(new ColoredString(value.DisplayName, _options.Arguments?.ArgumentNameColor));

            var descBuilder = new ColoredMultistringBuilder();
            if (!string.IsNullOrEmpty(value.Description))
            {
                descBuilder.Append(value.Description);
            }

            if (_options.Arguments.ShortName == ArgumentShortNameHelpMode.AppendToDescription &&
                !string.IsNullOrEmpty(value.ShortName))
            {
                descBuilder.Append(" [");
                descBuilder.Append(new ColoredString(Strings.UsageInfoShortForm, _options.Arguments.MetadataColor));
                descBuilder.Append(" ");
                descBuilder.Append(new ColoredString(value.ShortName, _options.Arguments?.ArgumentNameColor));
                descBuilder.Append("]");
            }

            var desc = descBuilder.ToMultistring();

            return new ParameterEntry
            {
                Syntax = syntaxBuilder.ToMultistring(),
                Description = descBuilder.ToMultistring(),
                InlineEnumEntries = null
            };
        }

        private static IEnumerable<IArgumentType> GetDependencyTransitiveClosure(IArgumentType type)
        {
            var types = new HashSet<IArgumentType>();
            var typesToProcess = new Queue<IArgumentType>();

            typesToProcess.Enqueue(type);

            while (typesToProcess.Count > 0)
            {
                var nextType = typesToProcess.Dequeue();
                if (!types.Add(nextType))
                {
                    continue;
                }

                foreach (var newType in nextType.DependentTypes.Where(t => !types.Contains(t)))
                {
                    typesToProcess.Enqueue(newType);
                }
            }

            return types;
        }

        private static IReadOnlyDictionary<IArgumentType, List<ArgumentUsageInfo>> GetAllArgumentTypeMap(ArgumentSetUsageInfo setInfo, Func<IArgumentType, bool> typeFilterFunc)
        {
            var map = new Dictionary<IArgumentType, List<ArgumentUsageInfo>>(new ArgumentTypeComparer());
            foreach (var parameter in setInfo.AllParameters.Where(p => p.ArgumentType != null))
            {
                foreach (var type in GetDependencyTransitiveClosure(parameter.ArgumentType).Where(typeFilterFunc))
                {
                    if (!map.TryGetValue(type, out List<ArgumentUsageInfo> infoList))
                    {
                        infoList = new List<ArgumentUsageInfo>();
                        map[type] = infoList;
                    }

                    infoList.Add(parameter);
                }
            }

            return map;
        }

        private IEnumerable<ColoredMultistring> GetAndFormatParameterEntries(
            ArgumentMetadataHelpOptions itemOptions,
            IEnumerable<ArgumentUsageInfo> info,
            ArgumentSetUsageInfo setInfo,
            IReadOnlyDictionary<ArgumentUsageInfo, List<IEnumArgumentType>> inlineDocumentedEnums)
        {
            var currentMaxWidth = _options.MaxWidth.GetValueOrDefault(ArgumentSetHelpOptions.DefaultMaxWidth);
            currentMaxWidth -= itemOptions.BlockIndent.GetValueOrDefault(_options.SectionEntryBlockIndentWidth);

            var entries = GetParameterEntries(currentMaxWidth, info, setInfo, inlineDocumentedEnums);
            return FormatParameterEntries(currentMaxWidth, entries);
        }

        private IEnumerable<ColoredMultistring> FormatParameterEntries(
            int currentMaxWidth,
            IEnumerable<ParameterEntry> entries)
        {
            IEnumerable<IEnumerable<ColoredMultistring>> formatted;
            if (_options.Arguments.Layout is OneColumnArgumentHelpLayout)
            {
                formatted = FormatParameterEntriesInOneColumn(entries);
            }
            else if (_options.Arguments.Layout is TwoColumnArgumentHelpLayout layout)
            {
                formatted = FormatParameterEntriesInTwoColumns(currentMaxWidth, layout, entries);
            }
            else
            {
                throw new NotSupportedException("Unsupported argument help layout");
            }

            if (_options.Arguments.BlankLinesBetweenArguments > 0)
            {
                var insertion = new[]
                {
                    new ColoredMultistring(
                        Enumerable.Repeat(new ColoredString(Environment.NewLine), _options.Arguments.BlankLinesBetweenArguments))
                };

                formatted = formatted.InsertBetween(insertion);
            }

            return formatted.Flatten();
        }

        private IEnumerable<IEnumerable<ColoredMultistring>> FormatParameterEntriesInOneColumn(
            IEnumerable<ParameterEntry> entries) =>
            entries.Select(e =>
            {
                var builder = new ColoredMultistringBuilder();

                builder.Append(e.Syntax);

                if (!e.Description.IsEmpty())
                {
                    builder.Append(" - ");
                    builder.Append(e.Description);
                }

                IEnumerable<ColoredMultistring> composed = new[] { builder.ToMultistring() };

                if (e.InlineEnumEntries != null)
                {
                    if (_options.Arguments.BlankLinesBetweenArguments > 0)
                    {
                        var insertion = new[]
                        {
                            new ColoredMultistring(
                                Enumerable.Repeat(new ColoredString(Environment.NewLine), _options.Arguments.BlankLinesBetweenArguments))
                        };

                        composed = composed.Concat(insertion);
                    }

                    // TODO: Let section hanging indent override.
                    var indent = new string(' ', _options.SectionEntryHangingIndentWidth);
                    composed = composed.Concat(e.InlineEnumEntries.Select(iee => indent + iee));
                }

                return composed;
            });

        private IEnumerable<IEnumerable<ColoredMultistring>> FormatParameterEntriesInTwoColumns(
            int currentMaxWidth,
            TwoColumnArgumentHelpLayout layout,
            IEnumerable<ParameterEntry> entries)
        {
            var entriesList = entries.ToList();
            if (entriesList.Count == 0)
            {
                return Enumerable.Empty<IEnumerable<ColoredMultistring>>();
            }

            if (layout.FirstLineColumnSeparator != null && layout.DefaultColumnSeparator != null &&
                layout.FirstLineColumnSeparator.Length != layout.DefaultColumnSeparator.Length)
            {
                throw new NotSupportedException("Default and first-line column separators must have the same length");
            }

            var totalWidths = layout.ColumnWidths.Sum(i => i.GetValueOrDefault(0)) + layout.DefaultColumnSeparator.Length;
            if (totalWidths > currentMaxWidth)
            {
                throw new NotSupportedException("Column widths are too wide.");
            }

            if (layout.ColumnWidths.Any(c => c.HasValue && c.Value == 0))
            {
                throw new NotSupportedException("Invalid column width 0.");
            }

            var columnWidths = new int[2]
            {
                layout.ColumnWidths[0].GetValueOrDefault(0),
                layout.ColumnWidths[1].GetValueOrDefault(0)
            };

            if (columnWidths[0] == 0 && columnWidths[1] == 0)
            {
                columnWidths[0] = entriesList.Max(e => e.Syntax.Length);
                if (columnWidths[0] > currentMaxWidth)
                {
                    columnWidths[0] = currentMaxWidth / 2;
                }
            }

            if (columnWidths[0] == 0) columnWidths[0] = currentMaxWidth - layout.DefaultColumnSeparator.Length - columnWidths[1];
            if (columnWidths[1] == 0) columnWidths[1] = currentMaxWidth - layout.DefaultColumnSeparator.Length - columnWidths[0];

            return entriesList.Select(e =>
            {
                var wrappedSyntaxLines = e.Syntax.Wrap(columnWidths[0]).Split('\n').ToList();
                var wrappedDescLines = e.Description.Wrap(columnWidths[1]).Split('\n').ToList();

                var lineCount = Math.Max(wrappedSyntaxLines.Count, wrappedDescLines.Count);
                while (wrappedSyntaxLines.Count < lineCount)
                {
                    wrappedSyntaxLines.Add(ColoredMultistring.Empty);
                }
                while (wrappedDescLines.Count < lineCount)
                {
                    wrappedDescLines.Add(ColoredMultistring.Empty);
                }

                wrappedSyntaxLines = wrappedSyntaxLines.Select(line => RightPadWithSpace(line, columnWidths[0])).ToList();
                wrappedDescLines = wrappedDescLines.Select(line => RightPadWithSpace(line, columnWidths[1])).ToList();

                List<ColoredMultistring> linesList = new List<ColoredMultistring>();
                for (var i = 0; i < wrappedSyntaxLines.Count; ++i)
                {
                    var left = wrappedSyntaxLines[i];
                    var right = wrappedDescLines[i];

                    if (string.IsNullOrWhiteSpace(right.ToString()))
                    {
                        linesList.Add((ColoredMultistring)left);
                    }
                    else
                    {
                        var separator = layout.DefaultColumnSeparator;
                        if (i == 0 && layout.FirstLineColumnSeparator != null)
                        {
                            separator = layout.FirstLineColumnSeparator;
                        }

                        linesList.Add((ColoredMultistring)left + separator + (ColoredMultistring)right);
                    }
                }

                IEnumerable<ColoredMultistring> lines = linesList;

                if (e.InlineEnumEntries != null)
                {
                    if (_options.Arguments.BlankLinesBetweenArguments > 0)
                    {
                        var insertion = new[]
                        {
                            new ColoredMultistring(
                                Enumerable.Repeat(new ColoredString(Environment.NewLine), _options.Arguments.BlankLinesBetweenArguments))
                        };

                        lines = lines.Concat(insertion);
                    }

                    // TODO: Let section hanging indent override.
                    var indent = new string(' ', _options.SectionEntryHangingIndentWidth);
                    lines = lines.Concat(e.InlineEnumEntries.Select(iee => indent + iee));
                }

                return lines;
            });
        }

        private IEnumerable<ParameterEntry> GetParameterEntries(
            int currentMaxWidth,
            IEnumerable<ArgumentUsageInfo> info,
            ArgumentSetUsageInfo setInfo,
            IReadOnlyDictionary<ArgumentUsageInfo, List<IEnumArgumentType>> inlineDocumentedEnums) =>
            info.Select(i =>
            {
                List<IEnumArgumentType> enumTypes = null;
                inlineDocumentedEnums?.TryGetValue(i, out enumTypes);

                // N.B. Special-case parent command groups that are already selected (i.e. skip them).
                if (i.IsSelectedCommand())
                {
                    return null;
                }

                return FormatParameterEntry(currentMaxWidth, i, setInfo, enumTypes);
            }).Where(e => e != null);

        private ParameterEntry FormatParameterEntry(
            int currentMaxWidth,
            ArgumentUsageInfo info,
            ArgumentSetUsageInfo setInfo,
            List<IEnumArgumentType> inlineDocumentedEnums)
        {
            List<ColoredMultistring> enumValueEntries = null;

            if (inlineDocumentedEnums?.Count > 0)
            {
                enumValueEntries = inlineDocumentedEnums.SelectMany(
                    e => GetEnumValueEntries(currentMaxWidth - _options.SectionEntryHangingIndentWidth, e))
                    .ToList();

                if (enumValueEntries.Count == 0)
                {
                    enumValueEntries = null;
                }
            }

            return new ParameterEntry
            {
                Syntax = FormatParameterSyntax(info, setInfo, inlineDocumentedEnums?.Count > 0),
                Description = FormatParameterDescription(info, setInfo),
                // TODO: Let section hanging indent override.
                InlineEnumEntries = enumValueEntries
            };
        }

        private ColoredMultistring FormatParameterSyntax(ArgumentUsageInfo info, ArgumentSetUsageInfo setInfo, bool suppressTypeInfo)
        {
            if (!_options.Arguments.IncludePositionalArgumentTypes) suppressTypeInfo = true;

            var originalSyntax = suppressTypeInfo ? info.Syntax : info.DetailedSyntax;
            var syntax = SimplifyParameterSyntax(originalSyntax);

            var formattedSyntax = new List<ColoredString>();

            // First add parameter's short name (if it has one and if it's supposed to be here).
            if (_options.Arguments.ShortName == ArgumentShortNameHelpMode.IncludeWithLongName &&
                !string.IsNullOrEmpty(info.ShortName) &&
                !string.IsNullOrEmpty(setInfo.DefaultShortNamePrefix))
            {
                formattedSyntax.Add(new ColoredString(setInfo.DefaultShortNamePrefix + info.ShortName, _options.Arguments?.ArgumentNameColor));
                formattedSyntax.Add(", ");
            }

            formattedSyntax.Add(new ColoredString(syntax, _options.Arguments?.ArgumentNameColor));

            return new ColoredMultistring(formattedSyntax);
        }

        private ColoredMultistring FormatParameterDescription(
            ArgumentUsageInfo info,
            ArgumentSetUsageInfo setInfo)
        {
            var builder = new ColoredMultistringBuilder();

            List<ColoredString> defaultValueContent = null;
            if (_options.Arguments.DefaultValue != ArgumentDefaultValueHelpMode.Omit &&
                !string.IsNullOrEmpty(info.DefaultValue))
            {
                defaultValueContent = new List<ColoredString>
                {
                    new ColoredString(Strings.UsageInfoDefaultValue, _options.Arguments.MetadataColor),
                    " ",
                    info.DefaultValue
                };
            }

            if (_options.Arguments.DefaultValue == ArgumentDefaultValueHelpMode.PrependToDescription &&
                defaultValueContent != null)
            {
                builder.Append("[");
                builder.Append(defaultValueContent);
                builder.Append("]");
            }

            if (_options.Arguments.IncludeDescription && !string.IsNullOrEmpty(info.Description))
            {
                if (builder.Length > 0)
                {
                    builder.Append(" ");
                }

                builder.Append(info.Description);
            }

            var metadataItems = new List<List<ColoredString>>();

            if (_options.Arguments.DefaultValue == ArgumentDefaultValueHelpMode.AppendToDescription &&
                defaultValueContent != null)
            {
                metadataItems.Add(defaultValueContent);
            }

            // Append parameter's short name (if it has one).
            if (_options.Arguments.ShortName == ArgumentShortNameHelpMode.AppendToDescription &&
                !string.IsNullOrEmpty(info.ShortName) &&
                !string.IsNullOrEmpty(setInfo.DefaultShortNamePrefix))
            {
                metadataItems.Add(new List<ColoredString>
                {
                    new ColoredString(Strings.UsageInfoShortForm, _options.Arguments.MetadataColor),
                    " ",
                    new ColoredString(setInfo.DefaultShortNamePrefix + info.ShortName, _options.Arguments?.ArgumentNameColor)
                });
            }

            if (metadataItems.Count > 0)
            {
                if (builder.Length > 0)
                {
                    builder.Append(" ");
                }

                builder.Append("[");
                builder.Append(metadataItems.InsertBetween(new List<ColoredString> { ", " }).Flatten());
                builder.Append("]");
            }

            return builder.ToMultistring();
        }

        // We add logic here to trim out a single pair of enclosing
        // square brackets if it's present -- it's just noisy here.
        // The containing section already makes it sufficiently clear
        // whether the parameter is required or optional.
        //
        // TODO: Make this logic more generic, and put it elsewhere.
        private static string SimplifyParameterSyntax(string s) =>
            s.TrimStart('[')
             .TrimEnd(']', '*', '+');

        private static IString RightPadWithSpace(IString s, int length)
        {
            if (s.Length >= length) return s;

            var builder = s.CreateNewBuilder();

            builder.Append(s);
            builder.Append(new string(' ', length - s.Length));

            return builder.Generate();
        }

        private class ArgumentTypeComparer : IEqualityComparer<IArgumentType>
        {
            public bool Equals(IArgumentType x, IArgumentType y) =>
                x.Type.GetTypeInfo().GUID == y.Type.GetTypeInfo().GUID;

            public int GetHashCode(IArgumentType value) =>
                value.Type.GetTypeInfo().GUID.GetHashCode();
        }

        private sealed class Section
        {
            public Section(ArgumentSetHelpOptions options, ArgumentMetadataHelpOptions itemOptions, IEnumerable<ColoredMultistring> entries, string name = null)
            {
                Entries = entries.ToList();
                Name = name ?? itemOptions.HeaderTitle;
                BodyIndentWidth = itemOptions.BlockIndent.GetValueOrDefault(options.SectionEntryBlockIndentWidth);
                HangingIndentWidth = itemOptions.HangingIndent.GetValueOrDefault(options.SectionEntryHangingIndentWidth);
            }

            public Section(ArgumentSetHelpOptions options, ArgumentMetadataHelpOptions itemOptions, ColoredMultistring entry)
                : this(options, itemOptions, new[] { entry })
            {
            }

            public Section(ArgumentSetHelpOptions options, ArgumentMetadataHelpOptions itemOptions, IEnumerable<ColoredString> entries)
                : this(options, itemOptions, entries.Select(e => (ColoredMultistring)e))
            {
            }

            public Section(ArgumentSetHelpOptions options, ArgumentMetadataHelpOptions itemOptions, IEnumerable<string> entries)
                : this(options, itemOptions, entries.Select(e => (ColoredString)e))
            {
            }

            public Section(ArgumentSetHelpOptions options, ArgumentMetadataHelpOptions itemOptions, ColoredString entry)
                : this(options, itemOptions, new[] { entry })
            {
            }

            public int BodyIndentWidth { get; set; }

            public int HangingIndentWidth { get; set; }

            public string Name { get; }

            public IReadOnlyList<ColoredMultistring> Entries { get; set; }
        }

        private class ParameterEntry
        {
            public ColoredMultistring Syntax { get; set; }

            public ColoredMultistring Description { get; set; }

            public IEnumerable<ColoredMultistring> InlineEnumEntries { get; set; }
        }
    }
}