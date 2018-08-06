using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using NClap.Metadata;
using NClap.Types;
using NClap.Utilities;

namespace NClap.Parser
{
    /// <summary>
    /// Encapsulates the state of an in-progress parse operation.
    /// </summary>
    internal class ArgumentParser
    {
        // Constants
        private static readonly ConsoleColor? ErrorForegroundColor = ConsoleColor.Yellow;

        /// <summary>
        /// Constructs a new stateful parser for the given argument.
        /// </summary>
        /// <param name="argSet">The argument set containing the argument to be parsed.</param>
        /// <param name="arg">The definition of the argument.</param>
        /// <param name="options">General options for parsing this argument set.</param>
        /// <param name="destination">The destination object into which the parsed result should go,
        /// if so desired; null otherwise to parse without saving results.</param>
        public ArgumentParser(ArgumentSetDefinition argSet, ArgumentDefinition arg, CommandLineParserOptions options,
            object destination = null)
        {
            ArgumentSet = argSet ?? throw new ArgumentNullException(nameof(argSet));
            Argument = arg ?? throw new ArgumentNullException(nameof(arg));
            Reporter = options?.Reporter ?? (s => { });
            DestinationObject = arg.FixedDestination ?? destination;
            ParseContext = CreateParseContext(Argument, ArgumentSet.Attribute, options, DestinationObject);

            if (Argument.IsCollection)
            {
                CollectionValues = GenericCollectionFactory.CreateList(Argument.CollectionArgumentType.ElementType.Type);
            }
        }

        /// <summary>
        /// Argument set containing the definition of the argument to be parsed.
        /// </summary>
        public ArgumentSetDefinition ArgumentSet { get; }

        /// <summary>
        /// Argument to be parsed.
        /// </summary>
        public ArgumentDefinition Argument { get; }

        /// <summary>
        /// Context for parsing.
        /// </summary>
        public ArgumentParseContext ParseContext { get; }

        /// <summary>
        /// Object to use to report errors.
        /// </summary>
        public ErrorReporter Reporter { get; }

        /// <summary>
        /// For collections, stores the in-progress list of values seen.
        /// </summary>
        public IList CollectionValues { get; }

        /// <summary>
        /// Destination object into which parsed and finalized result should be
        /// stored. May not be present.
        /// </summary>
        public object DestinationObject { get; }

        /// <summary>
        /// State variable indicating whether this argument has been seen in
        /// the currently-being-parsed command line.
        /// </summary>
        public bool SeenValue { get; set; }

