using ChaosFramework.Components;
using ChaosFramework.Math.Vectors;
using static ChaosFramework.Math.Constants;
using static ChaosFramework.Math.Exponentials;
using static ChaosFramework.Math.Trigonometry;

namespace LD60.World
{
    [Instancing.DefaultInstancer(1, "models/cube.gmdl", null)]
    public class Player : Component<FloatingIslands>
    {
        const float VIEW_DISTANCE = 5.0f;
        const float VIEW_HEIGHT = 10f;
        const float MOVEMENT_SPEED = 6.66f;

        Vector3f camTarget = Vector3f.EMPTY;
        float viewAngle = 0.3f;

        protected override void Create(CreateParameters cparams)
        {
        }

        public override void SetUpdateCalls()
        {
            base.SetUpdateCalls();
            scene.updateLayers[(int)UpdateLayers.SetCamera].Add(UpdateCamera);
        }

        void UpdateCamera()
        {
            float side = 0, forward = 0;
            if (scene.game.input.IsPressed(Input.Inputs.PanLeft))
                side -= 1.0f;
            if (scene.game.input.IsPressed(Input.Inputs.PanRight))
                side += 1.0f;
            if (scene.game.input.IsPressed(Input.Inputs.PanBack))
                forward -= 1.0f;
            if (scene.game.input.IsPressed(Input.Inputs.PanForward))
                forward += 1.0f;
            Vector2f movement = new Vector2f(side, forward).GetNormalized();
            Vector3f target = (
                scene.view.GetLocalX() * movement.x
                + scene.view.GetLocalZ().x0z.GetNormalized() * movement.y
                );
            camTarget += target * EaseIn(ftime) * MOVEMENT_SPEED;

            if (scene.game.input.IsPressed(Input.Inputs.TurnCamCw))
                viewAngle -= ftime;
            if (scene.game.input.IsPressed(Input.Inputs.TurnCamCcw))
                viewAngle += ftime;

            float sin = Sin(viewAngle);
            float cos = Cos(viewAngle);
            Vector3f viewOffset = new(cos * VIEW_DISTANCE, -VIEW_HEIGHT, sin * VIEW_DISTANCE);
            scene.view.Update(
                camTarget - viewOffset,
                viewOffset,
                new Vector3f(0, 1, 0),
                scene.view.nearClip,
                scene.view.farClip,
                PI_QUART,
                scene.view.screenRatio
                );
        }
    }
}
