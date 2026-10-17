using ChaosFramework.Components;
using ChaosFramework.Core;
using ChaosFramework.Graphics.OpenGl.Instancing;
using ChaosFramework.Input;
using ChaosFramework.Input.InputEvents;
using ChaosFramework.Math.Vectors;
using ChaosFramework.Physics;
using ChaosFramework.Shapes.Convex;
using static ChaosFramework.Math.Constants;
using static ChaosFramework.Math.Exponentials;
using static ChaosFramework.Math.Trigonometry;

namespace LD60.World
{
    [Instancing.DefaultInstancer(1, "models/cube.gmdl", null)]
    public class Player : Component<FloatingIslands>, Instancable
    {
        const float VIEW_DISTANCE = 5.0f;
        const float VIEW_HEIGHT = 2.8f;
        const float VIEW_ANGLE_SENSITIVITY = 0.005f;
        const float MOVEMENT_CONTROL = 30f;
        const float MOVEMENT_SPEED = 6.66f;

        Physical physics;
        float viewAngle = 0;

        protected override void Create(CreateParameters cparams)
        {
            physics = new Physical(this, true);
            scene.physics.Add(physics);
            physics.shapes.Clear();

            using (Disposable x = new())
                physics.AddShape(new MeshShape([.. scene.game.meshes.Load("models/cube.gmdl", x).content.data.pos]));
            physics.shapes[0].bounce = 0;
            physics.state.contactFriction = 0;
            physics.state.position = scene.hostility.goal;
            physics.state.position.y += 1;
            physics.state.velocity.z = -2.5f;

            scene.instancers.GetOrCreateManager(typeof(Player)).instancedThings.Add(this);
        }

        public override void SetUpdateCalls()
        {
            base.SetUpdateCalls();
            scene.updateLayers[(int)UpdateLayers.SetCamera].Add(UpdateCamera);
            scene.updateLayers[(int)UpdateLayers.Move].Add(Move);
            scene.game.input.AddMouseMoveHandler(InputLayer.Game, PivotCamera);
        }

        bool PivotCamera(InputChangeEvent<Mouse.Position> changeEvent)
        {
            if (changeEvent.axis.direction == Mouse.Direction.X)
            {
                viewAngle += (changeEvent.data.newValue - changeEvent.data.oldValue)
                           * VIEW_ANGLE_SENSITIVITY
                           * Confix.instance.mouseSensitivity;
                return true;
            }
            return false;
        }

        void Move()
        {
            float side = 0, forward = 0;
            if (scene.game.input.IsPressed(Input.Inputs.WalkLeft))
                side -= 1.0f;
            if (scene.game.input.IsPressed(Input.Inputs.WalkRight))
                side += 1.0f;
            if (scene.game.input.IsPressed(Input.Inputs.WalkBack))
                forward -= 1.0f;
            if (scene.game.input.IsPressed(Input.Inputs.WalkForward))
                forward += 1.0f;

            Vector2f movement = new Vector2f(side, forward).GetNormalized();
            Vector3f target = (
                scene.view.GetLocalX() * movement.x
                + scene.view.GetLocalZ().x0z.GetNormalized() * movement.y
                ) * MOVEMENT_SPEED;
            Vector2f delta = (target.xz - physics.state.velocity.xz) * EaseIn(ftime * MOVEMENT_CONTROL);
            physics.state.velocity.x += delta.x;
            physics.state.velocity.z += delta.y;
            physics.state.Move(ftime);
        }

        void UpdateCamera()
        {
            float sin = Sin(viewAngle);
            float cos = Cos(viewAngle);
            Vector3f viewOffset = new(cos * VIEW_DISTANCE, -VIEW_HEIGHT, sin * VIEW_DISTANCE);
            scene.view.Update(
                physics.state.position - viewOffset,
                viewOffset,
                new Vector3f(0, 1, 0),
                scene.view.nearClip,
                scene.view.farClip,
                PI_QUART,
                scene.view.screenRatio
                );
        }

        void Instancable.GiveMeInstances(InstancingAttribute[] instancers)
            => instancers[0].informer.AddInstance(physics.state.GetTransform());

        bool Instancable.NeedsInstancedDraw()
            => true;

        protected override void DoDispose()
        {
            scene.instancers[typeof(Player)].instancedThings.Remove(this);
            base.DoDispose();
        }
    }
}
