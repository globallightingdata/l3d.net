using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using FluentAssertions;
using FluentAssertions.Execution;
using L3D.Net.Data;
using L3D.Net.Geometry;
using L3D.Net.Internal;
using L3D.Net.Internal.Abstract;
using L3D.Net.XML.V0_11_0;
using NUnit.Framework;

namespace L3D.Net.Tests.XML.V0_11_0;

[TestFixture]
public class LuminaireResolverTests
{
    private LuminaireResolver _resolver = null!;

    [SetUp]
    public void Init()
    {
        _resolver = new LuminaireResolver(ObjParser.Instance, FileHandler.Instance);
    }

    [Test]
    [TestCase(GeometricUnits.m, 1f)]
    [TestCase(GeometricUnits.dm, 0.1f)]
    [TestCase(GeometricUnits.cm, 0.01f)]
    [TestCase(GeometricUnits.mm, 0.001f)]
    [TestCase(GeometricUnits.yard, 0.9144f)]
    [TestCase(GeometricUnits.foot, 0.3048f)]
    [TestCase(GeometricUnits.inch, 0.0254f)]
    public void GetScale_ShouldReturnExpected(GeometricUnits unit, float expected)
    {
        var result = LuminaireResolver.GetScale(unit);
        result.Should().Be(expected);
    }

    [Test]
    [TestCase(1f)]
    [TestCase(0.1f)]
    [TestCase(0.01f)]
    [TestCase(0.001f)]
    [TestCase(0.9144f)]
    [TestCase(0.3048f)]
    [TestCase(0.0254f)]
    public void ScaleModel_ShouldReturnExpected(float scale)
    {
        var model = new ObjModel3D
        {
            Data = new ModelData
            {
                Vertices = [Vector3.Zero, Vector3.One, Vector3.Negate(Vector3.One), Vector3.One * 2, Vector3.Negate(Vector3.One * 2)]
            }
        };
        using var cache = new ContainerCache();
        LuminaireResolver.ScaleModel(model, scale);
        model.Data.Vertices.Should().BeEquivalentTo([
            Vector3.Zero * scale,
            Vector3.One * scale,
            Vector3.Negate(Vector3.One) * scale,
            Vector3.One * 2 * scale,
            Vector3.Negate(Vector3.One * 2 * scale)
        ]);
    }

    #region ResolveModelMaterials

    [Test]
    public void ResolveModelMaterials_ShouldSetEmptyTexture_WhenGeometryIdNotExists()
    {
        var model = new ObjModel3D
        {
            Data = new ModelData
            {
                Materials = [new ModelMaterial {TextureName = "tex1"}, new ModelMaterial {TextureName = "tex2"}]
            }
        };
        using var cache = new ContainerCache();
        _resolver.ResolveModelMaterials(model, "geom1", cache);
        using (new AssertionScope())
        {
            model.Data.Materials[0].TextureBytes.Should().BeEmpty();
            model.Data.Materials[1].TextureBytes.Should().BeEmpty();
        }
    }

    [Test]
    public void ResolveModelMaterials_ShouldSetEmptyTexture_WhenGeometryIdExistsButNotTheTexture()
    {
        var model = new ObjModel3D
        {
            Data = new ModelData
            {
                Materials = [new ModelMaterial {TextureName = "tex1"}, new ModelMaterial {TextureName = "tex2"}]
            }
        };
        using var cache = new ContainerCache();
        cache.Geometries["geom1"] = new Dictionary<string, Stream>();
        _resolver.ResolveModelMaterials(model, "geom1", cache);
        using (new AssertionScope())
        {
            model.Data.Materials[0].TextureBytes.Should().BeEmpty();
            model.Data.Materials[1].TextureBytes.Should().BeEmpty();
        }
    }

    [Test]
    public void ResolveModelMaterials_ShouldResolveTextureBytes_WhenTexturesPartiallyExists()
    {
        var model = new ObjModel3D
        {
            Data = new ModelData
            {
                Materials = [new ModelMaterial {TextureName = "tex1"}, new ModelMaterial {TextureName = "tex2"}]
            }
        };
        using var cache = new ContainerCache();
        cache.Geometries["geom1"] = new Dictionary<string, Stream>
        {
            ["tex2"] = new MemoryStream([0x02])
        };
        _resolver.ResolveModelMaterials(model, "geom1", cache);
        using (new AssertionScope())
        {
            model.Data.Materials[0].TextureBytes.Should().BeEmpty();
            model.Data.Materials[1].TextureBytes.Should().BeEquivalentTo([0x02]);
        }
    }