        /// <summary>
        /// Finalizes parsing of the argument, reporting any errors from policy
        /// violations (e.g. missing required arguments).
        /// </summary>
        /// <param name="fileSystemReader">File system reader to use.</param>
        /// <returns>True indicates that finalization completed successfully;
        /// false indicates that a failure occurred.</returns>
        public bool TryFinalize(IFileSystemReader fileSystemReader)
        {
            // If we haven't seen a value for this argument, then fill in the default.
            if (!SeenValue && Argument.HasDefaultValue)
            {
                if (!TryValidateValue(Argument.DefaultValue, new ArgumentValidationContext(fileSystemReader)))
                {
                    return false;
                }

                if (DestinationObject != null && !TrySetValue(DestinationObject, Argument.DefaultValue))
                {
                    return false;
                }
            }

            // If this is a collection, then create the collection.
            if (Argument.IsCollection && (SeenValue || !Argument.TakesRestOfLine) && (DestinationObject != null))
            {
                if (!TryCreateCollection(Argument.CollectionArgumentType, CollectionValues, out object collection))
                {
                    return false;
                }

                if (!TrySetValue(DestinationObject, collection))
                {
                    return false;
                }
            }

            if (Argument.IsRequired && !SeenValue)
            {
                ReportMissingRequiredArgument();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Parses the provided value string using this object's value type.
        /// </summary>
        /// <param name="setParseState">Current parse state for containing arg set.</param>
        /// <param name="value">The string to parse.</param>
        /// <param name="parsedValue">On success, receives the parsed value.
        /// </param>
        /// <returns>True on success; false otherwise.</returns>
        public bool TryParseAndStore(ArgumentSetParser setParseState, string value, out object parsedValue)
        {
            // Check for disallowed duplicate arguments.
            if (SeenValue && !Argument.AllowMultiple)
            {
                ReportDuplicateArgumentValue(value);

                parsedValue = null;
                return false;
            }

            // Check for conflicting arguments that have already been specified.
            bool conflictsDetected = false;
            foreach (var arg in Argument.ConflictingArgs.Where(setParseState.HasSeenValueFor))
            {
                ReportConflictingArgument(value, arg);
                conflictsDetected = true;
            }

            if (conflictsDetected)
            {
                parsedValue = null;
                return false;
            }

            // Note that we've now seen a value for this argument so we can
            // catch disallowed duplicates later.
            SeenValue = true;

            // Parse the string version of the value.
            if (!ParseValue(value, out object newValue))
            {
                parsedValue = null;
                return false;
            }

            if (!TryValidateValue(newValue, new ArgumentValidationContext(ParseContext.FileSystemReader)))
            {
                parsedValue = null;
                return false;
            }

            if (Argument.IsCollection)
            {
                var newValues = Argument.CollectionArgumentType.ToEnumerable(newValue).Cast<object>();

                // Check for disallowed duplicate values in this argument.
                if (Argument.Unique &&
                    CollectionValues.Cast<object>().Intersect(newValues).Any())
                {
                    ReportDuplicateArgumentValue(value);

                    parsedValue = null;
                    return false;
                }

                // Add the value to the collection.
                foreach (var newValueItem in newValues)
                {
                    CollectionValues.Add(newValueItem);
                }
            }
            else if (IsObjectPresent(DestinationObject))
            {
                if (!TrySetValue(DestinationObject, newValue, value))
                {
                    parsedValue = null;
                    return false;
                }
            }

            parsedValue = newValue;
            return true;
        }

        /// <summary>
        /// Fills out this argument with the remainder of the provided command
        /// line.
        /// </summary>
        /// <param name="restOfLine">Remainder of the command-line tokens.</param>
        /// <returns>true on success; false otherwise.</returns>
        public bool TrySetRestOfLine(IEnumerable<string> restOfLine)
        {
            Debug.Assert(restOfLine != null);
            Debug.Assert(!SeenValue);

            SeenValue = true;

            if (Argument.IsCollection)
            {
                foreach (var arg in restOfLine)
                {
                    CollectionValues.Add(arg);
                }
            }
            else if (IsObjectPresent(DestinationObject))
            {
                var restOfLineAsList = restOfLine.ToList();
                if (!TrySetValue(DestinationObject,
                    CreateCommandLine(restOfLineAsList),
                    string.Join(" ", restOfLineAsList)))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Generate possible completions of this argument that start with the
        /// provided string prefix.
        /// </summary>
        /// <param name="tokens">The set of tokens in the input being completed.
        /// </param>
        /// <param name="indexOfTokenToComplete">The 0-based index of the token
        /// to complete.</param>
        /// <param name="valueToComplete">The prefix string.</param>
        /// <param name="inProgressParsedObject">Optionally, the object
        /// resulting from parsing and processing the tokens before the one
        /// being completed.</param>
        /// <returns>Possible completions.</returns>
        public IEnumerable<string> GetCompletions(IReadOnlyList<string> tokens, int indexOfTokenToComplete, string valueToComplete, object inProgressParsedObject)
        {
            var context = new ArgumentCompletionContext
            {
                ParseContext = ParseContext,
                Tokens = tokens,
                TokenIndex = indexOfTokenToComplete,
                InProgressParsedObject = inProgressParsedObject,
                CaseSensitive = ArgumentSet.Attribute.CaseSensitive
            };

            return Argument.ArgumentType.GetCompletions(context, valueToComplete);
        }

        /// <summary>
        /// Attempts to validate the given value in the provided context.
        /// </summary>
        /// <param name="value">Value to validate.</param>
        /// <param name="validationContext">Context for validation.</param>
        /// <param name="reportInvalidValue">Whether or not invalid values should
        /// be reported.</param>
        /// <returns>true if the value passed validation; false otherwise.</returns>
        public bool TryValidateValue(object value, ArgumentValidationContext validationContext, bool reportInvalidValue = true) =>
            Argument.ValidationAttributes.All(attrib =>
            {
                if (attrib.TryValidate(validationContext, value, out string reason))
                {
                    return true;
                }

                if (reportInvalidValue)
                {
                    ReportBadArgumentValue(Argument.ValueType.Format(value), reason);
                }

                return false;
            });

        private bool TrySetValue(object containingObject, object value, string valueString = null)
        {
            //
            // Try to set the value.  If the member is a property, it's
            // possible that there's a non-default implementation of its
            // 'set' method that further validates values.  Watch out for
            // that and surface any reported errors.
            //

            try
            {
                if (Argument.FixedDestination != null)
                {
                    containingObject = Argument.FixedDestination;
                }

                Argument.Member.SetValue(containingObject, value);
                return true;
            }
            catch (ArgumentException ex)
            {
                ReportBadArgumentValue(valueString ?? value.ToString(), ex);
                return false;
            }
        }

        private bool TryCreateCollection(ICollectionArgumentType argType, IList values, out object collection)
        {
            try
            {
                collection = argType.ToCollection(values);
                return true;
            }
            catch (ArgumentException ex)
            {
                var convertedValues = new List<string>();
                foreach (var v in values)
                {
                    convertedValues.Add(v.ToString());
                }

                ReportBadArgumentValue(string.Join(", ", convertedValues), ex);

                collection = null;
                return false;
            }
        }

        private bool ParseValue(string stringData, out object value)
        {
            if (Argument.ArgumentType.TryParse(ParseContext, stringData, out value))
            {
                return true;
            }

            ReportBadArgumentValue(stringData);

            value = null;
            return false;
        }

        private void ReportMissingRequiredArgument()
        {
            if (Argument.IsPositional)
            {
                ReportLine(Strings.MissingRequiredPositionalArgument, Argument.LongName);
            }
            else
            {
                ReportLine(
                    Strings.MissingRequiredNamedArgument,
                    ArgumentSet.Attribute.NamedArgumentPrefixes.FirstOrDefault() ?? string.Empty,
                    Argument.LongName);
            }
        }

        private void ReportDuplicateArgumentValue(string value) =>
            ReportLine(Strings.DuplicateArgument, Argument.LongName, value);

        private void ReportConflictingArgument(string value, ArgumentDefinition conflictingArg) =>
            ReportLine(Strings.ConflictingArgument, value, Argument.LongName, conflictingArg.LongName);

        private void ReportBadArgumentValue(string value, ArgumentException exception) =>
            ReportBadArgumentValue(value, exception.Message);

        private void ReportBadArgumentValue(string value, string message = null)
        {
            if (message != null)
            {
                ReportLine(Strings.BadArgumentValueWithReason, value, Argument.LongName, message);
            }
            else
            {
                ReportLine(Strings.BadArgumentValue, value, Argument.LongName);
            }

            var values = Argument.ValueType.GetCompletions(new ArgumentCompletionContext { ParseContext = ParseContext }, string.Empty)
                .ToList();

            if (values.Count > 0)
            {
                ReportLine(
                    "  " + Strings.PossibleArgumentValues,
                    string.Join(", ", values.Select(a => "'" + a + "'")));
            }
        }

        private void ReportLine(string message, params object[] args)
        {
            Debug.Assert(Reporter != null);
            Reporter?.Invoke(new ColoredMultistring(
                new[]
                {
                    new ColoredString(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            message + Environment.NewLine,
                            args),
                        ErrorForegroundColor)
                }));
        }

        private static ArgumentParseContext CreateParseContext(ArgumentDefinition argument, ArgumentSetAttribute setAttribute, CommandLineParserOptions options, object containingObject) =>
            new ArgumentParseContext
            {
                NumberOptions = argument.Attribute.NumberOptions,
                ElementSeparators = argument.Attribute.ElementSeparators,
                AllowEmpty = argument.Attribute.AllowEmpty,
                FileSystemReader = options.FileSystemReader,
                ParserContext = options.Context,
                ServiceConfigurer = options.ServiceConfigurer,
                CaseSensitive = setAttribute.CaseSensitive,
                ContainingObject = containingObject
            };

        private static bool IsObjectPresent<T>(T value) =>
            typeof(T).GetTypeInfo().IsValueType ||
            (object)value != null;

        private static string CreateCommandLine(IEnumerable<string> arguments) =>
            string.Join(" ", arguments.Select(StringUtilities.QuoteIfNeeded));
    }
}
