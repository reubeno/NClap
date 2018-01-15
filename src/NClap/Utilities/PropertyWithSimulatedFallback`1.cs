﻿using System;

namespace NClap.Utilities
{
    /// <summary>
    /// Helper class to encapsulate a property that may legitimately not be retrievable or
    /// mutable in all scenarios, but for which it's important for calling code to receive
    /// some syntactically valid value.
    /// </summary>
    /// <typeparam name="T">Type of the property\'s value</typeparam>
    internal class PropertyWithSimulatedFallback<T>
    {
        private readonly Func<T> _getter;
        private readonly Action<T> _setter;
        private readonly Predicate<Exception> _fallbackFilter;

        private T _lastKnownValue;

        /// <summary>
        /// Primary constructor.
        /// </summary>
        /// <param name="getter">Inner function to invoke to get the value of the property.</param>
        /// <param name="setter">Inner action to invoke to set the value of the property.</param>
        /// <param name="fallbackFilter">Filter predicate that will be used to decide if an
        /// exception thrown by <paramref name="getter"/> or <paramref name="setter"/> will be
        /// caught and wrapped with the simulated fallback.  If this predicate returns true
        /// for the exception that was thrown, then it will be caught and wrapped; if it returns
        /// false, then this object will not catch the exception and it will be passed back
        /// up the stack.</param>
        /// <param name="initialFallbackValue">The default initial value to be used in fallback cases.</param>
        public PropertyWithSimulatedFallback(
            Func<T> getter, Action<T> setter, Predicate<Exception> fallbackFilter, T initialFallbackValue = default(T))
        {
            this._getter = getter;
            this._setter = setter;
            this._fallbackFilter = fallbackFilter;
            this._lastKnownValue = initialFallbackValue;
        }

        /// <summary>
        /// The value represented by this object.
        /// </summary>
        public T Value
        {
            get
            {
                try
                {
                    return _getter();
                }
                catch (Exception ex) when (_fallbackFilter(ex))
                {
                    return _lastKnownValue;
                }
            }

            set
            {
                try
                {
                    _setter(value);
                }
                catch (Exception ex) when (_fallbackFilter(ex))
                {
                }

                _lastKnownValue = value;
            }
        }
    }
}
