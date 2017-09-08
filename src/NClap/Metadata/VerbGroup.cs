using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NClap.Parser;
using NClap.Types;
using NClap.Utilities;

namespace NClap.Metadata
{
    /// <summary>
    /// Represents a group of verbs, i.e. a verb with sub-verbs.
    /// </summary>
    public class VerbGroup<TVerbType> : Verb where TVerbType : struct
    {
        /// <summary>
        /// The enum value corresponding with the selected verb, or null if no
        /// selection has yet been made (and if there is no default).
        /// </summary>
        public TVerbType? SelectedVerbType { get; set; }

        /// <summary>
        /// The verb presently selected from this group, or null if no
        /// selection has yet been made (and if there is no default).
        /// </summary>
        public IVerb SelectedVerb { get; set; }

        /// <summary>
        /// Executes the verb.
        /// </summary>
        /// <param name="cancel">Cancellation token.</param>
        /// <returns>Result of execution.</returns>
        public override Task<VerbResult> ExecuteAsync(CancellationToken cancel)
        {
            if (SelectedVerb == null)
            {
                return Task.FromResult(VerbResult.UsageError);
            }

            return SelectedVerb.ExecuteAsync(cancel);
        }

        /// <summary>
        /// Constructs a collection of the type described by this object,
        /// populated with objects from the provided input collection.
        /// </summary>
        /// <param name="objects">Objects to add to the collection.</param>
        /// <returns>Constructed collection.</returns>
        public object ToCollection(IEnumerable objects)
        {
            var tokens = new List<string>();
            foreach (string s in objects)
            {
                tokens.Add(s);
            }

            if (!TryParse(tokens, out VerbGroup<TVerbType> verbGroup))
            {
                /* DBG:RRO */
                throw new ArgumentException("Unable to parse", nameof(objects));
            }

            return verbGroup;
        }

        private static bool TryParse(IReadOnlyList<string> tokens, ArgumentParseContext context, out VerbGroup<TVerbType> verbGroup)
        {
            verbGroup = null;

            if (tokens.Count < 1)
            {
                return false;
            }

            if (!ArgumentType.TryGetType(typeof(TVerbType), out IArgumentType verbTypeType))
            {
                return false;
            }

            var verbToken = tokens[0];
            if (!verbTypeType.TryParse(context, verbToken, out object verbTypeObject))
            {
                return false;
            }

            var verbType = (TVerbType)verbTypeObject;
            return TryParse(verbType, tokens.Skip(1).ToList(), out verbGroup);
        }

        private static bool TryParse(TVerbType verbType, IEnumerable<string> tokens, out VerbGroup<TVerbType> verbGroup)
        {
            verbGroup = null;

            var verbTypeName = typeof(TVerbType).GetTypeInfo().GetEnumName(verbType);
            var verbTypeField = typeof(TVerbType).GetTypeInfo().GetField(verbTypeName);
            var verbAttrib = verbTypeField.GetSingleAttribute<VerbAttribute>();
            var implementingType = verbAttrib.GetImplementingType(typeof(TVerbType));

            IVerb verb = null;
            if (implementingType != null)
            {
                var constructor = implementingType.GetTypeInfo().GetConstructor(Array.Empty<Type>());
                if (constructor == null)
                {
                    return false;
                }

                verb = constructor.Invoke(Array.Empty<object>()) as IVerb;
                if (verb == null)
                {
                    return false;
                }

                var options = new CommandLineParserOptions();
                if (!CommandLineParser.Parse(tokens.ToList(), verb, options))
                {
                    return false;
                }
            }

            verbGroup = new VerbGroup<TVerbType>
            {
                SelectedVerb = verb,
                SelectedVerbType = verbType
            };

            return true;
        }
    }
}
