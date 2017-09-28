using System;
using System.Collections.Generic;
using System.Linq;

namespace NClap.Utilities
{
    /// <summary>
    /// Simplified, colored string version of StringBuilder.
    /// </summary>
    public class ColoredMultistringBuilder : IStringBuilder
    {
        private readonly List<ColoredString> _pieces = new List<ColoredString>();

        /// <summary>
        /// Append a colored string.
        /// </summary>
        /// <param name="value">The colored string to append.</param>
        public void Append(ColoredString value) => _pieces.Add(value);

        /// <summary>
        /// Append the provided colored strings.
        /// </summary>
        /// <param name="values">The colored strings to append.</param>
        public void Append(IEnumerable<ColoredString> values) => _pieces.AddRange(values);

        /// <summary>
        /// Append the contents of the provided multistring.
        /// </summary>
        /// <param name="value">The multistring to append.</param>
        public void Append(ColoredMultistring value) => Append(value.Content);

        /// <summary>
        /// Append the contents of the provided multistrings.
        /// </summary>
        /// <param name="values">The multistrings to append.</param>
        public void Append(IEnumerable<ColoredMultistring> values) => Append(values.SelectMany(v => v.Content));

        /// <summary>
        /// Append a newline.
        /// </summary>
        public void AppendLine() => _pieces.Add(Environment.NewLine);

        /// <summary>
        /// Append a colored string followed by a newline.
        /// </summary>
        /// <param name="value">The colored string to append.</param>
        public void AppendLine(ColoredString value)
        {
            _pieces.Add(value);
            _pieces.Add(new ColoredString(Environment.NewLine, value.ForegroundColor, value.BackgroundColor));
        }

        /// <summary>
        /// Append the provided colored strings followed by a newline.
        /// </summary>
        /// <param name="values">The colored multistrings to append.</param>
        public void AppendLine(IEnumerable<ColoredString> values)
        {
            Append(values);
            AppendLine();
        }

        /// <summary>
        /// Append the contents of the provided multistring followed by a
        /// newline.
        /// </summary>
        /// <param name="value">The multistring to append.</param>
        public void AppendLine(ColoredMultistring value) => AppendLine(value.Content);

        /// <summary>
        /// Append the contents of the provided multistrings followed by a
        /// newline.
        /// </summary>
        /// <param name="values">The multistrings to append.</param>
        public void AppendLine(IEnumerable<ColoredMultistring> values)
        {
            Append(values);
            AppendLine();
        }

        /// <summary>
        /// Converts the current contents of the builder to a bare string.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => string.Concat(_pieces);

        /// <summary>
        /// Converts the current contents of the builder to a colored
        /// multistring.
        /// </summary>
        /// <returns>The multistring.</returns>
        public ColoredMultistring ToMultistring() => new ColoredMultistring(_pieces);

        /// <summary>
        /// Appends a new string to the end of this builder's current content.
        /// </summary>
        /// <param name="s">The string to append.</param>
        public void Append(IString s) => Append((ColoredMultistring)s);

        /// <summary>
        /// Appends a new string to the end of this builder's current content.
        /// </summary>
        /// <param name="s">The string to append.</param>
        public void Append(string s) => Append((ColoredString)s);

        /// <summary>
        /// Appends the specified character to the end of this builder's current
        /// content, repeated the indicated number of times.
        /// </summary>
        /// <param name="c">The char to append.</param>
        /// <param name="count">The number of times to append it.</param>
        public void Append(char c, int count) => Append(new string(c, count));

        /// <summary>
        /// Generates a composed string from the contents of this builder.
        /// </summary>
        /// <returns>The composed string.</returns>
        public IString Generate() => ToMultistring();
    }
}
