using System.Text;

namespace NClap.Expressions
{
    /// <summary>
    /// Utility class for expanding strings.
    /// </summary>
    internal static class StringExpander
    {
        private const char ExprStartChar = '{';
        private const char ExprEndChar = '}';

        /// <summary>
        /// Try to expand the given string value in the context of the
        /// provided environment.
        /// </summary>
        /// <param name="env">Environment for context.</param>
        /// <param name="value">Value to expand.</param>
        /// <param name="result">On success, receives the expanded result.</param>
        /// <returns>true on success; false otherwise.</returns>
        public static bool TryExpand(ExpressionEnvironment env, string value, out string result)
        {
            var builder = new StringBuilder();

            for (var i = 0; i < value.Length;)
            {
                var exprStartIndex = value.IndexOf(ExprStartChar, i);
                if (exprStartIndex < 0)
                {
                    builder.Append(value.Substring(i, value.Length - i));
                    break;
                }

                if (exprStartIndex > i)
                {
                    builder.Append(value.Substring(i, exprStartIndex - i));
                }

                if (exprStartIndex + 1 >= value.Length)
                {
                    result = null;
                    return false;
                }

                var exprEndIndex = value.IndexOf(ExprEndChar, exprStartIndex + 1);
                if (exprEndIndex < 0)
                {
                    result = null;
                    return false;
                }

                if (exprEndIndex - exprStartIndex <= 1)
                {
                    result = null;
                    return false;
                }

                var exprString = value.Substring(exprStartIndex + 1, exprEndIndex - exprStartIndex - 1);

                if (!ExpressionParser.TryParse(exprString, out Expression expr))
                {
                    result = null;
                    return false;
                }

                if (!expr.TryEvaluate(env, out string evalutedExpr))
                {
                    result = null;
                    return false;
                }

                builder.Append(evalutedExpr);

                i = exprEndIndex + 1;
            }

            result = builder.ToString();
            return true;
        }
    }
}
