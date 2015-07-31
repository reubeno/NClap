using System.Globalization;
using System.Text.RegularExpressions;

namespace NClap.Metadata
{
    /// <summary>
    /// Attribute that requires the associated string argument to match the
    /// specified regular expression.
    /// </summary>
    public sealed class MustMatchRegexAttribute : StringValidationAttribute
    {
        private RegexOptions _options;
        private Regex _regEx;

        /// <summary>
        /// Primary constructor.
        /// </summary>
        /// <param name="pattern">The regular expression pattern to match
        /// against.</param>
        public MustMatchRegexAttribute(string pattern)
        {
            Pattern = pattern;
            _options = RegexOptions.None;

            _regEx = new Regex(pattern, _options);
        }

        /// <summary>
        /// The regular expression pattern matched by this attribute.
        /// </summary>
        public string Pattern { get; }

        /// <summary>
        /// The options to use in constructing the regular expression.
        /// </summary>
        public RegexOptions Options
        {
            get
            {
                return _options;
            }

            set
            {
                _options = value;
                _regEx = new Regex(Pattern, _options);
            }
        }

        /// <summary>
        /// Validate the provided value in accordance with the attribute's
        /// policy.
        /// </summary>
        /// <param name="context">Context for validation.</param>
        /// <param name="value">The value to validate.</param>
        /// <param name="reason">On failure, receives a user-readable string
        /// message explaining why the value is not valid.</param>
        /// <returns>True if the value passes validation; false otherwise.
        /// </returns>
        public override bool TryValidate(ArgumentValidationContext context, object value, out string reason)
        {
            if (_regEx.IsMatch(GetString(value)))
            {
                reason = null;
                return true;
            }

            reason = string.Format(CultureInfo.CurrentCulture, Strings.StringDoesNotMatchRegEx, _regEx);
            return false;
        }
    }
}
