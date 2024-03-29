﻿namespace L3D.Net.Abstract;

public static class ErrorMessages
{
    public const string InvalidZip = "invalid_zip";

    public const string StructureXmlMissing = "structure_xml_missing";

    public const string StructureXmlNotReadable = "structure_xml_not_readable";

    public const string StructureXmlVersionMissing = "structure_xml_version_missing";

    public const string StructureXmlVersionNotReadable = "structure_xml_version_not_readable";

    public const string StructureXsdVersionNotKnown = "structure_xsd_version_not_known";

    public const string StructureXmlContentError = "structure_xml_content_error";

    public const string StructureXmlContentWarning = "structure_xml_content_warning";

    public const string StructureXmlContentValidationFailed = "structure_xml_content_validation_failed";

    // ReSharper disable once InconsistentNaming
    public const string NotAL3D = "not_a_l3d";

    public const string MissingGeometryReference = "missing_geometry_reference";

    public const string UnusedFile = "unused_file";

    public const string MissingMaterial = "missing_material_reference";

    public const string MissingTexture = "missing_texture_reference";

    public const string InvalidL3DContent = "invalid_l3d_content";
}