    [Test]
    public void ResolveModelMaterials_ShouldResolveAllTextureBytes_WhenTexturesExists()
    {
        var model = new ObjModel3D
        {
            Data = new ModelData
            {
                Materials = [new ModelMaterial {TextureName = "tex1"}, new ModelMaterial {TextureName = "tex2"}]
            }
        };
        using var cache = new ContainerCache();
        cache.Geometries["geom1"] = new Dictionary<string, Stream>
        {
            ["tex1"] = new MemoryStream([0x01]),
            ["tex2"] = new MemoryStream([0x02])
        };
        _resolver.ResolveModelMaterials(model, "geom1", cache);
        using (new AssertionScope())
        {
            model.Data.Materials[0].TextureBytes.Should().BeEquivalentTo([0x01]);
            model.Data.Materials[1].TextureBytes.Should().BeEquivalentTo([0x02]);
        }
    }

    #endregion

    #region Resolve

    [Test]
    public void Resolve_ShouldReturnNull_WhenNullGiven()
    {
        using var cache = new ContainerCache();
        var result = _resolver.Resolve(null, cache);
        result.Should().BeNull();
    }

    [Test]
    public void Resolve_ShouldSetCorrectGeometryDefinition_WhenPartsReferencedToTheSameObject_ButModelCannotBeLoaded()
    {
        using var cache = new ContainerCache();
        var luminaire = new Luminaire();
        luminaire.GeometryDefinitions.Add(new GeometryFileDefinition {FileName = "geom1.obj", GeometryId = "geom1", Units = GeometricUnits.m});
        luminaire.Parts.Add(new GeometryPart {GeometryReference = new GeometryFileDefinition {GeometryId = "geom1"}});
        luminaire.Parts.Add(new GeometryPart {GeometryReference = new GeometryFileDefinition {GeometryId = "geom1"}});
        var result = _resolver.Resolve(luminaire, cache);
        using (new AssertionScope())
        {
            result!.GeometryDefinitions[0].Should().BeSameAs(luminaire.Parts[0].GeometryReference);
            result.GeometryDefinitions[0].Should().BeSameAs(luminaire.Parts[1].GeometryReference);
            luminaire.Parts[0].GeometryReference.Should().BeSameAs(luminaire.Parts[1].GeometryReference);
        }
    }

    [Test]
    public void Resolve_ShouldLoadAndScaleModel()
    {
        using var cache = Setup.TestDataDirectory.ToCache();
        cache.Geometries["geom1"] = new Dictionary<string, Stream>
        {
            ["geom1.obj"] = new MemoryStream(),
            ["sconce_01.mtl"] = new MemoryStream()
        };
        cache.Geometries["obj"]["sconce_01.obj"].CopyTo(cache.Geometries["geom1"]["geom1.obj"]);
        cache.Geometries["obj"]["sconce_01.mtl"].CopyTo(cache.Geometries["geom1"]["sconce_01.mtl"]);
        var luminaire = new Luminaire();
        var originalModel = ObjParser.Instance.Parse("sconce_01.obj", cache.Geometries["obj"]);
        luminaire.GeometryDefinitions.Add(new GeometryFileDefinition {FileName = "geom1.obj", GeometryId = "geom1", Units = GeometricUnits.mm});
        luminaire.Parts.Add(new GeometryPart {GeometryReference = new GeometryFileDefinition {GeometryId = "geom1"}});
        luminaire.Parts.Add(new GeometryPart {GeometryReference = new GeometryFileDefinition {GeometryId = "geom1"}});
        var result = _resolver.Resolve(luminaire, cache);
        using (new AssertionScope())
        {
            result!.GeometryDefinitions[0].Model.Should().BeSameAs(luminaire.Parts[0].GeometryReference.Model);
            result.GeometryDefinitions[0].Model.Should().BeSameAs(luminaire.Parts[1].GeometryReference.Model);
            luminaire.Parts[0].GeometryReference.Model.Should().BeSameAs(luminaire.Parts[1].GeometryReference.Model);
            result.GeometryDefinitions[0].Model!.Data!.Vertices.Should().BeEquivalentTo(originalModel!.Data!.Vertices.Select(v => v * 0.001f));
        }
    }

    #endregion
}