using System;
using System.Diagnostics;
using System.IO;
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
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="assembly"/>
        /// is null.</exception>
        public static string GetLogo(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            var builder = new StringBuilder();

            var title = GetAssemblyTitle(assembly, assembly.GetName().Name);
            var fileVersion = FileVersionInfo.GetVersionInfo(assembly.Location);

            builder.AppendFormat("{0} version {1}", title, fileVersion.FileVersion);
            builder.AppendLine();

            var copyright = assembly.GetSingleAttribute<AssemblyCopyrightAttribute>()?.Copyright;
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
        internal static string GetAssemblyTitle(this ICustomAttributeProvider assembly, string assemblyName)
        {
            Debug.Assert(assembly != null);

            var company = assembly.GetSingleAttribute<AssemblyCompanyAttribute>()?.Company.Trim();
            var title = assembly.GetSingleAttribute<AssemblyTitleAttribute>()?.Title.Trim();
            var product = assembly.GetSingleAttribute<AssemblyProductAttribute>()?.Product.Trim();

            if (!string.IsNullOrEmpty(title))
            {
                if (!string.IsNullOrEmpty(company) && !title.Contains(company))
                {
                    title = $"{company} {title}";
                }

                return title;
            }

            if (!string.IsNullOrEmpty(product))
            {
                if (!string.IsNullOrEmpty(company) && !product.Contains(company))
                {
                    product = $"{company} {product}";
                }

                return product;
            }

            if (!string.IsNullOrEmpty(company) && !assemblyName.Contains(company))
            {
                assemblyName = $"{company} {assemblyName}";
            }

            return assemblyName;
        }
    }
}
