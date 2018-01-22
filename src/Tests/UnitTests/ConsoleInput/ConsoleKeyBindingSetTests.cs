using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.ConsoleInput;

namespace NClap.Tests.ConsoleInput
{
    [TestClass]
    public class ConsoleKeyBindingSetTests
    {
        [TestMethod]
        public void EmptyBindings()
        {
            var bindings = new ConsoleKeyBindingSet();
            bindings.Count.Should().Be(0);
        }

        [TestMethod]
        public void DefaultBindings()
        {
            var bindings = ConsoleKeyBindingSet.Default;
            bindings.Count.Should().BeGreaterThan(0);
        }

        [TestMethod]
        public void EnumerateBindings()
        {
            var bindings = new ConsoleKeyBindingSet();
            bindings.Bind(ConsoleKey.E, ConsoleModifiers.Shift, ConsoleInputOperation.BackwardKillWord);
            bindings.Bind(ConsoleKey.F, ConsoleModifiers.Control, ConsoleInputOperation.AcceptLine);

            var enumerable = (IEnumerable)bindings;
            foreach (KeyValuePair<ConsoleKeyInfo, ConsoleInputOperation> pair in enumerable)
            {
                if (pair.Key.Key == ConsoleKey.E)
                {
                    pair.Key.Modifiers.Should().Be(ConsoleModifiers.Shift);
                    pair.Value.Should().Be(ConsoleInputOperation.BackwardKillWord);
                }
                else if (pair.Key.Key == ConsoleKey.F)
                {
                    pair.Key.Modifiers.Should().Be(ConsoleModifiers.Control);
                    pair.Value.Should().Be(ConsoleInputOperation.AcceptLine);
                }
                else
                {
                    Assert.Fail();
                }
            }
        }

        [TestMethod]
        public void AddBinding()
        {
            const ConsoleInputOperation op = ConsoleInputOperation.Undo;

            var bindings = new ConsoleKeyBindingSet();
            bindings.Bind(ConsoleKey.D, ConsoleModifiers.Control, op);

            var keyInfo = new ConsoleKeyInfo('\x04', ConsoleKey.D, false, false, true);
            bindings.Count.Should().Be(1);
            bindings.ContainsKey(keyInfo).Should().BeTrue();
            bindings.Keys.Should().Equal(keyInfo);
            bindings.Values.Should().Equal(op);

            bindings[keyInfo].Should().Be(op);

            var nonExistentKeyInfo = new ConsoleKeyInfo('d', ConsoleKey.D, false, false, false);
            Action lookupAction = () => { var x = bindings[nonExistentKeyInfo]; };
            lookupAction.Should().Throw<KeyNotFoundException>();

            var pairs = bindings.ToList();
            pairs.Should().ContainSingle();
            pairs.Should().Equal(new KeyValuePair<ConsoleKeyInfo, ConsoleInputOperation>(keyInfo, op));
        }

        [TestMethod]
        public void SeveralBindings()
        {
            var bindings = new ConsoleKeyBindingSet();

            bindings.Bind(ConsoleKey.F, ConsoleModifiers.Control | ConsoleModifiers.Alt, ConsoleInputOperation.AcceptLine);
            bindings.Bind(ConsoleKey.E, ConsoleModifiers.Alt, ConsoleInputOperation.BackwardKillWord);
            bindings.Bind(ConsoleKey.D, ConsoleModifiers.Control, ConsoleInputOperation.Abort);
            bindings.Bind(ConsoleKey.C, ConsoleModifiers.Shift, ConsoleInputOperation.BackwardWord);
            bindings.Bind(ConsoleKey.Tab, (ConsoleModifiers)0, ConsoleInputOperation.ForwardChar);

            bindings.Bind('@', ConsoleModifiers.Control | ConsoleModifiers.Alt, ConsoleInputOperation.BeginningOfHistory);
            bindings.Bind('u', ConsoleModifiers.Alt, ConsoleInputOperation.BeginningOfHistory);
            bindings.Bind('|', ConsoleModifiers.Control, ConsoleInputOperation.BeginningOfHistory);
            bindings.Bind('/', (ConsoleModifiers)0, ConsoleInputOperation.BeginningOfHistory);

            bindings.Count.Should().Be(9);

            var pairs = bindings.ToList();
        }

