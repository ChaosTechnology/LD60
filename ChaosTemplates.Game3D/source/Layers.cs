namespace ChaosTemplates.Game3D
{
    public enum DrawLayers
    {
        StoreFrameBuffer,
        ResetInstancers,
        FillInstancers,
        BeginWorld,
        World,
        BeginMaterial,
        Material,
        Sky,
        RenderDeferredShader,
        Transparents,
        AntiAliasing,
        ResetFramebuffer,
        Postprocessing,
        HUD,
    }

    public enum UpdateLayers
    {
        PrepareUpdate,
        Input,
        Move,
        Collision,
        PostCollision,
        CreateParticles,
        UpdateParticles,
        SetCamera,
    }

    public enum InputLayer
    {
        Game,
        HUD,
        Menu,
        ScreenShot,
    }
}
