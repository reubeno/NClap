namespace NClap.Metadata
{
    /// <summary>
    /// Abstract base class for integer validation attributes that compare
    /// against a known value.
    /// </summary>
    public abstract class IntegerComparisonValidationAttribute : IntegerValidationAttribute
    {
        /// <summary>
        /// Constructor for derived classes to use.
        /// </summary>
        /// <param name="target">Value to compare against.</param>
        protected IntegerComparisonValidationAttribute(object target)
        {
            Target = target;
        }

        /// <summary>
        /// Fixed comparison value for validation.
        /// </summary>
        public object Target { get; }
    }
}
