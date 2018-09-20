using System.Collections.Generic;
using NClap.Types;

namespace MergedCommandApp
{
    internal class EnumProvider : IEnumArgumentTypeProvider
    {
        public IEnumerable<IEnumArgumentType> GetTypes()
        {
            return new[] { (IEnumArgumentType)ArgumentType.GetType(typeof(ExtraCommandsType)) };
        }
    }
}
