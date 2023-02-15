using System;

namespace L3D.Net.Internal.Abstract;

interface ILightEmittingSurfaceOptions
{
    ILightEmittingSurfaceOptions WithLightEmittingPart(string leoPartName);
    ILightEmittingSurfaceOptions WithSurface(int faceIndex, int groupIndex);
    ILightEmittingSurfaceOptions WithSurfaceRange(int faceIndexBegin, int faceIndexEnd, int groupIndex);
}
    
interface ILightEmittingSurfaceHolder
{
    void WithLightEmittingSurface(string lesPartName, Action<ILightEmittingSurfaceOptions> options);
}