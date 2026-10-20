using ChaosFramework.Components;
using ChaosFramework.Graphics.OpenGl;
using ChaosFramework.Graphics.OpenGl.AssetContainers;
using ChaosFramework.Math;
using OpenTK.Graphics.OpenGL;

namespace LD60.World
{
    public class Skybox : Component<DeferredRenderingScene>
    {
        CubeTexture sky;
        ShaderContainer.Entry shader;

        protected override void Create(CreateParameters cparams)
        {
            sky = CubeTexture.FromStreams(scene.game.graphics.dispatcher,
                scene.game.archive.OpenRead("textures/sky/left.png"),
                scene.game.archive.OpenRead("textures/sky/right.png"),
                scene.game.archive.OpenRead("textures/sky/bottom.png"),
                scene.game.archive.OpenRead("textures/sky/top.png"),
                scene.game.archive.OpenRead("textures/sky/back.png"),
                scene.game.archive.OpenRead("textures/sky/front.png"));
            shader = scene.game.shaders.Load("shaders/sky.fx", this);
        }

        public override void SetDrawCalls()
        {
            base.SetDrawCalls();
            scene.drawLayers[(int)DrawLayers.Sky].Add(DrawSky);
        }

        void DrawSky()
        {
            shader.content.SetValue("tex", sky);
            scene.view.SetValues(shader, Matrix.Translation(scene.view.Position));
            GL.BindVertexArray(scene.game.graphics.emptyVAO);
            Graphics.ThrowErrors();
            shader.BeginPass("Sky");
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 14);
            Graphics.ThrowErrors();
            shader.EndPass();
        }

        protected override void DoDispose()
        {
            base.DoDispose();
            sky.Dispose();
        }
    }
}