        [TestMethod]
        public void RemoveKeyBinding()
        {
            const ConsoleInputOperation op = ConsoleInputOperation.Undo;

            var bindings = new ConsoleKeyBindingSet();
            bindings.Bind(ConsoleKey.D, ConsoleModifiers.Control, op);

            bindings.Bind(ConsoleKey.D, ConsoleModifiers.Control, null);
            bindings.Values.Should().BeEmpty();
        }

        [TestMethod]
        public void RemoveCharBinding()
        {
            const ConsoleInputOperation op = ConsoleInputOperation.Undo;

            var bindings = new ConsoleKeyBindingSet();
            bindings.Bind('@', ConsoleModifiers.Control, op);

            bindings.Bind('@', ConsoleModifiers.Control, null);
            bindings.Values.Should().BeEmpty();
        }

        [TestMethod]
        public void ReplaceBinding()
        {
            const ConsoleInputOperation oldOp = ConsoleInputOperation.Undo;
            const ConsoleInputOperation newOp = ConsoleInputOperation.Abort;
            var anyKey = Any.Enum<ConsoleKey>();

            var bindings = new ConsoleKeyBindingSet();
            bindings.Bind(anyKey, ConsoleModifiers.Control, oldOp);
            bindings.Bind(anyKey, ConsoleModifiers.Control, newOp);

            bindings.Values.Should().ContainSingle();

            var keyInfo = new ConsoleKeyInfo('\x04', anyKey, false, false, true);
            bindings[keyInfo].Should().Be(newOp);
        }

        [TestMethod]
        public void TestThatKeyMayBeBoundWithIgnoredModifiers()
        {
            var anyKey = ConsoleKey.C;
            var anyKeyChar = 'c';
            var anyOp = Any.Enum<ConsoleInputOperation>();

            var bindings = new ConsoleKeyBindingSet();
            bindings.BindWithIgnoredModifiers(anyKey, anyOp);

            // It's not enumerable.
            bindings.Values.Should().BeEmpty();

            // ...but it's retrievable.
            void ValidateItIsRetrievableWithModifier(ConsoleModifiers mods)
            {
                bindings.TryGetValue(new ConsoleKeyInfo(anyKeyChar, anyKey, false, false, false), out ConsoleInputOperation op)
                    .Should().BeTrue();
                op.Should().Be(anyOp);
            }

            ValidateItIsRetrievableWithModifier((ConsoleModifiers)0);

            ValidateItIsRetrievableWithModifier(ConsoleModifiers.Alt);
            ValidateItIsRetrievableWithModifier(ConsoleModifiers.Control);
            ValidateItIsRetrievableWithModifier(ConsoleModifiers.Shift);

            ValidateItIsRetrievableWithModifier(ConsoleModifiers.Alt | ConsoleModifiers.Control);
            ValidateItIsRetrievableWithModifier(ConsoleModifiers.Alt | ConsoleModifiers.Shift);
            ValidateItIsRetrievableWithModifier(ConsoleModifiers.Control | ConsoleModifiers.Shift);

            ValidateItIsRetrievableWithModifier(ConsoleModifiers.Alt | ConsoleModifiers.Control | ConsoleModifiers.Shift);
        }

        [TestMethod]
        public void TestThatKeyWithIgnoredModifiersMayBeUnbound()
        {
            var anyKey = ConsoleKey.C;
            var anyKeyChar = 'c';
            var anyOp = Any.Enum<ConsoleInputOperation>();

            var bindings = new ConsoleKeyBindingSet();
            bindings.BindWithIgnoredModifiers(anyKey, anyOp);
            bindings.BindWithIgnoredModifiers(anyKey, null);

            bindings.TryGetValue(new ConsoleKeyInfo(anyKeyChar, anyKey, false, false, false), out ConsoleInputOperation op)
                .Should().BeFalse();
        }
    }
}
