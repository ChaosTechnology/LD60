using ChaosFramework.Components;
using ChaosFramework.Math.Vectors;
using static ChaosFramework.Math.Constants;
using static ChaosFramework.Math.Exponentials;
using static ChaosFramework.Math.Trigonometry;

namespace LD60.World
{
    [Instancing.DefaultInstancer(1, "models/cube.gmdl", null)]
    public class CameraController : Component<FloatingIslands>
    {
        public const float VIEW_DISTANCE = 30.0f;
        public const float VIEW_HEIGHT = 20f;
        public const float START_VIEW_ANGLE = -1.3405931f;
        const float MOVEMENT_SPEED = 66.6f * 0.5f;

        public static readonly Vector3f START_CAM_TARGET = new(-2.676854f, 0f, 16.290289f);


        Vector3f camTarget = START_CAM_TARGET;
        float viewAngle = START_VIEW_ANGLE;

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
