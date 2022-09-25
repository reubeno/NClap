# Commands

Many command-line arguments are _data_; they enable or disable options, provide parameters, or something similar. They're the nouns. You often also need to let the user express which _actions_ they want your application to take. We call these _commands_; they're the verbs.

When specifying a command, a user often needs to specify command-specific parameters that might not apply to other commands, or might only apply to closely related commands. There are also applications that find it helpful to group commands into a hierarchy, for easier discovery. NClap provides native support for each of these patterns, and does so in a way that allows the definition of individual commands to be as self-contained as you'd like, potentially supporting the sharing of reusable commands in multiple scenarios or applications.

Okay, with that explained--now for some code!

## Defining a command

We'll start from the bottom. Each command is an invokable action; it's represented by a class that implements the `ICommand` interface. The interface is simple, containing a standard method that will be invoked when the command is executed. For flexibility, the execution is expected to be asynchronous, but you can have your command class inherit from a standard abstract class to simplify your implementation: `Command` for asynchronous commands and `SynchronousCommand` for commands that only support synchronous execution, e.g.:

```csharp
using NClap.Metadata;

class MyCoolCommand : SynchronousCommand
{
    [NamedArgument(ArgumentFlags.Required)]
    public int MyValue { get; set; }

    public override CommandResult Execute()
    {
        Console.WriteLine("Hello from my command: value = {MyValue}!");
        return CommandResult.Success;
    }
}
```

Note above that you can use standard NClap attributes to add parameters to the command. In this example, we have a required integer parameter that we can depend on in the `Execute` method.

## Adding the command to your command line argument set

The next step is to link this command definition into your application's command-line argument set. There's two steps here:

1. Define an `enum` type that lists the possible commands that are mutually exclusive.

   For example, here we define an `enum` that allows the user to either execute `MyCoolCommand` or `MyLessCoolCommand`:

   ```csharp
   enum MyCommandType
   {
       [Command(typeof(MyCoolCommand), LongName = "cool", Description = "Do the cool thing!")]
       Cool,

       [Command(typeof(MyLessCoolCommand), LongName = "notcool", Description = "Execute my significantly less cool command")]
       LessCool
   }
   ```

   Note the ability to use standard NClap argument options (e.g. `LongName` and `Description`) to customize how the different command choices may be indicated on the command line. Here, the user should be able to type `cool` or `notcool` to select one of the commands.

2. Add this group of commands to the main command-line argument set for the
   application:

   ```csharp
   class ProgramArguments
   {
       [NamedArgument(ArgumentFlags.Optional)]
       public bool Verbose { get; set; }

       [PositionalArgument(ArgumentFlags.Required, Position = 0)]
       public CommandGroup<MyCommandType> PrimaryCommand { get; set; }

       // ...
   }
   ```

   Note that we use standard NClap attributes--`PositionalArgumentAttribute` in this case--to indicate where the command and all of its command-specific arguments will be spliced into the overall `ProramArguments`.

## Invoking the command

Once you've parsed your application's command-line arguments, you can easily execute whichever command was selected, e.g.:

```csharp
public static void Main(string[] args)
{
    ProgramArguments programArgs;
    if (!CommandLineParser.TryParse(args, out programArgs))
    {
        return;
    }

    CommandResult result = programArgs.PrimaryCommand.Execute();

    // ...
}
```

## Exposing commands in an interactive loop

Once you've defined your commands, you can also leverage them to create an interactive loop, with just a few extra lines of code:

```csharp
public static void RunLoop()
{
    var loop = new Loop(typeof(MyCommandType));
    loop.Execute();
}
```
