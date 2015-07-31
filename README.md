# .NET Command Line Argument Parser (NClap)

[![Build status](https://ci.appveyor.com/api/projects/status/ay9tjpxor3n4gv1i/branch/master?svg=true)](https://ci.appveyor.com/project/reubeno/nclap/branch/master)

NClap is a .NET library for parsing command-line arguments and building interactive
command shells. It's driven by a declarative attribute syntax, and easy to extend.

## License

NClap is shared under the MIT license, as described in [LICENSE.txt](https://reubeno.github.io/NClap/LICENSE.txt).


## Getting NClap

The easiest way to consume NClap is by adding its [NuGet package](https://www.nuget.org/packages/NClap) to your project.


## Laundry list of features

### Command-line parsing

NClap supports parsing most commonly used .NET types out of the box:

* Nearly all primitive .NET types (e.g. `bool`, `char`, `string`, `byte`, `short`, `int`, `long`, `float`, `double`, `decimal`, all of the unsigned variants, etc.)
* Many other common .NET types (e.g. `Guid`, `DateTime`, `TimeSpan`, `IPAddress`, `Uri`, `Regex`)
* Any .NET `enum` type (including extended support for `enum` types marked with `[FlagsAttribute]`)
* The nullable version of any supported value type listed above (e.g. `int?`)
* A custom `FileSystemPath` type exported by NClap to simplify file-path parsing, validation, and completion
* Any generic `ICollection<T>` over any supported type listed above (e.g. `List<T>`, `LinkedList<T>`, `HashSet<T>`, `SortedSet<T>`, `KeyValuePair<T>`, `Tuple<>`, `Dictionary<TKey, TValue>`)

And for any type not directly supported, it's easy to extend NClap to parse a
custom type: you just need to implement a simple parsing interface
(`IStringParser`) and a simple string formatting interface (`IObjectFormatter`).
For bonus points, you can extend a string completion interface
(`IStringCompleter`) and get custom type-specific tab completion when building
an interactive shell that uses the type.

For all of these types, NClap supports:

* Named arguments
* Positional arguments
* Optional arguments
* Required arguments
* Multiply-specified arguments
* Arguments with default values
* Auto-generating usage info help for argument sets
* Attribute-driven argument validation (e.g. `MustBeGreaterThanAttribute`, `MustMatchRegExAttribute`, `MustNotBeEmptyAttribute`)
* Custom argument validation (i.e. properties with custom `get` and `set` accessors)
* Arguments parsed into fields or properties
* Arguments parsed into public, internal, or private fields or properties

### Interactive shells

NClap makes it easy to build an interactive shell and project actions into it as verbs, with support for:

* Verbs with complex arguments (i.e. the full argument parsing support described above)
* Type- and context-sensitive tab completion (supporting many types listed above)
* Easy extension model for tab completion (i.e. just implement `IStringCompleter`)
* Easily colorized output
* Custom input/output
* A custom partial implementation of `readline`
* Auto-generated help verb
* Customizable keyboard bindings for input operations

## Basic usage: parsing command lines

1. First, define a type (`class` or `struct`) to describe the parsed arguments, e.g.:

    ```csharp
    class MyProgramArguments
    {
        [NamedArgument(ArgumentFlags.AtMostOnce)]
        public int ImportantValue { get; set; }
    }
    ```

    The basic idea is that each field or property in the type decorated with a `NamedArgumentAttribute` or
    `PositionalArgumentAttribute` will be mapped to a named argument or positional argument, respectively.
    You can customize this mapping by providing additional arguments to the attributes, e.g.:

    ```csharp
    [NamedArgument(ArgumentFlags.Required | ArgumentFlags.Multiple,
                   LongName = "ImpVal",
                   ShortName = "iv",
                   DefaultValue = 17,
                   HelpText = "This is the very important value")]
    public int ImportantValue { get; set; }
    ```

2. Next, parse them!  You'll need to construct or otherwise acquire an instance of the target type that
   your arguments will be parsed into, and then call one of the static parser methods, e.g.:

    ```csharp
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
            ...
        }
    }
    ```

    There are various overloads and variants of the parse methods (`CommandLineParser.Parse` and
    `CommandLineParser.ParseWithUsage`).  The particular variant used here will automatically
    display usage information to the console if an error occurred during argument parsing.

## Basic usage: building an interactive shell

1. First, define the commands, or verbs, that you want exposed into the shell, e.g.:

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

    ```csharp
    class ListCommand : IVerb
    {
        [PositionalArgument(ArgumentFlags.Required, Position = 0, HelpText = "Type of things to list")]
        public string ThingsType;

        public void Execute(object o)
        {
            // TODO: Do something here.
            ...
        }
    }
    ```

    Finally, create the interactive shell and enter it:

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
