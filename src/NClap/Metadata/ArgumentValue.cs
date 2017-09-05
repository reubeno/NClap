namespace NClap.Metadata
{
    internal class ArgumentValue
    {
        public ArgumentValue(ArgumentValueAttribute attribute)
        {
            Attribute = attribute;
        }

        ArgumentValueAttribute Attribute { get; }
    }
}
