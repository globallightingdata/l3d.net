using System;

namespace L3D.Net.Abstract;

[Flags]
public enum Validation
{
    None = 0,
    IsZipPackage = 1 << 0,
    HasStructureXml = 1 << 1,
    IsXmlValid = 1 << 2,
    IsProductValid = 1 << 3,
    DoesReferencedObjectsExist = 1 << 4,
    AreAllFileDefinitionsUsed = 1 << 5,
    HasAllMaterials = 1 << 6,
    HasAllTextures = 1 << 7,
    MandatoryField = 1 << 8,
    MinMaxRestriction = 1 << 9,
    NameConvention = 1 << 10,
    GeometryReferences = 1 << 11,
    FaceReferences = 1 << 12,
    NameReferences = 1 << 13,
    HasLightEmittingPart = 1 << 14,
    LuminaireContentValid = MandatoryField | MinMaxRestriction | NameConvention | GeometryReferences | FaceReferences | NameReferences | HasLightEmittingPart,
    Container = IsZipPackage | HasStructureXml | IsXmlValid | IsProductValid | DoesReferencedObjectsExist,
    All = Container | AreAllFileDefinitionsUsed | HasAllMaterials | LuminaireContentValid | HasAllTextures
}