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
        Default = IncludeParameterDescriptions |
                  IncludeParameterShortNameAliases,

        /// <summary>
        /// Include a product logo in the usage information.
        /// </summary>
        IncludeLogo = 0x1,

        /// <summary>
        /// Include parameter descriptions.
        /// </summary>
        IncludeParameterDescriptions = 0x2,

        /// <summary>
        /// Include parameter default values.
        /// </summary>
        IncludeParameterDefaultValues = 0x4,

        /// <summary>
        /// Include information about parameters with short name aliases.
        /// </summary>
        IncludeParameterShortNameAliases = 0x8,

        /// <summary>
        /// Include example usage.
        /// </summary>
        IncludeExamples = 0x10,

        /// <summary>
        /// Use multiple colors in the output.
        /// </summary>
        UseColor = 0x20
    }
}