using NClap.Metadata;

namespace MergedCommandApp
{
    internal enum ExtraCommandsType
    {
        [Command(typeof(ExtraCommand), Description = "Do something a bit extra")]
        Extra
    }
}
