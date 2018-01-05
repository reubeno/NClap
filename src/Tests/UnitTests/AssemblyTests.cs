using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using FluentAssertions.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NClap.Tests
{
    [TestClass]
    public class AssemblyTests
    {
        public static readonly Type RepresentativeType = typeof(CommandLineParser);

        public static readonly Assembly AssemblyUnderTest = RepresentativeType.GetTypeInfo().Assembly;

        [TestMethod]
        public void TestThatAllPublicTypesInAssemblyStartWithCorrectNamespacePrefix()
        {
            const string expectedNs = nameof(NClap);
            const string expectedNsWithDot = expectedNs + ".";

            foreach (var type in AllPublicTypes())
            {
                var ns = type.Namespace;
                if (ns != expectedNs)
                {
                    ns.Should().StartWith(expectedNsWithDot);
                }
            }
        }

        [TestMethod]
        public void TestThatNoPublicTypeInAssemblyContainsNonPrivateField()
        {
            var noNonPrivateFields = true;
            foreach (var type in AllPublicTypes().Where(t => !t.IsEnum))
            {
                foreach (var member in type.GetTypeInfo().GetFields().Where(f => !IsConstant(f)))
                {
                    if (!member.IsPrivate)
                    {
                        Console.Error.WriteLine($"Found non-private field in public type: {type.FullName} :: {member.Name}");
                        noNonPrivateFields = false;
                    }
                }
            }

            noNonPrivateFields.Should().BeTrue();
        }

        private static bool IsConstant(FieldInfo field) =>
            field.IsLiteral && !field.IsInitOnly;

        private static IEnumerable<Type> AllPublicTypes() => AllTypes.From(AssemblyUnderTest).Where(t => t.GetTypeInfo().IsPublic);
    }
}
