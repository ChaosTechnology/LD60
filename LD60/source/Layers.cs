namespace LD60
{
    public enum DrawLayers
    {
        StoreFrameBuffer,
        ResetInstancers,
        FillInstancers,
        BeginWorld,
        World,
        BeginPicking,
        Picking,
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
        ScreenShot,
        Menu,
        Tools,
        Game,
    }
}
