using FluentAssertions;
using L3D.Net.Internal;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace L3D.Net.Tests;

[TestFixture]
public class ContainerDirectoryTests
{
    [Test]
    public void Constructor_ShouldCreateInstanceWithNewDirectoryPath()
    {
        List<string> directories = new();

        for (var i = 0; i < 100; i++)
        {
            var directory = new ContainerDirectory();
            try
            {
                directories.Should().NotContain(directory.Path);
                directories.Add(directory.Path);
            }
            finally
            {
                directory.CleanUp();
            }
        }
    }

    [Test]
    public void Constructor_ShouldCreateDirectory()
    {
        var directory = new ContainerDirectory();
        try
        {
            Directory.Exists(directory.Path).Should().BeTrue();
        }
        finally
        {
            directory.CleanUp();
        }
    }

    [Test]
    public void CleanUp_ShouldDeleteDirectory()
    {
        var directory = new ContainerDirectory();
        var path = directory.Path;
        directory.CleanUp();
        Directory.Exists(path).Should().BeFalse();
    }

    [Test]
    public void CleanUp_ShouldNotThrow_WhenDirectoryCanNotBeDeleted()
    {
        var scope = new ContainerDirectory();
        var directory = scope.Path;

        var openFile = File.OpenWrite(Path.Combine(directory, Guid.NewGuid().ToString()));

        var action = () => scope.CleanUp();
        action.Should().NotThrow();

        openFile.Close();
        openFile.Dispose();

        try
        {
            Directory.Delete(directory, true);
        }
        catch
        {
            // ignored
        }
    }
}