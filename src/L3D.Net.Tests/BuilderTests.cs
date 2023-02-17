using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Extensions.Logging.NSubstitute;
using FluentAssertions;
using L3D.Net.API.Dto;
using L3D.Net.BuilderOptions;
using L3D.Net.Data;
using L3D.Net.Exceptions;
using L3D.Net.Internal.Abstract;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

// ReSharper disable ObjectCreationAsStatement
// ReSharper disable UnusedMethodReturnValue.Local

namespace L3D.Net.Tests;

[TestFixture]
class BuilderTests
{
    #region Context

    private const int TempFileCount = 3;
    private static string[] _tempFilenames;

    class Context
    {
        public LuminaireBuilder LuminaireBuilder { get; protected set; }

        public IObjParser ObjectParser { get; }

        public IContainerBuilder ContainerBuilder { get; }
        public ILogger Logger { get; }

        public string KnownToolNameAndVersion { get; }


        public Context()
        {
            KnownToolNameAndVersion = Guid.NewGuid().ToString();

            ObjectParser = Substitute.For<IObjParser>();
            ObjectParser.Parse(Arg.Any<string>(), Arg.Any<ILogger>()).Returns(info =>
            {
                var model = Substitute.For<IModel3D>();
                model.FilePath.Returns(info.Arg<string>());
                model.IsFaceIndexValid(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
                return model;
            });

            ContainerBuilder = Substitute.For<IContainerBuilder>();
            Logger = LoggerSubstitute.Create();
        }

        public string CreatePartName()
        {
            return "PN." + Guid.NewGuid();
        }

    }

    class InternalContext : Context
    {
        protected internal readonly List<Action<LuminaireBuilder>> BuilderTasks = new List<Action<LuminaireBuilder>>();

        public void Init()
        {
            LuminaireBuilder = new LuminaireBuilder(KnownToolNameAndVersion, ObjectParser, ContainerBuilder, Logger);
        }


        public void RunBuilderTasks()
        {
            foreach (var task in BuilderTasks)
            {
                task(LuminaireBuilder);
            }
        }
    }

    class ContextOptions
    {
        private readonly InternalContext _context;

        public ContextOptions(InternalContext context)
        {
            _context = context;
        }

        public ContextOptions WithObjParserThrowing(string path = null)
        {
            if (path == null)
                _context.ObjectParser.Parse(Arg.Any<string>(), Arg.Any<ILogger>()).Throws<Exception>();
            else
                _context.ObjectParser.Parse(Arg.Is(path), Arg.Any<ILogger>()).Throws<Exception>();
            return this;
        }

        public ContextOptions WithSimpleBuild(out string basePartName, out string jointPartName, out string headPartName, out string leoPartName)
        {
            basePartName = _context.CreatePartName();
            jointPartName = _context.CreatePartName();
            headPartName = _context.CreatePartName();
            leoPartName = _context.CreatePartName();

            var localBasePartName = basePartName;
            var localJointPartName = jointPartName;
            var localHeadPartName = headPartName;
            var localLeoPartName = leoPartName;

            _context.BuilderTasks.Add(builder =>
            {
                builder
                    .AddGeometry(localBasePartName, _tempFilenames[0], GeometricUnits.m, geomOptions => geomOptions
                        .AddJoint(localJointPartName, jointOptions => jointOptions
                            .AddGeometry(localHeadPartName, _tempFilenames[1], GeometricUnits.m, headOptions =>
                                headOptions
                                    .AddCircularLightEmittingObject(localLeoPartName, 0.1, leoOptions => leoOptions
                                        .WithLightEmittingSurfaceOnParent("les", lesOptions => lesOptions.WithSurface(10))))));
            });

            return this;
        }
    }

    private Context CreateContext(Action<ContextOptions> options = null)
    {
        var context = new InternalContext();
        options?.Invoke(new ContextOptions(context));
        context.Init();
        context.RunBuilderTasks();
        return context;
    }

    [OneTimeSetUp]
    public void Init()
    {
        _tempFilenames = new string[TempFileCount];

        for (int i = 0; i < TempFileCount; i++)
        {
            _tempFilenames[i] = Path.GetTempFileName();
        }
    }

