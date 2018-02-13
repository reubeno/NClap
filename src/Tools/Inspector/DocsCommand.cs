using System;
using System.Linq;
using System.Reflection;
using NClap.Metadata;
using NClap.Parser;
using NClap.Types;
using Newtonsoft.Json;

namespace NClap.Inspector
{
    class DocsCommand : SynchronousCommand
    {
        private class ArgumentSetSummary
        {
            public string description;
            public string assembly;
            public string type;
            public ArgumentSummary[] args;
            public string[] examples;
        }

        private class ArgumentSummary
        {
            public string name;
            public string short_name;
            public bool required;
            public bool command;
            public bool takes_rest_of_line;
            public string type;
            public string description;
            public object default_value;
            public ArgumentValueSummary[] possible_values;
        }

        private class ArgumentValueSummary
        {
            public string description;
            public string name;
            public string short_name;
            public string command_type;
            public ArgumentSetSummary command_argument_set;
        }

        private readonly ProgramArguments _programArgs;

        public DocsCommand(ProgramArguments programArgs)
        {
            _programArgs = programArgs;
        }

        public override CommandResult Execute()
        {
            var def = GetArgumentSetFor(_programArgs.LoadedType);

            var serialized = SerializeSummary(def);

            Console.WriteLine(serialized);

            return CommandResult.Success;
        }

        private string SerializeSummary(ArgumentSetDefinition argSet)
        {
            var argSetSummary = Summarize(argSet);

            var serializer = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };

            return JsonConvert.SerializeObject(argSetSummary, Formatting.Indented, serializer);
        }

        private ArgumentSetDefinition GetArgumentSetFor(Type type) =>
            AttributeBasedArgumentDefinitionFactory.CreateArgumentSet(type);

        private ArgumentSetSummary Summarize(ArgumentSetDefinition argSet)
        {
            return new ArgumentSetSummary
            {
                description = argSet.Attribute.Description,
                assembly = _programArgs.LoadedAssembly.FullName,
                type = _programArgs.LoadedType.Name,

                args = argSet.AllArguments
                    .Where(arg => !arg.Attribute.Hidden)
                    .Select(arg => Summarize(arg))
                    .ToArray(),

                examples = argSet.Attribute.Examples,
            };
        }

        private ArgumentSummary Summarize(ArgumentDefinition arg)
        {
            var attrib = arg.Attribute;
            var namedAttrib = arg.Attribute as NamedArgumentAttribute;
            var enumType = arg.ArgumentType as IEnumArgumentType;

            var isCommand = false;
            var type = arg.ArgumentType.Type;
            if (type.GetTypeInfo().IsGenericType &&
                type.GetGenericTypeDefinition().Name == "CommandGroup`1")
            {
                enumType = ArgumentType.GetType(type.GetGenericArguments().First()) as IEnumArgumentType;
                isCommand = true;
            }

            return new ArgumentSummary
            {
                name = attrib.LongName,
                short_name = namedAttrib?.ShortName,
                required = arg.IsRequired,
                command = isCommand,
                takes_rest_of_line = arg.TakesRestOfLine,
                type = arg.ArgumentType.DisplayName,
                description = attrib.Description,
                default_value = arg.HasDefaultValue ? attrib.DefaultValue : null,
                possible_values = enumType?.GetValues()
                    .Where(value => !value.Hidden && !value.Disallowed)
                    .Select(value => Summarize(arg, value))
                    .ToArray(),
            };
        }

        private ArgumentValueSummary Summarize(ArgumentDefinition arg, IArgumentValue value)
        {
            var commandAttrib = value.GetAttributes<CommandAttribute>().SingleOrDefault();
            var commandType = commandAttrib?.GetImplementingType(arg.ArgumentType.Type);
            return new ArgumentValueSummary
            {
                description = value.Description,
                name = value.LongName,
                short_name = value.ShortName,
                command_type = commandType.Name,
                command_argument_set = Summarize(GetArgumentSetFor(commandType))
            };
        }
    }
}
