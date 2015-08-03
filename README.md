# .NET Command Line Argument Parser (NClap)

[![Build status](https://ci.appveyor.com/api/projects/status/ay9tjpxor3n4gv1i/branch/master?svg=true)](https://ci.appveyor.com/project/reubeno/nclap/branch/master)
[![NuGet package](https://img.shields.io/nuget/dt/NClap.svg)](https://www.nuget.org/packages/NClap)
[![GitHub license](https://img.shields.io/github/license/reubeno/NClap.svg)](https://reubeno.github.io/NClap/LICENSE.txt)
[![Code coverage](https://img.shields.io/coveralls/reubeno/NClap.svg)](https://coveralls.io/github/reubeno/NClap)

Welcome! NClap is a .NET library for parsing command-line arguments and building interactive
command shells. It's driven by a declarative attribute syntax, and easy to extend.
It's sort of like a lightweight serializer for command-line arguments.

It's also very much under active development, and we welcome contributions! We're still writing
up some notes and tips about contributing to the project, how to run our unit tests, etc. But in the
meantime, don't be shy! Please go right ahead and file issues, drop us a line, submit pull requests,
etc.

## Getting NClap

The easiest way to consume NClap is by adding its [NuGet package](https://www.nuget.org/packages/NClap) to your project.

NClap is shared under the MIT license, as described in [LICENSE.txt](https://reubeno.github.io/NClap/LICENSE.txt).

## More information

* [Basic usage](#basic-usage)
  - [Parsing command lines](#parsing-command-lines)
  - [Building an interactive shell](#building-an-interactive-shell)
 
* [Mostly complete feature list](docs/Features.md)

* _NClap dev guide (to be written)_

## Basic usage

### Parsing command lines

1. Define a type (`class` or `struct`) to describe the parsed arguments, e.g.:

    <!-- MdCompile: assembly=ParseExample -->
    ```csharp
    using NClap.Metadata;
    
    class MyProgramArguments
    {
        [NamedArgument]
        public int ImportantValue { get; set; } // e.g.: ImportantValue=10
    }
    ```

    Each field or property in the type decorated with a `NamedArgumentAttribute` or
    `PositionalArgumentAttribute` will be mapped to a named argument or positional argument, respectively.
    
    Alternatively, you can add an attribute to the type itself to indicate that all writable, public
    fields and properties should be mapped to optional named arguments:
    
    <!-- MdCompile: import=NClap.Metadata -->
    ```csharp
    [ArgumentSet(PublicMembersAreNamedArguments = true)]
    class MyOtherProgramArguments
    {
        // These will become optional, named parameters
        public int MyAwesomeValue { get; set; }
        public int MyOtherAwesomeValue { get; set; }
    }
    ```
    
    You can customize the arguments through optional parameters to the attributes, e.g.:

    <!-- MdCompile: wrapinclass=true, import=NClap.Metadata -->
    ```csharp
    [NamedArgument(ArgumentFlags.Required | ArgumentFlags.Multiple,
                   LongName = "ImpVal",
                   ShortName = "iv",
                   HelpText = "This is the very important value")]
    public int ImportantValue { get; set; }
    ```

    In this above example, the C# property `ImportantValue` will be associated with
    a command-line option with two alternate names (`"ImpVal"` and `"iv"`). It must
    appear at least once on the command line, and may appear multiple times.

2. Next, parse them!  You'll need to construct or otherwise acquire an instance of the target type that
   your arguments will be parsed into, and then call one of the static parser methods, e.g.:

    <!-- MdCompile: assembly=ParseExample, import=NClap.Parser -->
    ```csharp
    using NClap;
    
    class MyProgram
    {
        private static void Main(string[] args)
        {
            var programArgs = new MyProgramArguments();
            if (!CommandLineParser.ParseWithUsage(args, programArgs))
            {
                return;
            }

            // TODO: Do something with the parsed args here...
        }
    }
    ```

    There are various overloads and variants of the parse methods (`CommandLineParser.Parse` and
    `CommandLineParser.ParseWithUsage`).  The particular variant used here will automatically
    display usage information to the console if an error occurred during argument parsing.

### Building an interactive shell

1. First, define the commands, or verbs, that you want exposed into the shell, e.g.:

    <!-- MdCompile: assembly=ShellExample, import=NClap.Repl, import=NClap.Metadata -->
    ```csharp
    enum MyCommandType
    {
        [Verb(typeof(ListCommand), HelpText = "Lists important things")]
        ListImportantThings,

        [Verb(Exits = true, HelpText = "Exits the shell")]
        Exit
    }
    ```

    Next, define the implementations of those verbs, making sure to indicate any arguments to them, e.g.:

    <!-- MdCompile: assembly=ShellExample, import=NClap.Metadata, import=NClap.Repl -->
    ```csharp
    class ListCommand : IVerb
    {
        [PositionalArgument(ArgumentFlags.Required, Position = 0, HelpText = "Type of things to list")]
        public string ThingsType { get; set; }

        public void Execute(object o)
        {
            // TODO: Do something here.
        }
    }
    ```

    Finally, create the interactive shell and enter it:

    <!-- MdCompile: assembly=ShellExample, wrapinclass=true, import=NClap.Repl, import=System -->
    ```csharp
    private static void RunInteractiveShell()
    {
        Console.WriteLine("Entering loop...");

        var options = new LoopOptions();
        Loop<MyCommandType>.Execute(options);

        Console.WriteLine("Exited loop...");
    }
    ```

    As with command-line argument parsing, there are many ways to further customize verbs or the
    shell itself.
