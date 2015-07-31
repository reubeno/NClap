using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using NClap.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NClap.Parser;

using NSubstitute;

namespace NClap.Tests.Types
{
    [TestClass]
    public class FileSystemPathTests
    {
        [TestMethod]
        public void ImplicitConversionOfAbsolutePath()
        {
            const string originalPath = @"r:\some\path";

            FileSystemPath path = originalPath;
            path.OriginalPath.Should().Be(originalPath);
            path.Path.Should().Be(originalPath);
        }

        [TestMethod]
        public void ImplicitConversionOfRelativePath()
        {
            const string originalPath = @"relative\path";

            FileSystemPath path = originalPath;
            path.OriginalPath.Should().Be(originalPath);
            path.Path.Should().Be(originalPath);
        }

        [TestMethod]
        public void ImplicitConversionBackToString()
        {
            var path = new FileSystemPath(@"relative\path", false, @"c:\");

            string pathString = path;
            pathString.Should().Be(@"c:\relative\path");
        }

        [TestMethod]
        public void ExplicitConversion()
        {
            const string originalPath = @"c:\path";

            var path = (FileSystemPath)originalPath;
            path.OriginalPath.Should().Be(originalPath);
            path.Path.Should().Be(originalPath);
        }

        [TestMethod]
        public void AsUsage()
        {
            const string originalPath = @"c:\path";
            var opaqueOriginalPath = (object)originalPath;

            var path = opaqueOriginalPath as FileSystemPath;
            path.Should().BeNull();
        }

        [TestMethod]
        public void AbsolutePathWithResolution()
        {
            const string originalPath = @"c:\absolute\path";
            const string rootPath = @"c:\foo";

            var path = new FileSystemPath(originalPath, false /* expand? */, rootPath);
            path.OriginalPath.Should().Be(originalPath);
            path.Path.Should().Be(originalPath);
        }

        [TestMethod]
        public void RelativePathWithResolution()
        {
            const string originalPath = @"relative\path";
            const string rootPath = @"c:\foo";

            var path = new FileSystemPath(originalPath, false /* expand? */, rootPath);
            path.OriginalPath.Should().Be(originalPath);
            path.Path.Should().Be(Path.Combine(rootPath, originalPath));
        }

        [TestMethod]
        public void DriveRelativePathWithResolution()
        {
            const string originalPath = @"\relative\path";
            const string rootPath = @"c:\foo";

            var path = new FileSystemPath(originalPath, false /* expand? */, rootPath);
            path.OriginalPath.Should().Be(originalPath);
            path.Path.Should().Be(@"c:\relative\path");
        }

        [TestMethod]
        public void GetCompletionsWithInvalidContext()
        {
            Action getCompletions = () => FileSystemPath.GetCompletions(null, @"h:\foo");
            getCompletions.ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void GetCompletionsOfNullOrEmptyString()
        {
            var completions = new[] { "hello", "world" };

            var reader = Substitute.For<IFileSystemReader>();
            reader.EnumerateFileSystemEntries(".", "*").Returns(completions);

            var context = CreateContext(reader);
            FileSystemPath.GetCompletions(context, null).Should().Contain(completions);
            FileSystemPath.GetCompletions(context, string.Empty).Should().Contain(completions);
        }

        [TestMethod]
        public void GetCompletionsOfPathWithoutDirectory()
        {
            var completions = new[] { "hello" };

            var reader = Substitute.For<IFileSystemReader>();
            reader.EnumerateFileSystemEntries(".", "h*").Returns(completions);

            var context = CreateContext(reader);
            FileSystemPath.GetCompletions(context, "h").Should().Contain(completions);
        }

        [TestMethod]
        public void GetCompletionsWhenReaderThrowsIoException()
        {
            var reader = Substitute.For<IFileSystemReader>();
            reader.EnumerateFileSystemEntries(".", "h*").Returns(call => { throw new DirectoryNotFoundException(); });

            var context = CreateContext(reader);
            FileSystemPath.GetCompletions(context, "h").Should().BeEmpty();
        }

        [TestMethod]
        public void GetCompletionsWhenReaderThrowsArgumentException()
        {
            var reader = Substitute.For<IFileSystemReader>();
            reader.EnumerateFileSystemEntries(".", "h*").Returns(call => { throw new ArgumentOutOfRangeException(); });

            var context = CreateContext(reader);
            FileSystemPath.GetCompletions(context, "h").Should().BeEmpty();
        }

        [TestMethod]
        public void GetCompletionsWhenReaderThrowsUnexpectedException()
        {
            var reader = Substitute.For<IFileSystemReader>();
            reader.EnumerateFileSystemEntries(".", "h*").Returns(call => { throw new NotImplementedException(); });

            var context = CreateContext(reader);

            Action completeAction = () => FileSystemPath.GetCompletions(context, "h");
            completeAction.ShouldThrow<NotImplementedException>();
        }

        [TestMethod]
        public void GetHashCodeWorksCorrectly()
        {
            var path = new FileSystemPath("Hello");
            path.GetHashCode().Should().Be(path.GetHashCode());

            Func<string, int> hash = s => new FileSystemPath(s).GetHashCode();

            hash("Hello").Should().Be(hash("Hello"));
            hash("Hello").Should().Be(hash("HELLO"));
            hash("Hello").Should().NotBe(hash("Hello "));
        }

        private static ArgumentCompletionContext CreateContext(IFileSystemReader reader = null)
        {
            return new ArgumentCompletionContext
            {
                ParseContext = new ArgumentParseContext { FileSystemReader = reader },
                Tokens = new List<string> { string.Empty },
                TokenIndex = 0
            };
        }
    }
}
