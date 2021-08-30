using System;
using L3D.Net.Internal.Abstract;
using NSubstitute;

namespace L3D.Net.Tests.Context
{
    internal interface IContextWithFileHandler
    {
        IFileHandler FileHandler { get; }
    }

    internal interface IContextOptionsWithFileHandler
    {
        IContextWithFileHandler Context { get; }
    }

    internal static class ContextWithFileHandlerExtensions
    {
        
        public static TOptions WithTemporaryDirectoryScope<TOptions>(this TOptions options, out IContainerDirectory containerDirectory, out string path) where TOptions : IContextOptionsWithFileHandler
        {
            containerDirectory = Substitute.For<IContainerDirectory>();
            path = Guid.NewGuid().ToString();
            containerDirectory.Path.Returns(path);
            options.Context.FileHandler.CreateContainerDirectory().Returns(containerDirectory);
            return options;
        }
    }
}
