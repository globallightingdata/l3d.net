using L3D.Net.Abstract;
using System.Collections.Generic;
using System.IO;

namespace L3D.Net.Internal.Abstract;

public interface IXmlValidator
{
    public IEnumerable<ValidationHint> ValidateStream(Stream xmlStream);
}