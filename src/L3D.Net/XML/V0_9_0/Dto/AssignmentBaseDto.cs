using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace L3D.Net.XML.V0_9_0.Dto
{
    /// <remarks/>
    public abstract class AssignmentBaseDto
    {
        /// <remarks/>
        [XmlAttribute("groupIndex")]
        public int GroupIndex { get; set; }

        /// <remarks/>
        [XmlAttribute("lightEmittingPartName")]
        public string LightEmittingPartName { get; set; }

        // ReSharper disable UnusedMember.Global

        [ExcludeFromCodeCoverage]
        public bool ShouldSerializeGroupIndex() => GroupIndex != 0;

        // ReSharper restore UnusedMember.Global
    };
}