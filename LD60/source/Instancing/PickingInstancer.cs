using ChaosUtil.Platform.Paths;

namespace LD60.Instancing
{
    class PickingInstancer
        : DefaultInstancer
    {
        public PickingInstancer() { }
        public PickingInstancer(
            int maxInstances,
            string meshSource,
            string materialSource,
            string overrideEffect = "shaders/picking_instanced.fx",
            params string[] customRegisters
            ) : base(
                maxInstances,
                Normalization.NormalizeRelative(meshSource),
                Normalization.NormalizeRelative(materialSource),
                overrideEffect == null ? null : Normalization.NormalizeRelative(overrideEffect),
                ["INSTANCE_ID", ..customRegisters]
                )
        { }

        public override void SetDrawCalls()
        {
            base.SetDrawCalls();
            host.scene.drawLayers[(int)DrawLayers.Picking].Add(DrawPicking);
        }

        void DrawPicking() => DrawInstances(host.view, "Picking");
    }
}
