using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NClap.Types;
using NClap.Utilities;

namespace NClap.Parser
{
    /// <summary>
    /// Help formatter that optimizes for vertical compactness.
    /// </summary>
    internal class CondensedHelpFormatter : HelpFormatter
    {
        public ConsoleColor? NameForegroundColor { get; set; } = ConsoleColor.White;
        public ConsoleColor? ParameterNameForegroundColor { get; set; } = ConsoleColor.White;

        public override ColoredMultistring Format(ArgumentSetUsageInfo info) => Format(GenerateSections(info));

        private IReadOnlyList<Section> GenerateSections(ArgumentSetUsageInfo info)
        {
            var sections = new List<Section>();

            if (Options.HasFlag(UsageInfoOptions.IncludeLogo) &&
                info.Logo != null && !info.Logo.IsEmpty())
            {
                sections.Add(new Section(null, info.Logo)
                {
                    BodyIndentWidth = 0
                });
            }

            // Append basic usage info.
            if (Options.HasFlag(UsageInfoOptions.IncludeBasicSyntax))
            {
                var basicSyntax = new ColoredString[]
                {
                    new ColoredString(info.Name, NameForegroundColor),
                    " ",
                    info.GetBasicSyntax(includeOptionalParameters: true)
                };

                sections.Add(new Section(Strings.UsageInfoUsageHeader + ":", new[] { new ColoredMultistring(basicSyntax) })
                {
                    BodyIndentWidth = Section.DefaultIndent * 3 / 2,
                    HangingIndentWidth = Section.DefaultIndent / 2
                });
            }

            if (Options.HasFlag(UsageInfoOptions.IncludeDescription) && !string.IsNullOrEmpty(info.Description))
            {
                sections.Add(new Section(null, info.Description));
            }

            // If needed, get help info for enum values.
            IReadOnlyDictionary<ArgumentUsageInfo, List<IEnumArgumentType>> inlineDocumented = null;
            IEnumerable<IEnumArgumentType> separatelyDocumented = null;
            if (Options.HasFlag(UsageInfoOptions.IncludeEnumValues))
            {
                GetEnumsToDocument(info, out inlineDocumented, out separatelyDocumented);
            }

            // If desired (and present), append "REQUIRED PARAMETERS" section.
            if (Options.HasFlag(UsageInfoOptions.IncludeRequiredParameterDescriptions) &&
                info.RequiredParameters.Any())
            {
                var entries = GetParameterEntries(info.RequiredParameters, info, inlineDocumented).ToList();
                if (entries.Count > 0)
                {
                    sections.Add(new Section(Strings.UsageInfoRequiredParametersHeader + ":", entries)
                    {
                        BodyIndentWidth = Section.DefaultIndent * 3 / 2,
                        HangingIndentWidth = Section.DefaultIndent / 2
                    });
                }
            }

            // If desired (and present), append "OPTIONAL PARAMETERS" section.
            if (Options.HasFlag(UsageInfoOptions.IncludeOptionalParameterDescriptions) &&
                info.OptionalParameters.Any())
            {
                var entries = GetParameterEntries(info.OptionalParameters, info, inlineDocumented).ToList();
                if (entries.Count > 0)
                {
                    sections.Add(new Section(Strings.UsageInfoOptionalParametersHeader + ":", entries)
                    {
                        BodyIndentWidth = Section.DefaultIndent * 3 / 2,
                        HangingIndentWidth = Section.DefaultIndent / 2
                    });
                }
            }

            // If needed, provide help for shared enum values.
            if (separatelyDocumented != null)
            {
                foreach (var enumType in separatelyDocumented)
                {
                    sections.Add(new Section(
                        string.Format(Strings.UsageInfoEnumValueHeaderFormat, enumType.DisplayName),
                        GetEnumValueEntries(enumType))
                    {
                        BodyIndentWidth = Section.DefaultIndent * 3 / 2,
                        HangingIndentWidth = Section.DefaultIndent / 2
                    });
                }
            }

            // If present, append "EXAMPLES" section.
            if (Options.HasFlag(UsageInfoOptions.IncludeExamples) && info.Examples.Any())
            {
                sections.Add(new Section(Strings.UsageInfoExamplesHeader + ":", info.Examples));
            }

            // If requested, display remarks
            if (Options.HasFlag(UsageInfoOptions.IncludeRemarks) && !string.IsNullOrEmpty(info.Remarks))
            {
                sections.Add(new Section(Strings.UsageInfoRemarksHeader + ":", info.Remarks));
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

            // See if we can collapse the values onto one entry.
            if (values.All(v => string.IsNullOrEmpty(v.Description) && v.ShortName == null))
            {
                return new[] { new ColoredMultistring($"{{{string.Join(", ", values.Select(v => v.DisplayName))}}}") };
            }
            else
            {
                return values.Select(FormatEnumValueInfo);
            }
        }
            
        private ColoredMultistring FormatEnumValueInfo(IArgumentValue value)
        {
            var builder = new ColoredMultistringBuilder();
            builder.Append(new ColoredString(value.DisplayName, ParameterNameForegroundColor));

            if (!string.IsNullOrEmpty(value.Description))
            {
                builder.Append(" - ");
                builder.Append(value.Description);
            }

            if (!string.IsNullOrEmpty(value.ShortName))
            {
                builder.Append(" [");
                builder.Append(new ColoredString(Strings.UsageInfoShortForm, ParameterMetadataForegroundColor));
                builder.Append(" ");
                builder.Append(new ColoredString(value.ShortName, NameForegroundColor));
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
            var entries = info.SelectMany(i =>
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

            return entries;
        }

        private IEnumerable<ColoredMultistring> FormatParameterInfo(
            ArgumentUsageInfo info,
            ArgumentSetUsageInfo setInfo,
            List<IEnumArgumentType> inlineDocumentedEnums)
        {
            var builder = new ColoredMultistringBuilder();

            var syntax = SimplifyParameterSyntax(info.DetailedSyntax);
            builder.Append(new ColoredString(syntax, ParameterNameForegroundColor));

            if (!string.IsNullOrEmpty(info.Description))
            {
                builder.Append(" - ");
                builder.Append(info.Description);
            }

            var metadataItems = new List<List<ColoredString>>();

            if (Options.HasFlag(UsageInfoOptions.IncludeParameterDefaultValues) &&
                !string.IsNullOrEmpty(info.DefaultValue))
            {
                metadataItems.Add(new List<ColoredString>
                {
                    new ColoredString(Strings.UsageInfoDefaultValue, ParameterMetadataForegroundColor),
                    " ",
                    info.DefaultValue
                });
            }

            // Append parameter's short name (if it has one).
            if (Options.HasFlag(UsageInfoOptions.IncludeParameterShortNameAliases) &&
                !string.IsNullOrEmpty(info.ShortName) &&
                !string.IsNullOrEmpty(setInfo.DefaultShortNamePrefix))
            {
                metadataItems.Add(new List<ColoredString>
                {
                    new ColoredString(Strings.UsageInfoShortForm, ParameterMetadataForegroundColor),
                    " ",
                    new ColoredString(setInfo.DefaultShortNamePrefix + info.ShortName, NameForegroundColor)
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
                        new string(' ', Section.DefaultIndent)
                    }.Concat(entry.Content));
                    formattedInfo.Add(indentedEntry);
                }
            }

            return formattedInfo;
        }

        private class ArgumentTypeComparer : IEqualityComparer<IArgumentType>
        {
            public bool Equals(IArgumentType x, IArgumentType y) =>
                x.Type.GetTypeInfo().GUID == y.Type.GetTypeInfo().GUID;

            public int GetHashCode(IArgumentType obj) =>
                obj.Type.GetTypeInfo().GUID.GetHashCode();
        }
    }
}