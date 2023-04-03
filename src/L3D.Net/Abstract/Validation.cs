using System;

namespace L3D.Net.Abstract
{
    [Flags]
    public enum Validation : long
    {
        None = 0,
        IsZipPackage = 1 << 0,
        HasStructureXml = 1 << 1,
        IsXmlValid = 1 << 2,
        IsProductValid = 1 << 3,
        DoesReferencedObjectsExist = 1 << 4,
        Container = IsZipPackage | HasStructureXml | IsXmlValid | IsProductValid | DoesReferencedObjectsExist,
        All = Container
    }
}
