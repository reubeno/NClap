using System;
using System.Diagnostics;
using System.Reflection;
using NClap.Expressions;
using NClap.Utilities;

namespace NClap.Help
{
    /// <summary>
    /// Utility class for generating logos.
    /// </summary>
    internal class LogoFactory : ExpressionEnvironment
    {
        private readonly Assembly _assembly;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="assembly">Assembly for which logos will be generated.</param>
        public LogoFactory(Assembly assembly)
        {
            _assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        }

        /// <summary>
        /// Tries to expand the given logo format string.
        /// </summary>
        /// <param name="logoFormat">Format string to expand.</param>
        /// <param name="result">On success, receives result.</param>
        /// <returns>true on success; false otherwise.</returns>
        public bool TryExpand(string logoFormat, out string result) =>
            StringExpander.TryExpand(this, logoFormat, out result);

        /// <summary>
        /// Tries to retrieve the value associated with the given
        /// variable.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="value">On success, receives the value associated
        /// with the variable.</param>
        /// <returns>true if the variable was found; false otherwise.</returns>
        public override bool TryGetVariable(string variableName, out string value)
        {
            switch (variableName.ToUpperInvariant())
            {
                case "TITLE": // guaranteed to be present
                    if (!TryGetMetadata<AssemblyTitleAttribute>(a => a.Title, out value))
                    {
                        // Fallback to assembly name.
                        value = _assembly.GetName().Name;
                    }

                    return true;

                case "VERSION": // guaranteed to be present
                    if (!TryGetMetadata<AssemblyVersionAttribute>(a => a.Version, out value))
                    {
                        value = FileVersionInfo.GetVersionInfo(_assembly.Location).FileVersion;
                    }

                    return true;

                case "COMPANY":
                    return TryGetMetadata<AssemblyCompanyAttribute>(a => a.Company, out value);
                case "CONFIGURATION":
                    return TryGetMetadata<AssemblyConfigurationAttribute>(a => a.Configuration, out value);
                case "COPYRIGHT":
                    return TryGetMetadata<AssemblyCopyrightAttribute>(
                        a => a.Copyright.Replace("©", "(C)"),
                        out value);
                case "CULTURE":
                    return TryGetMetadata<AssemblyCultureAttribute>(a => a.Culture, out value);
                case "DESCRIPTION":
                    return TryGetMetadata<AssemblyDescriptionAttribute>(a => a.Description, out value);
                case "FILEVERSION":
                    return TryGetMetadata<AssemblyFileVersionAttribute>(a => a.Version, out value);
                case "INFORMATIONALVERSION":
                    return TryGetMetadata<AssemblyInformationalVersionAttribute>(a => a.InformationalVersion, out value);
                case "PRODUCT":
                    return TryGetMetadata<AssemblyProductAttribute>(a => a.Product, out value);
                case "TRADEMARK":
                    return TryGetMetadata<AssemblyTrademarkAttribute>(a => a.Trademark, out value);

                default:
                    value = null;
                    return false;
            }
        }

        private bool TryGetMetadata<T>(Func<T, string> func, out string value)
            where T : Attribute
        {
            var attrib = _assembly.GetSingleAttribute<T>();
            if (attrib == null)
            {
                value = null;
                return false;
            }

            value = func(attrib);
            return value != null;
        }
    }
}
