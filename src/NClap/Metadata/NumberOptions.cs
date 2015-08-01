using System;

namespace NClap.Metadata
{
    /// <summary>
    /// Options describing how to parse numeric values.
    /// </summary>
    [Flags]
    public enum NumberOptions
    {
        /// <summary>
        /// Default behavior.
        /// </summary>
        None,

        /// <summary>
        /// Allow use of metric unit suffixes (e.g. k to denote a multiplier of
        /// 1 thousand, M to denote a multiplier of 1 million).  This option
        /// conflicts with AllowBinaryMetricUnitSuffix.  If both flags are
        /// present, then AllowBinaryMetricUnitSuffix takes precedence.
        /// </summary>
        AllowMetricUnitSuffix,

        /// <summary>
        /// Allow use of binary metric unit suffixes (e.g. k to denote 1024,
        /// M to denote 1024 * 1024).  This option conflicts with
        /// AllowMetricUnitSuffix.    If both flags are present, then
        /// AllowBinaryMetricUnitSuffix takes precedence.
        /// </summary>
        AllowBinaryMetricUnitSuffix
    }
}
