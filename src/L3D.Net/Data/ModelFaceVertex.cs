namespace L3D.Net.Data
{
    public class ModelFaceVertex
    {
        public ModelFaceVertex(int vertexIndex, int normalIndex, int textureCoordinateIndex)
        {
            VertexIndex = vertexIndex;
            NormalIndex = normalIndex;
            TextureCoordinateIndex = textureCoordinateIndex;
        }

        public int VertexIndex { get; }
        public int NormalIndex { get; }
        public int TextureCoordinateIndex { get; }
    }
}