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
    internal class ArgumentSetHelpRenderer
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

                if (!string.IsNullOrEmpty(info.Name))
                {
                    basicSyntax.Add(new ColoredString(info.Name, _options.Syntax?.CommandNameColor));
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
                var entries = GetParameterEntries(info.RequiredParameters, info, inlineDocumented).ToList();
                if (entries.Count > 0)
                {
                    sections.Add(new Section(_options, _options.Arguments.RequiredArguments, entries));
                }
            }

            // If desired (and present), append "OPTIONAL PARAMETERS" section.
            if ((_options.Arguments?.OptionalArguments?.Include ?? false) &&
                info.OptionalParameters.Any())
            {
                var entries = GetParameterEntries(info.OptionalParameters, info, inlineDocumented).ToList();
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
                    sections.Add(new Section(_options,
                        _options.EnumValues,
                        GetEnumValueEntries(enumType),
                        name: string.Format(Strings.UsageInfoEnumValueHeaderFormat, enumType.DisplayName)));
                }
            }

            // If present, append "EXAMPLES" section.
            if ((_options.Examples?.Include ?? false) && info.Examples.Any())
            {
                sections.Add(new Section(_options, _options.Examples, info.Examples));
            }

            // If requested, display remarks
            if ((_options.Remarks?.Include ?? false) && !string.IsNullOrEmpty(info.Remarks))
            {
                sections.Add(new Section(_options, _options.Remarks, info.Remarks));
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

        private IEnumerable<ColoredMultistring> GetEnumValueEntries(IEnumArgumentType type)
        {
            var values = type.GetValues()
                .Where(v => !v.Disallowed && !v.Hidden)
                .OrderBy(v => v.DisplayName)
                .ToList();

            IEnumerable<ColoredMultistring> entries;

            // See if we can collapse the values onto one entry.
            if (values.All(v => string.IsNullOrEmpty(v.Description) && v.ShortName == null))
            {
                entries = new[] { new ColoredMultistring($"{{{string.Join(", ", values.Select(v => v.DisplayName))}}}") };
            }
            else
            {
                entries = values.Select(FormatEnumValueInfo);
            }

            if ((_options.Arguments?.BlankLinesBetweenArguments ?? 0) > 0)
            {
                var insertion = new ColoredMultistring(
                    Enumerable.Repeat(new ColoredString(Environment.NewLine), _options.Arguments.BlankLinesBetweenArguments));

                entries = entries.InsertBetween(insertion);
            }

            return entries;
        }
            
        private ColoredMultistring FormatEnumValueInfo(IArgumentValue value)
        {
            var builder = new ColoredMultistringBuilder();
            builder.Append(new ColoredString(value.DisplayName, _options.Arguments?.ArgumentNameColor));

            if (!string.IsNullOrEmpty(value.Description))
            {
                builder.Append(" - ");
                builder.Append(value.Description);
            }

            if (!string.IsNullOrEmpty(value.ShortName))
            {
                builder.Append(" [");
                builder.Append(new ColoredString(Strings.UsageInfoShortForm, _options.Arguments.MetadataColor));
                builder.Append(" ");
                builder.Append(new ColoredString(value.ShortName, _options.Arguments?.ArgumentNameColor));
                builder.Append("]");
            }

            return builder.ToMultistring();
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
            if (typeFilterFunc == null)
            {
                throw new ArgumentNullException(nameof(typeFilterFunc));
            }

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

        private IEnumerable<ColoredMultistring> GetParameterEntries(
            IEnumerable<ArgumentUsageInfo> info,
            ArgumentSetUsageInfo setInfo,
            IReadOnlyDictionary<ArgumentUsageInfo, List<IEnumArgumentType>> inlineDocumentedEnums)
        {
            var entries = info.Select(i =>
            {
                List<IEnumArgumentType> enumTypes = null;
                inlineDocumentedEnums?.TryGetValue(i, out enumTypes);

                // N.B. Special-case parent command groups that are already selected (i.e. skip them).
                if (i.IsSelectedCommand())
                {
                    return Enumerable.Empty<ColoredMultistring>();
                }

                return FormatParameterInfo(i, setInfo, enumTypes);
            });

            if (_options.Arguments.BlankLinesBetweenArguments > 0)
            {
                var insertion = new[] { new ColoredMultistring(
                    Enumerable.Repeat(new ColoredString(Environment.NewLine), _options.Arguments.BlankLinesBetweenArguments)) };

                entries = entries.InsertBetween(insertion);
            }

            return entries.Flatten();
        }

        private IEnumerable<ColoredMultistring> FormatParameterInfo(
            ArgumentUsageInfo info,
            ArgumentSetUsageInfo setInfo,
            List<IEnumArgumentType> inlineDocumentedEnums)
        {
            var builder = new ColoredMultistringBuilder();

            var syntax = SimplifyParameterSyntax(info.DetailedSyntax);
            builder.Append(new ColoredString(syntax, _options.Arguments?.ArgumentNameColor));

            if (_options.Arguments.IncludeDescription && !string.IsNullOrEmpty(info.Description))
            {
                builder.Append(" - ");
                builder.Append(info.Description);
            }

            var metadataItems = new List<List<ColoredString>>();

            if (_options.Arguments.DefaultValue == ArgumentDefaultValueHelpMode.AppendToDescription &&
                !string.IsNullOrEmpty(info.DefaultValue))
            {
                metadataItems.Add(new List<ColoredString>
                {
                    new ColoredString(Strings.UsageInfoDefaultValue, _options.Arguments.MetadataColor),
                    " ",
                    info.DefaultValue
                });
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
                builder.Append(" [");
                builder.Append(metadataItems.InsertBetween(new List<ColoredString> { ", " }).Flatten());
                builder.Append("]");
            }

            var formattedInfo = new List<ColoredMultistring> { builder.ToMultistring() };

            if (inlineDocumentedEnums?.Count > 0)
            {
                foreach (var entry in inlineDocumentedEnums.SelectMany(GetEnumValueEntries))
                {
                    var indentedEntry = new ColoredMultistring(new ColoredString[]
                    {
                        new string(' ', _options.SectionEntryBlockIndentWidth)
                    }.Concat(entry.Content));
                    formattedInfo.Add(indentedEntry);
                }
            }

            return formattedInfo;
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

        private class ArgumentTypeComparer : IEqualityComparer<IArgumentType>
        {
            public bool Equals(IArgumentType x, IArgumentType y) =>
                x.Type.GetTypeInfo().GUID == y.Type.GetTypeInfo().GUID;

            public int GetHashCode(IArgumentType obj) =>
                obj.Type.GetTypeInfo().GUID.GetHashCode();
        }

        private class Section
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