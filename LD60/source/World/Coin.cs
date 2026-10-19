using ChaosFramework.Components;
using ChaosFramework.Math;
using ChaosUtil.Primitives;
using static ChaosFramework.Math.Constants;
using static ChaosFramework.Math.Exponentials;

namespace LD60.World
{
    using Instancing;

    [PickingInstancer(666, MODEL, "models/coin/coin.mat")]
    public class Coin
        : StandardPickable
    {
        const string MODEL = "models/coin/coin.gmdl";

        public int value = 10;

        bool pickedup = false;

        float r = Random.instance.Rnd(PI_2);

        protected override void Create(CreateParameters cparams)
        {
            base.Create(cparams);
            physics.shapes.Add(scene.game.shapes.Load(MODEL, this));
        }

        public override void SetUpdateCalls()
        {
            base.SetUpdateCalls();
            scene.updateLayers[(int)UpdateLayers.Move].Add(Turn);
        }

        public void Collect()
        {
            scene.inventory.gold += value;
            pickedup = true;
        }

        void Turn()
        {
            r += ftime;
            physics.state.baseTransform = Matrix.RotationY(r);

            if (pickedup)
            {
                float ease = EaseIn(ftime);
                physics.state.velocity.y += ease * 100;
                physics.state.baseTransform = Matrix.Scaling(1 - ease * 20) * physics.state.baseTransform;
                if (physics.state.position.y > 1000)
                    Dispose();
            }
            else if (scene.overlay.picked == this)
                Collect();
        }
    }
}
