namespace LD60.Instancing
{
    public class AnimatedPickableInstancer
        : AnimatedInstancer
    {
        public AnimatedPickableInstancer() { }
        public AnimatedPickableInstancer(
            int maxInstances,
            string meshSource,
            string materialSource,
            string overrideEffect = "shaders/picking_skinned_instanced.fx",
            params string[] customRegisters)
            : base(
                maxInstances,
                meshSource,
                materialSource,
                overrideEffect,
                ["INSTANCE_ID", ..customRegisters]
            )
        { }

        public override void SetDrawCalls()
        {
            base.SetDrawCalls();
            host.scene.drawLayers[(int)DrawLayers.Picking].Add(DrawPicking);
        }

        void DrawPicking()
            => DrawInstances(host.view, "Picking");
    }
}
