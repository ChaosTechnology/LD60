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
        BeginWorldAfterPicking,
        WorldAfterPicking,
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
        ChangePhase,
        PrepareUpdate,
        Input,
        Move,
        Collision,
        PostCollision,
        CreateParticles,
        UpdateParticles,
        ClearAnimations,
        UpdateAnimations,
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
