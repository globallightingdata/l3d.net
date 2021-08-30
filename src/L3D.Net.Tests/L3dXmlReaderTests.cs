using System;
using System.IO;
using System.Linq;
using Extensions.Logging.NSubstitute;
using FluentAssertions;
using L3D.Net.Internal.Abstract;
using L3D.Net.XML;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

// ReSharper disable ObjectCreationAsStatement

namespace L3D.Net.Tests
{
    [TestFixture]
    class L3dXmlReaderTests
    {
        class Context
        {
            public IXmlValidator Validator { get; }
            public ILogger Logger { get; }
            public L3dXmlReader Reader { get; }

            public Context()
            {
                Validator = Substitute.For<IXmlValidator>();
                Logger = LoggerSubstitute.Create();
                Reader = new L3dXmlReader(Validator, Logger);
            }
        }

        private Context CreateContext()
        {
            return new Context();
        }
        
        [Test]
        public void Constructor_ShouldThrowArgumentNullException_WhenValidatorIsNull()
        {
            Action action = () => new L3dXmlReader(null, Substitute.For<ILogger>());
            action.Should().Throw<ArgumentNullException>();
        }
        
        [Test]
        public void Constructor_ShouldNotThrowArgumentNullException_WhenLoggerIsNull()
        {
            Action action = () => new L3dXmlReader(Substitute.For<IXmlValidator>(), null);
            action.Should().NotThrow();
        }

        [Test]
        public void Read_ShouldCallValidatorValidateFile()
        {
            var context = CreateContext();

            string filename = Setup.ExampleXmlFiles.First();
            string workingDirectory = Path.GetDirectoryName(filename);
            
            context.Validator
                .ValidateFile(Arg.Any<string>(), out Arg.Any<L3dXmlVersion>(), Arg.Any<ILogger>())
                .Returns(true);

            context.Reader.Read(filename, workingDirectory);

            context.Validator.Received(1).ValidateFile(filename, out Arg.Any<L3dXmlVersion>(), Arg.Any<ILogger>());
        }

        [Test]
        public void Read_ShouldThrowXmlValidationException_WhenValidationFailed()
        {
            var context = CreateContext();

            string filename = XmlValidatorTests.GetNoRootTestFiles().First();
            string workingDirectory = Path.GetDirectoryName(filename);

            var message = Guid.NewGuid().ToString();
            context.Validator
                .ValidateFile(Arg.Any<string>(), out Arg.Any<L3dXmlVersion>(), Arg.Any<ILogger>())
                .Throws(new Exception(message));

            Action action = () => context.Reader.Read(filename, workingDirectory);
            action.Should().Throw<Exception>().WithMessage(message);

            context.Validator.Received(1).ValidateFile(filename, out Arg.Any<L3dXmlVersion>(), Arg.Any<ILogger>());
        }
    } 
}