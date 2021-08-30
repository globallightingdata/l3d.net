using System;
using System.Linq;
using System.Numerics;
using FluentAssertions;
using L3D.Net.API.Dto;
using L3D.Net.Data;
using L3D.Net.Geometry;
using L3D.Net.Internal.Abstract;
using NSubstitute;
using NUnit.Framework;

// ReSharper disable ObjectCreationAsStatement

namespace L3D.Net.Tests
{
    [TestFixture]
    class LuminaireDataTests
    {
        [Test]
        [TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
        public void GeometryDefinition_Constructor_ShouldThrowArgumentException_WhenIdIsNullOrEmpty(string id)
        {
            Action action = () => new GeometryDefinition(id, new ObjModel3D(), GeometricUnits.m);
            action.Should().Throw<ArgumentException>();
        }

        [Test]
        public void GeometryDefinition_Constructor_ShouldThrowArgumentNullException_WhenModelIsNull()
        {
            Action action = () => new GeometryDefinition(Guid.NewGuid().ToString(), null, GeometricUnits.m);
            action.Should().Throw<ArgumentException>();
        }

        [Test]
        [TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
        public void GeometryPart_Constructor_ShouldThrowArgumentException_WhenPartNameIsNullOrEmpty(string partName)
        {
            Action action = () => new GeometryPart(partName, new GeometryDefinition(Guid.NewGuid().ToString(), Substitute.For<IModel3D>(), GeometricUnits.m));
            action.Should().Throw<ArgumentException>();
        }

        [Test]
        [TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
        public void LightEmittingPart_Constructor_ShouldThrowArgumentException_WhenPartNameIsNullOrEmpty(string partName)
        {
            Action action = () => new LightEmittingPart(partName, new Circle(1));
            action.Should().Throw<ArgumentException>();
        }

        [Test]
        public void LightEmittingPart_Constructor_ShouldThrowArgumentNullException_WhenShapeIsNull()
        {
            Action action = () => new LightEmittingPart(Guid.NewGuid().ToString(), null);
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        [TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
        public void JointPart_Constructor_ShouldThrowArgumentException_WhenPartNameIsNullOrEmpty(string partName)
        {
            Action action = () => new JointPart(partName);
            action.Should().Throw<ArgumentException>();
        }

        [Test]
        [TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
        public void SensorPart_Constructor_ShouldThrowArgumentException_WhenPartNameIsNullOrEmpty(string partName)
        {
            Action action = () => new SensorPart(partName);
            action.Should().Throw<ArgumentException>();
        }

        [Test]
        [TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
        public void SingleLightEmittingFaceAssignement__Constructor_ShouldThrowArgumentException_WhenPartNameIsNullOrEmpty(string partName)
        {
            Action action = () => new SingleLightEmittingFaceAssignment(partName, 1, 1);
            action.Should().Throw<ArgumentException>();
        }

        [Test]
        public void SingleLightEmittingFaceAssignement_Constructor_ShouldThrowArgumentOutOfRangeException_WhenGroupIndexIsBelowZero()
        {
            Action action = () => new SingleLightEmittingFaceAssignment(Guid.NewGuid().ToString(), -1, 0);
            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Test]
        public void SingleLightEmittingFaceAssignement_Constructor_ShouldThrowArgumentOutOfRangeException_WhenFaceIndexIsBelowZero()
        {
            Action action = () => new SingleLightEmittingFaceAssignment(Guid.NewGuid().ToString(), 0, -1);
            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Test]
        [TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
        public void LightEmittingFaceRangeAssignement__Constructor_ShouldThrowArgumentException_WhenPartNameIsNullOrEmpty(string partName)
        {
            Action action = () => new LightEmittingFaceRangeAssignment(partName, 0, 0, 2);
            action.Should().Throw<ArgumentException>();
        }

        [Test]
        public void LightEmittingFaceRangeAssignement_Constructor_ShouldThrowArgumentOutOfRangeException_WhenGroupIndexIsBelowZero()
        {
            Action action = () => new LightEmittingFaceRangeAssignment(Guid.NewGuid().ToString(), -1, 0, 2);
            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Test]
        public void LightEmittingFaceRangeAssignement_Constructor_ShouldThrowArgumentOutOfRangeException_WhenFaceIndexBeginIsBelowZero()
        {
            Action action = () => new LightEmittingFaceRangeAssignment(Guid.NewGuid().ToString(), 0, -1, 1);
            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Test]
        public void LightEmittingFaceRangeAssignement_Constructor_ShouldThrowArgumentOutOfRangeException_WhenFaceIndexEndIsBelowZero()
        {
            Action action = () => new LightEmittingFaceRangeAssignment(Guid.NewGuid().ToString(), 0, 0, -1);
            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Test]
        public void LightEmittingFaceRangeAssignement_Constructor_ShouldThrowArgumentOutOfRangeException_WhenFaceIndexEndIsLowerThenFaceIndexBegin()
        {
            Action action = () => new LightEmittingFaceRangeAssignment(Guid.NewGuid().ToString(), 0, 2, 1);
            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Test]
        [TestCase(0)]
        [TestCase(-0.1)]
        public void Circle_ShouldThrowArgumentException_WhenDiameterIsZeroOrBelow(double diameter)
        {
            Action action = () => new Circle(diameter);

            action.Should().Throw<ArgumentException>();
        }

        [Test]
        [TestCase(1, 0)]
        [TestCase(1, 1)]
        public void AxisRotation_ShouldThrowArgumentException_WhenMaxIsLowerOrEqualMin(double min, double max)
        {
            Action action = () => new AxisRotation(min, max, 0.1);

            action.Should().Throw<ArgumentException>();
        }

        [Test]
        [TestCase(0)]
        [TestCase(-0.1)]
        public void AxisRotation_ShouldThrowArgumentException_WhenStepIsZeroOrBelow(double step)
        {
            Action action = () => new AxisRotation(0, 1, step);

            action.Should().Throw<ArgumentException>();
        }

        [Test]
        [TestCase(0)]
        [TestCase(-0.1)]
        public void Rectangle_ShouldThrowArgumentException_WhenSizeXIsZeroOrBelow(double sizeX)
        {
            Action action = () => new Rectangle(sizeX, 1);

            action.Should().Throw<ArgumentException>();
        }

        [Test]
        [TestCase(0)]
        [TestCase(-0.1)]
        public void Rectangle_ShouldThrowArgumentException_WhenSizeYIsZeroOrBelow(double sizeY)
        {
            Action action = () => new Rectangle(1, sizeY);

            action.Should().Throw<ArgumentException>();
        }

        [Test]
        [TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
        public void ModelPart_ShouldThrowArgumentException_WhenNameIsNullOrEmpty(string name)
        {
            Action action = () => new ModelFaceGroup(name, Enumerable.Empty<ModelFace>());

            action.Should().Throw<ArgumentException>();
        }

        [Test]
        public void ModelPart_ShouldThrowArgumentNullException_WhenFacesListIsNull()
        {
            Action action = () => new ModelFaceGroup(Guid.NewGuid().ToString(), null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void ModelFace_ShouldThrowArgumentNullException_WhenVecticesListIsNull()
        {
            Action action = () => new ModelFace(null, 0);

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void ModelData_ShouldThrowArgumentNullException_WhenVecticesListIsNull()
        {
            Action action = () => new ModelData(null, Enumerable.Empty<Vector3>(), Enumerable.Empty<Vector2>(), Enumerable.Empty<ModelFaceGroup>(), Enumerable.Empty<ModelMaterial>());

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void ModelData_ShouldThrowArgumentNullException_WhenNormalsListIsNull()
        {
            Action action = () => new ModelData(Enumerable.Empty<Vector3>(), null, Enumerable.Empty<Vector2>(), Enumerable.Empty<ModelFaceGroup>(), Enumerable.Empty<ModelMaterial>());

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void ModelData_ShouldThrowArgumentNullException_WhenTexCoordsListIsNull()
        {
            Action action = () => new ModelData(Enumerable.Empty<Vector3>(), Enumerable.Empty<Vector3>(), null, Enumerable.Empty<ModelFaceGroup>(), Enumerable.Empty<ModelMaterial>());

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void ModelData_ShouldThrowArgumentNullException_WhenPartListIsNull()
        {
            Action action = () => new ModelData(Enumerable.Empty<Vector3>(), Enumerable.Empty<Vector3>(), Enumerable.Empty<Vector2>(), null, Enumerable.Empty<ModelMaterial>());

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void ModelData_ShouldThrowArgumentNullException_WhenMaterialListIsNull()
        {
            Action action = () => new ModelData(Enumerable.Empty<Vector3>(), Enumerable.Empty<Vector3>(), Enumerable.Empty<Vector2>(), Enumerable.Empty<ModelFaceGroup>(), null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        [TestCaseSource(typeof(Setup), nameof(Setup.EmptyStringValues))]
        public void ModelMaterial_ShouldThrowArgumentException_WhenNameIsNull(string name)
        {
            Action action = () => new ModelMaterial(name, new Vector3(0, 0, 0), "testFileName");

            action.Should().Throw<ArgumentException>();
        }
    }
}
