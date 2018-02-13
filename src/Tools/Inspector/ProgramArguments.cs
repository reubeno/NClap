using System;
using System.IO;
using System.Reflection;
using NClap.Metadata;
using NClap.Types;

namespace NClap.Inspector
{
    [ArgumentSet(Style = ArgumentSetStyle.PowerShell)]
    class ProgramArguments : IArgumentSetWithHelp
    {
        public enum CommandType
        {
            [Command(typeof(CompleteCommand))]
            Complete,

            [Command(typeof(DocsCommand))]
            Docs
        }

        [NamedArgument(ArgumentFlags.Optional, LongName = "Verbose")]
        public bool Verbose { get; set; }

        [NamedArgument(ArgumentFlags.Optional, LongName = "Help")]
        public bool Help { get; set; }

        [NamedArgument(ArgumentFlags.Required, LongName = "Assembly")]
        public FileSystemPath AssemblyPath { get; set; }

        [NamedArgument(ArgumentFlags.Required, LongName = "Type")]
        public string TypeName { get; set; }

        [PositionalArgument(ArgumentFlags.Required, Position = 0)]
        public CommandGroup<CommandType> Command { get; set; }

        public Assembly LoadedAssembly { get; private set; }

        public Type LoadedType { get; private set; }

        public CommandResult Execute()
        {
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += (sender, args) =>
            {
                if (Verbose) Console.WriteLine($"Trying to load: {args.Name}");

                var name = new AssemblyName(args.Name);
                var possiblePath = Path.Combine(
                    Path.GetDirectoryName(AssemblyPath),
                    name.Name + ".dll");

                Assembly assembly;
                if (File.Exists(possiblePath))
                {
                    if (Verbose) Console.WriteLine($"Loading by path: {possiblePath}");
                    assembly = Assembly.ReflectionOnlyLoadFrom(possiblePath);
                }
                else
                {
                    assembly = Assembly.ReflectionOnlyLoad(args.Name);
                }

                if (assembly == null)
                {
                    Console.Error.WriteLine($"Error: Failed to load: {args.Name}");
                }

                return assembly;
            };

            LoadedAssembly = Assembly.ReflectionOnlyLoadFrom(AssemblyPath);
            LoadedType = LoadedAssembly.GetType(TypeName);
            if (LoadedType == null)
            {
                Console.Error.WriteLine($"Error: unable to load type: {TypeName}");
                return CommandResult.RuntimeFailure;
            }

            return Command.Execute();
        }
    }
}
