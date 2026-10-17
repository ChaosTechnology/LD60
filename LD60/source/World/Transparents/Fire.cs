using ChaosFramework.Components;
using ChaosFramework.Graphics.Colors;
using ChaosFramework.Graphics.OpenGl;
using ChaosFramework.Graphics.OpenGl.AssetContainers;
using ChaosFramework.Graphics.OpenGl.Particles;
using ChaosFramework.Math;
using ChaosFramework.Math.Vectors;
using ChaosUtil.Primitives;

namespace LD60.World.Transparents
{
    public partial class ParticleFire : TransparentParticleSystem
    {
        const int MAX_PARTICLES = 100000;

        protected override void Create(CreateParameters cparams)
            => base.Create(new Params(
                ((Game)scene.game).shaders.Load("shaders/fire.fx", this),
                ((Game)scene.game).textures.Load("textures/particles/fire.png", this),
                1,
                ((WorldScene)scene).view,
                MAX_PARTICLES,
                (int)UpdateLayers.UpdateParticles, new string[] { "PARTICLE_TEXOFFSET", "PARTICLE_COLOR" }
                ));

        public override void DrawTransparent(TransparencyRenderer renderer)
        {
            shader.SetValue("tex", maskTexture);
            Draw("Instanced");
        }

        public void Explode(Vector3f pos, float diff, Rgba color, int numParticles = 100)
        {
            for (int i = 0; i < numParticles; i++)
                particles.Add(new FireParticle(
                    ftime,
                    null,
                    pos,
                    1,
                    0.1f * diff,
                    0.1f * diff,
                    Random.instance.RndVector3(Random.instance.Rnd(diff)),
                    color
                    ));
        }
    }
}
