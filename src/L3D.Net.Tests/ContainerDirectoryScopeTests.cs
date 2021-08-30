using System;
using FluentAssertions;
using L3D.Net.Internal;
using L3D.Net.Internal.Abstract;
using NSubstitute;
using NUnit.Framework;

// ReSharper disable ObjectCreationAsStatement

namespace L3D.Net.Tests
{
    [TestFixture]
    public class ContainerDirectoryScopeTests
    {
        [Test]
        public void Constructor_ShouldThrowArgumentNullException_WhenDirectoryIsNull()
        {
            Action action = () => new ContainerDirectoryScope(null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Path_ShouldReturnInnerDirectoryPath()
        {
            var expectedPath = Guid.NewGuid().ToString();
            var directory = Substitute.For<IContainerDirectory>();
            directory.Path.Returns(expectedPath);

            using var scope = new ContainerDirectoryScope(directory);
            scope.Directory.Should().Be(expectedPath);
        }

        [Test]
        public void Dispose_ShouldCallContainerDirectoryCleanUp()
        {
            var directory = Substitute.For<IContainerDirectory>();
            var scope = new ContainerDirectoryScope(directory);
            
            scope.Dispose();
            directory.Received(1).CleanUp();
        }
    }
}