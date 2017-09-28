using System;
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
        public void AddBinding()
        {
            const ConsoleInputOperation op = ConsoleInputOperation.Undo;

            var bindings = new ConsoleKeyBindingSet();
            bindings.Bind(ConsoleKey.D, ConsoleModifiers.Control, op);

            var keyInfo = new ConsoleKeyInfo('\x04', ConsoleKey.D, false, false, true);
            bindings.Count.Should().Be(1);
            bindings.ContainsKey(keyInfo).Should().BeTrue();
            bindings.Keys.Should().ContainInOrder(keyInfo);
            bindings.Values.Should().ContainInOrder(op);

            bindings[keyInfo].Should().Be(op);

            var nonExistentKeyInfo = new ConsoleKeyInfo('d', ConsoleKey.D, false, false, false);
            Action lookupAction = () => { var x = bindings[nonExistentKeyInfo]; };
            lookupAction.ShouldThrow<KeyNotFoundException>();

            var pairs = bindings.ToList();
            pairs.Should().HaveCount(1);
            pairs.Should().ContainInOrder(new KeyValuePair<ConsoleKeyInfo, ConsoleInputOperation>(keyInfo, op));
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

            var bindings = new ConsoleKeyBindingSet();
            bindings.Bind(ConsoleKey.D, ConsoleModifiers.Control, oldOp);
            bindings.Bind(ConsoleKey.D, ConsoleModifiers.Control, newOp);

            bindings.Values.Should().HaveCount(1);

            var keyInfo = new ConsoleKeyInfo('\x04', ConsoleKey.D, false, false, true);
            bindings[keyInfo].Should().Be(newOp);
        }
    }
}
