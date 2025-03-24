using FluentAssertions;
using L3D.Net.Abstract;
using L3D.Net.Data;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace L3D.Net.Tests;

[TestFixture]
public class ReaderExampleTests
{
    private readonly List<string> _tempDirectories = new();
    private readonly IWriter _writer = new Writer();

    private static List<string> ExampleDirectories()
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
    public void TearDown()
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

    public enum ContainerTypeToTest
    {
        Path,
        Bytes,
        Stream
    }

    public static IEnumerable<ContainerTypeToTest> ContainerTypeToTestEnumValues => Enum.GetValues<ContainerTypeToTest>();

    [Test]
    public void Reader_ShouldBeAbleToReadAllExampleFiles_ContainerPath([ValueSource(nameof(ExampleDirectories))] string exampleDirectory,
        [ValueSource(nameof(ContainerTypeToTestEnumValues))]
        ContainerTypeToTest containerTypeToTest)
    {
        var exampleName = Path.GetFileName(exampleDirectory).ToLower();

        if (!Setup.ExampleBuilderMapping.TryGetValue(exampleName, out var buildFunc))
            throw new Exception($"No test code for example '{exampleName}' available!");

        var containerTempDirectory = GetTempDirectory();

        var luminaire = new Luminaire();

        luminaire = buildFunc(luminaire);

        var containerPath = Path.Combine(containerTempDirectory, "luminaire" + Constants.L3dExtension);

        switch (containerTypeToTest)
        {
            case ContainerTypeToTest.Stream:
                using (var stream = File.OpenWrite(containerPath))
                {
                    _writer.WriteToStream(luminaire, stream);
                }

                break;
            case ContainerTypeToTest.Path:
                _writer.WriteToFile(luminaire, containerPath);
                break;
            case ContainerTypeToTest.Bytes:
                var bytes = _writer.WriteToByteArray(luminaire);
                File.WriteAllBytes(containerPath, bytes);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null);
        }

        Func<Luminaire> readAction = containerTypeToTest switch
        {
            ContainerTypeToTest.Path => () => new Reader().ReadContainer(containerPath),
            ContainerTypeToTest.Bytes => () =>
            {
                var containerBytes = File.ReadAllBytes(containerPath);
                return new Reader().ReadContainer(containerBytes);
            },
            ContainerTypeToTest.Stream => () =>
            {
                using var stream = File.OpenRead(containerPath);
                return new Reader().ReadContainer(stream);
            },
            _ => throw new ArgumentOutOfRangeException(nameof(containerTypeToTest), containerTypeToTest, null)
        };

        var result = readAction();
        result.Should().BeEquivalentTo(luminaire, opt => opt.AllowingInfiniteRecursion());
    }
}