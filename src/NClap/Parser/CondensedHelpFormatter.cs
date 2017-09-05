using System;
using System.Collections.Generic;
using System.Linq;
using NClap.Types;
using NClap.Utilities;

namespace NClap.Parser
{
    internal class CondensedHelpFormatter : HelpFormatter
    {
        public ConsoleColor? NameForegroundColor { get; set; } = ConsoleColor.White;
        public ConsoleColor? ParameterNameForegroundColor { get; set; } = ConsoleColor.White;
        public bool SkipLineBetweenEntries { get; set; } = false;

        public override ColoredMultistring Format(ArgumentSetUsageInfo info) => Format(GenerateSections(info));

        private IReadOnlyList<Section> GenerateSections(ArgumentSetUsageInfo info)
        {
            var sections = new List<Section>();

            if (Options.HasFlag(UsageInfoOptions.IncludeLogo) && info.Logo != null)
            {
                sections.Add(new Section(null, info.Logo));
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

            // If desired (and present), append "REQUIRED PARAMETERS" section.
            if (Options.HasFlag(UsageInfoOptions.IncludeRequiredParameterDescriptions) &&
                info.RequiredParameters.Any())
            {
                var entries = GetParameterEntries(info.RequiredParameters, info);
                sections.Add(new Section(Strings.UsageInfoRequiredParametersHeader + ":", entries)
                {
                    BodyIndentWidth = Section.DefaultIndent * 3 / 2,
                    HangingIndentWidth = Section.DefaultIndent / 2
                });
            }

            // If desired (and present), append "OPTIONAL PARAMETERS" section.
            if (Options.HasFlag(UsageInfoOptions.IncludeOptionalParameterDescriptions) &&
                info.OptionalParameters.Any())
            {
                var entries = GetParameterEntries(info.OptionalParameters, info);
                sections.Add(new Section(Strings.UsageInfoOptionalParametersHeader + ":", entries)
                {
                    BodyIndentWidth = Section.DefaultIndent * 3 / 2,
                    HangingIndentWidth = Section.DefaultIndent / 2
                });
            }

            // If needed, provide help for enum values.
            if (Options.HasFlag(UsageInfoOptions.IncludeEnumValues))
            {
                foreach (var enumType in GetReferencedEnums(info).ToList())
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

        private IEnumerable<EnumArgumentType> GetReferencedEnums(ArgumentSetUsageInfo setInfo) =>
            GetAllArgumentTypes(setInfo)
                .OfType<EnumArgumentType>()
                .Select(t => t.Type)
                .Distinct()
                .Select(EnumArgumentType.Create);

        private IEnumerable<ColoredMultistring> GetEnumValueEntries(EnumArgumentType type)
        {
            var entries = type.GetValues()
                .Where(v => !v.Disallowed && !v.Hidden)
                .OrderBy(v => v.DisplayName)
                .Select(FormatEnumValueInfo);

            if (SkipLineBetweenEntries)
            {
                entries = entries.InsertBetween(new ColoredString(string.Empty));
            }

            return entries;
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

        private IEnumerable<IArgumentType> GetAllArgumentTypes(ArgumentSetUsageInfo setInfo)
        {
            var types = new HashSet<IArgumentType>();
            var typesToProcess = new Queue<IArgumentType>();

            foreach (var type in setInfo.AllParameters.Select(p => p.ArgumentType).Where(t => t != null))
            {
                typesToProcess.Enqueue(type);
            }

            while (typesToProcess.Count > 0)
            {
                var type = typesToProcess.Dequeue();
                if (!types.Add(type))
                {
                    continue;
                }

                foreach (var newType in type.DependentTypes.Where(t => !types.Contains(t)))
                {
                    typesToProcess.Enqueue(newType);
                }
            }

            return types;
        }

        private IEnumerable<ColoredMultistring> GetParameterEntries(IEnumerable<ArgumentUsageInfo> info, ArgumentSetUsageInfo setInfo)
        {
            var entries = info.Select(i => FormatParameterInfo(i, setInfo));
            if (SkipLineBetweenEntries)
            {
                entries = entries.InsertBetween((ColoredString)string.Empty);
            }

            return entries;
        }

        private ColoredMultistring FormatParameterInfo(ArgumentUsageInfo info, ArgumentSetUsageInfo setInfo)
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

            return builder.ToMultistring();
        }
    }
}
