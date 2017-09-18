using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NClap.Utilities
{
    /// <summary>
    /// Utilities that manipulating Assembly objects.
    /// </summary>
    internal static class AssemblyUtilities
    {
        private static Assembly DefaultAssembly => Assembly.GetEntryAssembly() ?? typeof(AssemblyUtilities).GetTypeInfo().Assembly;

        /// <summary>
        /// Generates a logo string for the application's entry assembly, or
        /// the assembly containing this method if no entry assembly could
        /// be found.
        /// </summary>
        /// <returns>The logo string.</returns>
        public static string GetLogo() => GetLogo(DefaultAssembly);

        /// <summary>
        /// Generates a logo string for the provided Assembly, using its
        /// version and other metadata.
        /// </summary>
        /// <param name="assembly">Assembly to generate logo for.</param>
        /// <returns>The logo string.</returns>
        public static string GetLogo(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            var builder = new StringBuilder();

            var title = GetAssemblyTitle(assembly, assembly.GetName().Name);
            var fileVersion = FileVersionInfo.GetVersionInfo(assembly.Location);

            builder.AppendFormat("{0} version {1}", title, fileVersion.FileVersion);
            builder.AppendLine();

            var copyright = GetAttribute<AssemblyCopyrightAttribute>(assembly)?.Copyright;
            if (!string.IsNullOrEmpty(copyright))
            {
                builder.AppendLine(copyright.Replace("©", "(C)"));
            }

            return builder.ToString();
        }

        /// <summary>
        /// To get the assembly file name including extension (e.g.: foo.exe).
        /// </summary>
        /// <returns>Assembly File name with extension.</returns>
        public static string GetAssemblyFileName()
        {
            Debug.Assert(DefaultAssembly != null);
            return Path.GetFileName(DefaultAssembly.Location);
        }

        /// <summary>
        /// Formats a title string for the provided assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="assemblyName">The assembly's name.</param>
        /// <returns>The formatted title string.</returns>
        internal static string GetAssemblyTitle(ICustomAttributeProvider assembly, string assemblyName)
        {
            Debug.Assert(assembly != null);

            var prefix = string.Empty;
            var company = GetAttribute<AssemblyCompanyAttribute>(assembly)?.Company;
            if (!string.IsNullOrEmpty(company))
            {
                prefix = company + " ";
            }

            var title = GetAttribute<AssemblyTitleAttribute>(assembly)?.Title;
            if (!string.IsNullOrEmpty(title))
            {
                return title.StartsWith(prefix, StringComparison.CurrentCultureIgnoreCase) ? title : prefix + title;
            }

            var product = GetAttribute<AssemblyProductAttribute>(assembly)?.Product;
            if (!string.IsNullOrEmpty(product))
            {
                return product.StartsWith(prefix, StringComparison.CurrentCultureIgnoreCase) ? product : prefix + product;
            }

            return prefix + assemblyName;
        }

        private static TAttribute GetAttribute<TAttribute>(ICustomAttributeProvider attributeProvider) where TAttribute : Attribute
        {
            Debug.Assert(attributeProvider != null);
            return attributeProvider.GetCustomAttributes(typeof(TAttribute), true)
                                    .Cast<TAttribute>()
                                    .FirstOrDefault();
        }
    }
}
