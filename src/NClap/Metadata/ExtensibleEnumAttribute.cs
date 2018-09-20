﻿using System;
using NClap.Types;

namespace NClap.Metadata
{
    /// <summary>
    /// Attribute that indicates the associated enum type is extensible.
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum, AllowMultiple = true)]
    public sealed class ExtensibleEnumAttribute : Attribute
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="provider">Provider.</param>
        public ExtensibleEnumAttribute(Type provider)
        {
            Provider = provider;
        }

        /// <summary>
        /// Implementation of <see cref="IEnumArgumentTypeProvider"/>.
        /// </summary>
        public Type Provider { get; set; }
    }
}
