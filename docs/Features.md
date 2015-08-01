## NClap Features

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

