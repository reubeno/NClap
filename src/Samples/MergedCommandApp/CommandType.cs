using NClap.Metadata;

namespace MergedCommandApp
{
    [ExtensibleEnum(typeof(EnumProvider))]
    internal enum CommandType
    {
        [Command(typeof(SimpleCommand), Description = "Keep it simple")]
        Foo
    }
}
