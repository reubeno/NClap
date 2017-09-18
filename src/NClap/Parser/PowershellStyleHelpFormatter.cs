using System;
using System.Collections.Generic;
using System.Linq;
using NClap.Utilities;

namespace NClap.Parser
{
    internal class PowershellStyleHelpFormatter : HelpFormatter
    {
        public override ColoredMultistring Format(ArgumentSetUsageInfo info) => Format(GenerateSections(info));

        private IReadOnlyList<Section> GenerateSections(ArgumentSetUsageInfo info)
        {
            var sections = new List<Section>();

            // If requested, add a "logo" for the program.
            if (Options.HasFlag(UsageInfoOptions.IncludeLogo) &&
                info.Logo != null && !info.Logo.IsEmpty())
            {
                sections.Add(new Section(null, info.Logo));
            }

            // Append the "NAME" section: lists the program name.
            if (Options.HasFlag(UsageInfoOptions.IncludeName))
            {
                sections.Add(new Section(Strings.UsageInfoNameHeader.ToUpper(), info.Name));
            }

            // Append the "SYNTAX" section: describes the basic syntax.
            if (Options.HasFlag(UsageInfoOptions.IncludeBasicSyntax))
            {
                sections.Add(new Section(Strings.UsageInfoSyntaxHeader.ToUpper(), info.Name + " " + info.GetBasicSyntax()));
            }

            // If present (and if requested), display the "DESCRIPTION" for the
            // program here.
            if (Options.HasFlag(UsageInfoOptions.IncludeDescription) && !string.IsNullOrEmpty(info.Description))
            {
                sections.Add(new Section(Strings.UsageInfoDescriptionHeader.ToUpper(), info.Description));
            }

            // If desired (and present), append "REQUIRED PARAMETERS" section.
            if (Options.HasFlag(UsageInfoOptions.IncludeRequiredParameterDescriptions) &&
                info.RequiredParameters.Any())
            {
                var entries = GetParameterEntries(info.RequiredParameters, info);
                sections.Add(new Section(Strings.UsageInfoRequiredParametersHeader.ToUpper(), entries));
            }

            // If desired (and present), append "OPTIONAL PARAMETERS" section.
            if (Options.HasFlag(UsageInfoOptions.IncludeOptionalParameterDescriptions) &&
                info.OptionalParameters.Any())
            {
                var entries = GetParameterEntries(info.OptionalParameters, info);
                sections.Add(new Section(Strings.UsageInfoOptionalParametersHeader.ToUpper(), entries));
            }

            // If present, append "EXAMPLES" section.
            if (Options.HasFlag(UsageInfoOptions.IncludeExamples) && info.Examples.Any())
            {
                sections.Add(new Section(Strings.UsageInfoExamplesHeader.ToUpper(), info.Examples));
            }

            // If requested, display remarks
            if (Options.HasFlag(UsageInfoOptions.IncludeRemarks) && !string.IsNullOrEmpty(info.Remarks))
            {
                sections.Add(new Section(Strings.UsageInfoRemarksHeader.ToUpper(), info.Remarks));
            }

            return sections;
        }

        private IEnumerable<ColoredString> GetParameterEntries(IEnumerable<ArgumentUsageInfo> info, ArgumentSetUsageInfo setInfo) =>
            info.Select(i => FormatParameterInfo(i, setInfo))
                .InsertBetween(new ColoredString[] { string.Empty })
                .Flatten();

        private IReadOnlyList<ColoredString> FormatParameterInfo(ArgumentUsageInfo info, ArgumentSetUsageInfo setInfo)
        {
            // Select the colors we'll use.
            var paramMetadataFgColor = UseColor ? new ConsoleColor?(ParameterMetadataForegroundColor) : null;
            ConsoleColor? paramSyntaxFgColor = null;

            var entries = new List<ColoredString>
            {
                // Append parameter syntax info.
                new ColoredString(SimplifyParameterSyntax(info.DetailedSyntax), paramSyntaxFgColor)
            };

            // If both are present (and requested to be displayed), we combine the short name and
            // default value onto the same line.
            var parameterMetadata = new List<string>();

            // Append parameter's short name (if it has one).
            if (Options.HasFlag(UsageInfoOptions.IncludeParameterShortNameAliases) &&
                !string.IsNullOrEmpty(info.ShortName) &&
                !string.IsNullOrEmpty(setInfo.DefaultShortNamePrefix))
            {
                parameterMetadata.Add($"{Strings.UsageInfoShortForm} {setInfo.DefaultShortNamePrefix + info.ShortName}");
            }

            // Append the parameter's default value (if it has one, and if requested).
            if (Options.HasFlag(UsageInfoOptions.IncludeParameterDefaultValues) &&
                !string.IsNullOrEmpty(info.DefaultValue))
            {
                parameterMetadata.Add($"{Strings.UsageInfoDefaultValue} {info.DefaultValue}");
            }

            // Now append the short name and/or default value, if either
            // were present and accounted for.
            if (parameterMetadata.Count > 0)
            {
                entries.Add(new ColoredString(
                    string.Join(", ", parameterMetadata),
                    paramMetadataFgColor));
            }

            // Append the parameter's description (if it has one).
            if (!string.IsNullOrEmpty(info.Description))
            {
                entries.Add(info.Description);
            }

            return entries;
        }
    }
}
