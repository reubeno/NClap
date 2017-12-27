using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NClap.Help;
using NClap.Types;
using NClap.Utilities;

namespace NClap.Parser
{
    /// <summary>
    /// Help formatter that optimizes for vertical compactness.
    /// </summary>
    internal class CondensedHelpFormatter : HelpFormatter
    {
        public override ColoredMultistring Format(ArgumentSetUsageInfo info) => Format(GenerateSections(info));

        private IReadOnlyList<Section> GenerateSections(ArgumentSetUsageInfo info)
        {
            var sections = new List<Section>();

            if (Options.Logo?.Include ?? false &&
                info.Logo != null && !info.Logo.IsEmpty())
            {
                sections.Add(new Section(Options, Options.Logo, info.Logo));
            }

            // Append basic usage info.
            if (Options.Syntax?.Include ?? false)
            {
                var basicSyntax = new List<ColoredString>();

                if (!string.IsNullOrEmpty(info.Name))
                {
                    basicSyntax.Add(new ColoredString(info.Name, Options.Syntax?.CommandNameColor));
                    basicSyntax.Add(" ");
                }

                basicSyntax.Add(info.GetBasicSyntax(includeOptionalParameters: true));

                sections.Add(new Section(Options, Options.Syntax, new[] { new ColoredMultistring(basicSyntax) }));
            }

            if ((Options.Description?.Include ?? false) && !string.IsNullOrEmpty(info.Description))
            {
                sections.Add(new Section(Options, Options.Description, info.Description));
            }

            // If needed, get help info for enum values.
            IReadOnlyDictionary<ArgumentUsageInfo, List<IEnumArgumentType>> inlineDocumented = null;
            IEnumerable<IEnumArgumentType> separatelyDocumented = null;
            if (Options.EnumValues?.Include ?? false)
            {
                GetEnumsToDocument(info, out inlineDocumented, out separatelyDocumented);
            }

            // If desired (and present), append "REQUIRED PARAMETERS" section.
            if ((Options.Arguments?.RequiredArguments?.Include ?? false) &&
                info.RequiredParameters.Any())
            {
                var entries = GetParameterEntries(info.RequiredParameters, info, inlineDocumented).ToList();
                if (entries.Count > 0)
                {
                    sections.Add(new Section(Options, Options.Arguments.RequiredArguments, entries));
                }
            }

            // If desired (and present), append "OPTIONAL PARAMETERS" section.
            if ((Options.Arguments?.OptionalArguments?.Include ?? false) &&
                info.OptionalParameters.Any())
            {
                var entries = GetParameterEntries(info.OptionalParameters, info, inlineDocumented).ToList();
                if (entries.Count > 0)
                {
                    sections.Add(new Section(Options, Options.Arguments.OptionalArguments, entries));
                }
            }

            // If needed, provide help for shared enum values.
            if (separatelyDocumented != null)
            {
                foreach (var enumType in separatelyDocumented)
                {
                    sections.Add(new Section(Options,
                        Options.EnumValues,
                        GetEnumValueEntries(enumType),
                        name: string.Format(Strings.UsageInfoEnumValueHeaderFormat, enumType.DisplayName)));
                }
            }

            // If present, append "EXAMPLES" section.
            if ((Options.Examples?.Include ?? false) && info.Examples.Any())
            {
                sections.Add(new Section(Options, Options.Examples, info.Examples));
            }

            // If requested, display remarks
            if ((Options.Remarks?.Include ?? false) && !string.IsNullOrEmpty(info.Remarks))
            {
                sections.Add(new Section(Options, Options.Remarks, info.Remarks));
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
            builder.Append(new ColoredString(value.DisplayName, Options.Arguments?.ArgumentNameColor));

            if (!string.IsNullOrEmpty(value.Description))
            {
                builder.Append(" - ");
                builder.Append(value.Description);
            }

            if (!string.IsNullOrEmpty(value.ShortName))
            {
                builder.Append(" [");
                builder.Append(new ColoredString(Strings.UsageInfoShortForm, Options.Arguments.MetadataColor));
                builder.Append(" ");
                builder.Append(new ColoredString(value.ShortName, Options.Arguments?.ArgumentNameColor));
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
            builder.Append(new ColoredString(syntax, Options.Arguments?.ArgumentNameColor));

            if (Options.Arguments.IncludeDescription && !string.IsNullOrEmpty(info.Description))
            {
                builder.Append(" - ");
                builder.Append(info.Description);
            }

            var metadataItems = new List<List<ColoredString>>();

            if (Options.Arguments.DefaultValue == ArgumentDefaultValueHelpMode.AppendToDescription &&
                !string.IsNullOrEmpty(info.DefaultValue))
            {
                metadataItems.Add(new List<ColoredString>
                {
                    new ColoredString(Strings.UsageInfoDefaultValue, Options.Arguments.MetadataColor),
                    " ",
                    info.DefaultValue
                });
            }

            // Append parameter's short name (if it has one).
            if (Options.Arguments.ShortName == ArgumentShortNameHelpMode.AppendToDescription &&
                !string.IsNullOrEmpty(info.ShortName) &&
                !string.IsNullOrEmpty(setInfo.DefaultShortNamePrefix))
            {
                metadataItems.Add(new List<ColoredString>
                {
                    new ColoredString(Strings.UsageInfoShortForm, Options.Arguments.MetadataColor),
                    " ",
                    new ColoredString(setInfo.DefaultShortNamePrefix + info.ShortName, Options.Arguments?.ArgumentNameColor)
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
                        new string(' ', Options.SectionEntryBlockIndentWidth)
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