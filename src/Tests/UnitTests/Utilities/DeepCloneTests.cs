using System.Linq;
using System.Reflection;
using FluentAssertions;
using NClap.Utilities;

namespace NClap.Tests.Utilities
{
    public static class DeepCloneTests
    {
        public static void CloneShouldYieldADistinctButEquivalentObject<T>(T instance)
            where T : IDeepCloneable<T>
        {
            var type = instance.GetType();
            var clone = instance.DeepClone();

            clone.Should().NotBeNull();
            clone.Should().NotBeSameAs(instance);
            clone.Should().BeOfType(type);
            clone.Should().BeEquivalentTo(instance);

            var allProps = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in allProps.Where(p => p.PropertyType.GetTypeInfo().IsByRef))
            {
                var instanceValue = prop.GetValue(instance);
                if (instanceValue != null)
                {
                    var cloneValue = prop.GetValue(clone);
                    instanceValue.Should().NotBeSameAs(cloneValue);
                }
            }
        }
    }
}
