using System;

namespace NClap.Parser
{
    /// <summary>
    /// Options for generating usage info.
    /// </summary>
    [Flags]
    public enum UsageInfoOptions
    {
        /// <summary>
        /// Do not include any optional information.
        /// </summary>
        None = 0,

        /// <summary>
        /// Include default sets of optional information.
        /// </summary>
        Default =
            CondenseOutput |
            IncludeName |
            IncludeBasicSyntax |
            IncludeDescription |
            IncludeParameterDescriptions |
            IncludeParameterShortNameAliases |
            IncludeParameterDefaultValues |
            IncludeEnumValues |
            UseColor,

        /// <summary>
        /// Abridged default set of optional information.
        /// </summary>
        DefaultAbridged =
            CondenseOutput |
            IncludeName |
            IncludeBasicSyntax |
            IncludeRequiredParameterDescriptions |
            UseColor |
            IncludeRemarks,

        /// <summary>
        /// Condense the output as much as possible.
        /// </summary>
        CondenseOutput = 0x800,

        /// <summary>
        /// Include a product logo in the usage information.
        /// </summary>
        IncludeLogo = 0x1,

        /// <summary>
        /// Include the program or command name.
        /// </summary>
        IncludeName = 0x400,

        /// <summary>
        /// Include description.
        /// </summary>
        IncludeDescription = 0x2,

        /// <summary>
        /// Include basic syntax.
        /// </summary>
        IncludeBasicSyntax = 0x200,

        /// <summary>
        /// Include required parameter descriptions.
        /// </summary>
        IncludeRequiredParameterDescriptions = 0x4,

        /// <summary>
        /// Include optional parameter descriptions.
        /// </summary>
        IncludeOptionalParameterDescriptions = 0x8,

        /// <summary>
        /// Include all (required and optional) parameter descriptions.
        /// </summary>
        IncludeParameterDescriptions = IncludeRequiredParameterDescriptions | IncludeOptionalParameterDescriptions,

        /// <summary>
        /// Include parameter default values.
        /// </summary>
        IncludeParameterDefaultValues = 0x10,

        /// <summary>
        /// Include information about parameters with short name aliases.
        /// </summary>
        IncludeParameterShortNameAliases = 0x20,

        /// <summary>
        /// Include information about possible values for enum types.
        /// </summary>
        IncludeEnumValues = 0x1000,

        /// <summary>
        /// Include example usage.
        /// </summary>
        IncludeExamples = 0x40,

        /// <summary>
        /// Include remarks (such as those advertising detailed help), if
        /// available.
        /// </summary>
        IncludeRemarks = 0x80,

        /// <summary>
        /// Use multiple colors in the output.
        /// </summary>
        UseColor = 0x100
    }
}