    [OneTimeTearDown]
    public void Deinit()
    {
        for (int i = 0; i < TempFileCount; i++)
        {
            try
            {
                File.Delete(_tempFilenames[i]);
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }

    #endregion

    #region Constructor

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("     ")]
    public void Constructor_ShouldThrowArgumentException_WhenToolNameAndVersionIsNullOrWhitespace(string toolNameAndVersion)
    {
        Action action = () => new LuminaireBuilder(toolNameAndVersion,
            Substitute.For<IObjParser>(),
            Substitute.For<IContainerBuilder>(),
            Substitute.For<ILogger>());

        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Constructor_ShouldThrowArgumentException_WhenObjParserIsNull()
    {
        Action action = () => new LuminaireBuilder(Guid.NewGuid().ToString(),
            null,
            Substitute.For<IContainerBuilder>(),
            Substitute.For<ILogger>());

        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Constructor_ShouldThrowArgumentException_WhenContainerBuilderIsNull()
    {
        Action action = () => new LuminaireBuilder(Guid.NewGuid().ToString(),
            Substitute.For<IObjParser>(),
            null,
            Substitute.For<ILogger>());

        action.Should().Throw<ArgumentException>();
    }
        
    #endregion

    #region WithTool

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("     ")]
    public void WithTool_ShouldThrowArgumentException_WhenToolNameAndVersionIsNullOrWhitespace(string toolNameAndVersion)
    {
        var context = CreateContext();

        Action action = () => context.LuminaireBuilder.WithTool(toolNameAndVersion);

        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void WithTool_ShouldAssignLuminaireMetaDataTool()
    {
        var contex = CreateContext();
        var expectedToolNameAndVersion = Guid.NewGuid().ToString();

        contex.LuminaireBuilder.WithTool(expectedToolNameAndVersion);

        contex.LuminaireBuilder.Luminaire.Header.CreatedWithApplication.Should().Be(expectedToolNameAndVersion);
    }

    [Test]
    public void WithTool_ShouldReturnSelf()
    {
        var context = CreateContext();

        var builder = context.LuminaireBuilder.WithTool(Guid.NewGuid().ToString());

        builder.Should().BeSameAs(context.LuminaireBuilder);
    }

    #endregion

    #region WithCreatedTime

    [Test]
    public void WithCreatedTime_ShouldAssignValue()
    {
        var context = CreateContext();
        var expectedTime = DateTime.UtcNow;
            
        context.LuminaireBuilder.WithCreatedTime(expectedTime);

        context.LuminaireBuilder.Luminaire.Header.CreationTimeCode.Should().Be(expectedTime);
    }

    #endregion

    #region WithModelName

    [Test]
    public void WithModelName_ShouldAssignLuminaireMetaDataModelName()
    {
        var context = CreateContext();
        var expectedModelName = Guid.NewGuid().ToString();

        context.LuminaireBuilder.WithModelName(expectedModelName);

        context.LuminaireBuilder.Luminaire.Header.Name.Should().Be(expectedModelName);
    }

    [Test]
    [TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
    public void WithModelName_ShouldAssignNullToLuminaireMetaDataModelName_WhenGivenValueIsWhitespace(string modelName)
    {
        var context = CreateContext();

        context.LuminaireBuilder.WithModelName(modelName);

        context.LuminaireBuilder.Luminaire.Header.Name.Should().BeNull();
    }

    [Test]
    public void WithModelName_ShouldReturnSelf()
    {
        var context = CreateContext();

        var builder = context.LuminaireBuilder.WithModelName(Guid.NewGuid().ToString());

        builder.Should().BeSameAs(context.LuminaireBuilder);
    }

    #endregion

    #region WithDescription

    [Test]
    public void WithDescription_ShouldAssignLuminaireMetaDataDescription()
    {
        var context = CreateContext();
        var expectedDescription = Guid.NewGuid().ToString();

        context.LuminaireBuilder.WithDescription(expectedDescription);

        context.LuminaireBuilder.Luminaire.Header.Description.Should().Be(expectedDescription);
    }

    [Test]
    [TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
    public void WithDescription_ShouldAssignNullToLuminaireMetaDataDescription_WhenGivenValueIsWhitespace(string modelName)
    {
        var context = CreateContext();

        context.LuminaireBuilder.WithDescription(modelName);

        context.LuminaireBuilder.Luminaire.Header.Description.Should().BeNull();
    }

    [Test]
    public void WithDescription_ShouldReturnSelf()
    {
        var context = CreateContext();

        var builder = context.LuminaireBuilder.WithDescription(Guid.NewGuid().ToString());

        builder.Should().BeSameAs(context.LuminaireBuilder);
    }

    #endregion

    #region ThrowWhenPartNameIsInvalid

    [Test]
    [TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
    public void ThrowWhenPartNameIsInvalid_ShouldThrowArgumentException_WhenPartNameIsNullOrEmpty(string partName)
    {
        var context = CreateContext();

        Action action = () => context.LuminaireBuilder.ThrowWhenPartNameIsInvalid(partName);

        action.Should().Throw<ArgumentException>().Which.Message.Should().Be("The part name must not be null or empty! (Parameter 'partName')");
    }

    [Test]
    public void ThrowWhenPartNameIsInvalid_ShouldThrowArgumentException_WhenPartNameIsAlreadyTaken()
    {
        List<string> partNames = null;
        var context = CreateContext(options =>
        {
            options.WithSimpleBuild(out var p0, out var p1, out var p2, out var p3);
            partNames = new List<string> { p0, p1, p2, p3 };
        });

        foreach (var partName in partNames)
        {
            Action action = () => context.LuminaireBuilder.ThrowWhenPartNameIsInvalid(partName);
            action.Should().Throw<ArgumentException>();
        }
    }

    [Test]
    [TestCase(".test")]
    [TestCase("1test")]
    [TestCase("$test")]
    [TestCase("%test")]
    [TestCase("-test")]
    [TestCase("te:st")]
    [TestCase("te;st")]
    public void ThrowWhenPartNameIsInvalid_ShouldThrowArgumentException_WhenPartNameDoesNotStartWithLetter(string partName)
    {
        var context = CreateContext();

        Action action = () => context.LuminaireBuilder.ThrowWhenPartNameIsInvalid(partName);

        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void ThrowWhenPartNameIsInvalid_ShouldDoNothing_WhenPartNameIsValigAndNotAvailableYet()
    {
        var context = CreateContext(options => options.WithSimpleBuild(out _, out _, out _, out _));
        Action action = () => context.LuminaireBuilder.ThrowWhenPartNameIsInvalid(context.CreatePartName());
        action.Should().NotThrow();
    }

    #endregion

    #region IsValidLightEmittingPartName

    [Test]
    [TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
    public void IsValidLightEmittingPartName_ShouldReturnFalse_WhenPartNameNullOrEmpty(string partName)
    {
        var context = CreateContext();

        var isValid = context.LuminaireBuilder.IsValidLightEmittingPartName(partName);

        isValid.Should().BeFalse();
    }

    [Test]
    public void IsValidLightEmittingPartName_ShouldReturnFalse_WhenPartNameIsNotKnown()
    {
        var context = CreateContext(options => options.WithSimpleBuild(out _, out _, out _, out _));

        var isValid = context.LuminaireBuilder.IsValidLightEmittingPartName(context.CreatePartName());

        isValid.Should().BeFalse();
    }

    [Test]
    public void IsValidLightEmittingPartName_ShouldReturnFalse_WhenPartNameIsKnownButNotLightEmittingPart()
    {
        List<string> partNames = null;
        var context = CreateContext(options =>
        {
            options.WithSimpleBuild(out var p0, out var p1, out var p2, out _);
            partNames = new List<string> { p0, p1, p2 };
        });

        foreach (var partName in partNames)
        {
            var isValid = context.LuminaireBuilder.IsValidLightEmittingPartName(partName);

            isValid.Should().BeFalse();
        }
    }

    [Test]
    public void IsValidLightEmittingPartName_ShouldReturnTrue_WhenPartNameBelongsToALightEmittingPart()
    {
        string leoPartName = null;
        var context = CreateContext(options => options.WithSimpleBuild(out _, out _, out _, out leoPartName));

        var isValid = context.LuminaireBuilder.IsValidLightEmittingPartName(leoPartName);

        isValid.Should().BeTrue();
    }

    #endregion

    #region AddGeometry

    [Test]
    [TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
    public void AddGeometry_ShouldThrowArgumentException_WhenPartNameIsNullOrWhitespace(string partName)
    {
        var context = CreateContext();

        Action action = () => context.LuminaireBuilder.AddGeometry(partName, Guid.NewGuid().ToString(), GeometricUnits.m);

        action.Should().Throw<ArgumentException>().And.ParamName.Should().Be("partName");
    }

    [Test]
    [TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
    public void AddGeometry_ShouldThrowArgumentException_WhenModelFilePathIsNullOrWhitespace(string modelFilePath)
    {
        var context = CreateContext();

        Action action = () => context.LuminaireBuilder.AddGeometry(context.CreatePartName(), modelFilePath, GeometricUnits.m);

        action.Should().Throw<ArgumentException>().And.ParamName.Should().Be("modelFilePath");
    }

    [Test]
    public void AddGeometry_ShouldThrowFileNotFoundException_WhenTheModelFilePathIsInvalid()
    {
        var context = CreateContext();
        var modelFilePath = Guid.NewGuid().ToString();
        var expectedAbsoluteModelFilePath = Path.Combine(Environment.CurrentDirectory, modelFilePath);

        Action action = () => context.LuminaireBuilder.AddGeometry(context.CreatePartName(), modelFilePath, GeometricUnits.m);

        action.Should().Throw<FileNotFoundException>().Which.FileName.Should().Be(expectedAbsoluteModelFilePath);
    }

    [Test]
    public void AddGeometry_ShouldCallObjParserParse_WhenModelFilePathIsNewAndValid()
    {
        var context = CreateContext();

        context.LuminaireBuilder.AddGeometry(context.CreatePartName(), _tempFilenames[0], GeometricUnits.m);

        context.ObjectParser.Received(1).Parse(_tempFilenames[0], Arg.Any<ILogger>());
    }
        
    [Test]
    public void AddGeometry_ShouldNotCreateGeometryDefinition_WhenModelFilePathIsAlreadyKnown()
    {
        var context = CreateContext();

        context.LuminaireBuilder.AddGeometry(context.CreatePartName(), _tempFilenames[0], GeometricUnits.m);
        context.LuminaireBuilder.AddGeometry(context.CreatePartName(), _tempFilenames[0], GeometricUnits.m);

        context.LuminaireBuilder.Luminaire.GeometryDefinitions.Should().HaveCount(1);
    }

    [Test]
    public void AddGeometry_ShouldThrowArgumentException_WhenDifferentUnitsAreUsedForTheSameModelFilePath()
    {
        var context = CreateContext();

        context.LuminaireBuilder.AddGeometry(context.CreatePartName(), _tempFilenames[0], GeometricUnits.m);
        Action action = () => context.LuminaireBuilder.AddGeometry(context.CreatePartName(), _tempFilenames[0], GeometricUnits.mm);

        action.Should().Throw<ArgumentException>().Which.ParamName.Should().Be("modelUnits");
    }

    [Test]
    public void AddGeometry_ShouldCreateNewGeometryNode_WhenModelFilePathIsNewAndValid()
    {
        var context = CreateContext();
        var expectedPartName = context.CreatePartName();

        context.LuminaireBuilder.AddGeometry(expectedPartName, _tempFilenames[0], GeometricUnits.m);

        context.LuminaireBuilder.Luminaire.Parts.Single().Should().BeEquivalentTo(new GeometryPart(expectedPartName, context.LuminaireBuilder.Luminaire.GeometryDefinitions.ElementAt(0)));
    }

    [Test]
    public void AddGeometry_ShouldCreateNewGeometryNode_WhenModelFilePathIsAlreadyKnown()
    {
        var context = CreateContext();
        var expectedPartName = context.CreatePartName();
        context.LuminaireBuilder.AddGeometry(context.CreatePartName(), _tempFilenames[0], GeometricUnits.m);

        context.LuminaireBuilder.AddGeometry(expectedPartName, _tempFilenames[0], GeometricUnits.m);

        context.LuminaireBuilder.Luminaire.Parts.ElementAt(1).Should().BeEquivalentTo(new GeometryPart(expectedPartName, context.LuminaireBuilder.Luminaire.GeometryDefinitions.ElementAt(0)));
    }

    [Test]
    public void AddGeometry_ShouldCreateAnotherNewGeometryDefinition_WhenModelFilePathIsNewAndValid()
    {
        var context = CreateContext();

        context.LuminaireBuilder.AddGeometry(context.CreatePartName(), _tempFilenames[0], GeometricUnits.m);
        context.LuminaireBuilder.AddGeometry(context.CreatePartName(), _tempFilenames[1], GeometricUnits.mm);

        context.LuminaireBuilder.Luminaire.GeometryDefinitions.Should().HaveCount(2);
    }

    [Test]
    public void AddGeometry_ShouldCreateAnotherNewGeometryPart_WhenModelFilePathIsAlreadyKnown()
    {
        var context = CreateContext();
        var expectedPartName = context.CreatePartName();

        context.LuminaireBuilder.AddGeometry(context.CreatePartName(), _tempFilenames[0], GeometricUnits.m);

        context.LuminaireBuilder.AddGeometry(expectedPartName, _tempFilenames[0], GeometricUnits.m);

        context.LuminaireBuilder.Luminaire.Parts.ElementAt(1).Should().BeEquivalentTo(new GeometryPart(expectedPartName, context.LuminaireBuilder.Luminaire.GeometryDefinitions.ElementAt(0)));
    }

    [Test]
    public void AddGeometry_ShouldThrowArgumentException_WhenPartNameIsAlreadyTaken()
    {
        var context = CreateContext();
        var partName = context.CreatePartName();

        context.LuminaireBuilder.AddGeometry(partName, _tempFilenames[0], GeometricUnits.m);

        Action action = () => context.LuminaireBuilder.AddGeometry(partName, _tempFilenames[1], GeometricUnits.mm);

        action.Should().Throw<ArgumentException>().Which.ParamName.Should().Be("partName");
    }

    [Test]
    public void AddGeometry_ShouldThrowModelParseException_WhenParserThrowsAnyException()
    {
        var context = CreateContext(options => options.WithObjParserThrowing());
        var partName = context.CreatePartName();

        Action action = () => context.LuminaireBuilder.AddGeometry(partName, _tempFilenames[0], GeometricUnits.mm);

        action.Should().Throw<ModelParseException>();
    }

    [Test]
    public void AddGeometry_ShouldReturnSelf()
    {
        var context = CreateContext();

        var builder = context.LuminaireBuilder.AddGeometry(context.CreatePartName(), _tempFilenames[0], GeometricUnits.m);

        builder.Should().BeSameAs(context.LuminaireBuilder);
    }

    [Test]
    public void AddGeometry_ShouldExecuteOptionsFuncWithGeometryOptions_WhenNotNull()
    {
        var context = CreateContext();

        var optionsFunc = Substitute.For<Func<GeometryOptions, GeometryOptions>>();

        context.LuminaireBuilder.AddGeometry(context.CreatePartName(), _tempFilenames[0], GeometricUnits.m, optionsFunc);

        optionsFunc.Received(1).Invoke(Arg.Is<GeometryOptions>(options => options != null));
    }
        
    #endregion

    #region Build

    [Test]
    public void Build_ShouldCallContainerBuilderCreateContainer()
    {
        var context = CreateContext();
        var containerPath = Guid.NewGuid().ToString();

        context.LuminaireBuilder.Build(containerPath);

        context.ContainerBuilder.Received(1).CreateContainer(Arg.Is<Luminaire>(luminaire => luminaire != null), Arg.Is(containerPath));
    }

    #endregion

    [Test]
    [TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
    public void TryGetPartByName_ShouldReturnFalse_WhenPartNameIsNullOrEmpty(string partName)
    {
        var context = CreateContext();

        var found = context.LuminaireBuilder.TryGetPartByName(partName, out _);

        found.Should().BeFalse();
    }
}