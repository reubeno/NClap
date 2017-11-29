using System;
using System.Collections.Generic;
using NClap.ConsoleInput;

namespace NClap.Tests.ConsoleInput
{
    internal class TestTokenCompleter : ITokenCompleter
    {
        Func<IEnumerable<string>, int, IEnumerable<string>> _func;

        public TestTokenCompleter(Func<IEnumerable<string>, int, IEnumerable<string>> func)
        {
            _func = func;
        }

        public TestTokenCompleter(IEnumerable<string> results)
        {
            _func = (tokens, index) => results;
        }

        public IEnumerable<string> GetCompletions(IEnumerable<string> tokens, int tokenIndex) =>
            _func(tokens, tokenIndex);
    }
}
