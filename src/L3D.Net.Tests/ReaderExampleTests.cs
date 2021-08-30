using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace L3D.Net.Tests
{
    [TestFixture]
    public class ReaderExampleTests
    {
        private readonly List<string> _tempDirectories = new();

        static List<string> ExampleDirectories()
        {
            Setup.Initialize();
            var directories = Directory.EnumerateDirectories(Setup.ExamplesDirectory).ToList();
            return directories;
        }

        private string GetTempDirectory()
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), "test.gldf.io", Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDirectory);
            _tempDirectories.Add(tempDirectory);
            return tempDirectory;
        }

        [TearDown]
        public void Deinit()
        {
            foreach (var tempDirectory in _tempDirectories)
            {
                try
                {
                    Directory.Delete(tempDirectory, true);
                }
                catch
                {
                    // ignore
                }
            }
        }

        [Test]
        [TestCaseSource(nameof(ExampleDirectories))]
        public void Reader_ShouldBeAbleToReadAllExampleFiles(string exampleDirectory)
        {
            var exampleName = Path.GetFileName(exampleDirectory).ToLower();

            if (!Setup.ExampleBuilderMapping.TryGetValue(exampleName, out var buildFunc))
                throw new Exception($"No test code for example '{exampleName}' available!");

            var containerTempDirectory = GetTempDirectory();

            var builder = Builder.NewLuminaire();

            builder = buildFunc(builder);

            var containerPath = Path.Combine(containerTempDirectory, "luminaire" + Constants.L3dExtension);
            builder.Build(containerPath);

            Action action = () => new Reader().ReadContainer(containerPath);

            action.Should().NotThrow<Exception>();
        }
    }
}
