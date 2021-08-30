namespace L3D.Net.Internal.Abstract
{
    interface ILightEmittingSurfaceHolder
    {
        void CreateLightEmittingSurfaces(string leoPartName, int faceIndexBegin, int faceIndexEnd, int groupIndex);

        void CreateLightEmittingSurface(string leoPartName, int faceIndex, int groupIndex);
    }
}
