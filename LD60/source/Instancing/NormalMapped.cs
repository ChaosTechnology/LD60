using ChaosFramework.Graphics.OpenGl.AssetContainers;

namespace LD60.Instancing
{
    public interface NormalMapped
    {
        public ShaderContainer.Entry shader { get; }
        public MaterialContainer.Entry material { get; }
        public MeshContainer.Entry mesh { get; }
    }
}
