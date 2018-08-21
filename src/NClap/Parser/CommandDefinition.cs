using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NClap.Exceptions;
using NClap.Metadata;

namespace NClap.Parser
{
    /// <summary>
    /// Describes a command.
    /// </summary>
    internal class CommandDefinition
    {
        /// <summary>
        /// Defines a command.
        /// </summary>
        /// <param name="key">Value that was used / can be used to select this command.</param>
        /// <param name="implementingType">Type that implements this command.</param>
        public CommandDefinition(object key, Type implementingType)
        {
            Key = key;
            ImplementingType = implementingType ?? throw new ArgumentNullException(nameof(implementingType));

            if (!typeof(ICommand).GetTypeInfo().IsAssignableFrom(ImplementingType.GetTypeInfo()))
            {
                throw new InvalidCommandException($"Type {ImplementingType.FullName} does not implement required {typeof(ICommand).FullName} interface.");
            }
        }

        /// <summary>
        /// Value that selects this command.
        /// </summary>
        public object Key { get; }

        /// <summary>
        /// Implementing type for this command.
        /// </summary>
        public Type ImplementingType { get; }

        /// <summary>
        /// Instantiate the command.
        /// </summary>
        /// <param name="serviceConfigurer">Service configurer.</param>
        /// <returns>Instantiated command object.</returns>
        internal ICommand Instantiate(ServiceConfigurer serviceConfigurer)
        {
            var services = new ServiceCollection();

            serviceConfigurer?.Invoke(services);

            services.AddTransient(typeof(ICommand), ImplementingType);

            var provider = services.BuildServiceProvider();

            try
            {
                return provider.GetService<ICommand>();
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidCommandException($"No matching command constructor could be found on type '{ImplementingType.FullName}'.", ex);
            }
        }
    }
}
