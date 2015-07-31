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
        Default = IncludeDescription |
                  IncludeParameterDescriptions |
                  IncludeParameterShortNameAliases |
                  IncludeParameterDefaultValues |
                  UseColor,

        /// <summary>
        /// Abridged default set of optional information.
        /// </summary>
        DefaultAbridged = IncludeRequiredParameterDescriptions | UseColor | IncludeRemarks,

        /// <summary>
        /// Include a product logo in the usage information.
        /// </summary>
        IncludeLogo = 0x1,

        /// <summary>
        /// Include description.
        /// </summary>
        IncludeDescription = 0x2,

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