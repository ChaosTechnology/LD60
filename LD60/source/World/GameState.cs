using System;
using System.Linq;
using ChaosFramework.Collections;
using ChaosFramework.Components;
using ChaosFramework.Graphics;
using ChaosFramework.Graphics.Colors;
using ChaosFramework.Graphics.OpenGl.Text;
using ChaosFramework.Graphics.Text;
using ChaosFramework.Math;
using ChaosFramework.Math.Vectors;
using ChaosFramework.Menu.OpenGl;
using ChaosFramework.Menu.OpenGl.Controls;
using LD60.World.Enemies;
using static ChaosFramework.Math.Clamping;
using static ChaosFramework.Math.Constants;
using static ChaosFramework.Math.Trigonometry;

namespace LD60.World
{
    public class GameState
        : Component<FloatingIslands>
    {
        const int MIN_GOBLINS = 1;
        const float BASE_GOBLIN_FAC = 0.5f;
        const float RANDOM_GOBLIN_FAC = 0.33f;
        static readonly Vector3f START_CAM_TARGET = new(-10, 50, 30);

        static readonly LayoutInfo LAYOUT = new(Align.Center, false, true);

        public int wave {get; private set;} = 0;

        public Phase phase { get; private set; } = Phase.None;
        float phaseTime, phaseTimer;

        Text phaseText;
        float t;

#if DEBUG
        Label phaseLabel;
#endif

        public float percentageLeft => phaseTimer / phaseTime;

        protected override void Create(CreateParameters cparams)
        {
            EnterPhase(Phase.Startup, 1);
            phaseText = new Text(scene.game.textRenderer, 256);
            phaseText.UpdateText(scene.game.defaultFont, string.Empty, LAYOUT);
            phaseText.color = new Rgba(0.4f, 0, 0, 0.6f);
            phaseText.transform =
                Matrix.Scaling(0.4f * scene.view.screenRatio * 0.3f) *
                Matrix.Translation(0, 0, 1) *
                scene.view.invView;

#if DEBUG
            phaseLabel = scene.overlay.panel.CreateControl<Label>(new Vector2f(-scene.view.screenRatio, 0.8f), 0.05f, phase.ToString());
            phaseLabel.textAlign = Align.TopLeft;
#endif
        }

        public override void SetDrawCalls()
        {
            base.SetDrawCalls();
            scene.drawLayers[(int)DrawLayers.FillInstancers].Add(DrawPhaseText);
        }

        public override void SetUpdateCalls()
        {
            base.SetUpdateCalls();
            scene.updateLayers[(int)UpdateLayers.ChangePhase].Add(UpdatePhase);

#if DEBUG
            scene.updateLayers[(int)UpdateLayers.SetCamera].Add(UpdatePhaseLabel);
#endif
        }

#if DEBUG
        void UpdatePhaseLabel()
            => phaseLabel.text = $"{phase}\n{phaseTimer}";
#endif

        void UpdatePhase()
        {
            t += ftime;
            if ((phaseTimer -= ftime) < 0)
            {
                SwitchState();
                return;
            }

            if (phase == Phase.Horde)
                RateGoblinSlayer();

            if (phase == Phase.Startup)
            {
                float viewAngle = (1 - percentageLeft) * CameraController.START_VIEW_ANGLE;
                float sin = Sin(viewAngle);
                float cos = Cos(viewAngle);

                Vector3f camTarget = CameraController.START_CAM_TARGET
                                   + (START_CAM_TARGET - CameraController.START_CAM_TARGET) * percentageLeft;

                Vector3f viewOffset = new(
                    cos * CameraController.VIEW_DISTANCE,
                    -CameraController.VIEW_HEIGHT,
                    sin * CameraController.VIEW_DISTANCE
                    );

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

        void RateGoblinSlayer()
        {
            foreach(Door d in scene.EnumerateChildren<Door>(true))
                if (d.goblinPool > 0)
                    return;

            foreach(Goblin d in scene.EnumerateChildren<Goblin>(true))
                if (d.physics.state.position.y > FloatingIslands.SCREAM_PLANE)
                    return;

            SwitchState();
        }

        void SwitchState()
        {
            switch (phase)
            {
                case Phase.Startup:
                    BeginStrategize();
                    break;

                case Phase.Strategize:
                    EnterPhase(Phase.Countdown, 3);
                    break;

                case Phase.Countdown:
                    EnterPhase(Phase.Horde, float.NaN);
                    break;

                case Phase.Horde:
                    EnterPhase(Phase.Victory, 3);
                    break;

                case Phase.Victory:
                    BeginStrategize();
                    break;
            }
        }

        void NextWave()
        {
            ++wave;
            float somethingInBetween = wave * 0.777f + 0.5f;
            int numExtraGoblins = MIN_GOBLINS + (int)(somethingInBetween * somethingInBetween * RANDOM_GOBLIN_FAC);
            int numBaseGoblins = MIN_GOBLINS + (int)(somethingInBetween * BASE_GOBLIN_FAC);
            Door[] doors = scene.EnumerateChildren<Door>(true).ToArray();
            foreach (Door door in doors)
                door.goblinPool = numBaseGoblins;

            for (int i = 0; i < numExtraGoblins; ++i)
                doors.RandomElement().goblinPool++;
        }

        void EnterPhase(Phase phase, float phaseTime)
        {
            if (this.phase == Phase.Startup)
                scene.AddComponent<CameraController>();

            t = 0;
            this.phase = phase;
            this.phaseTime = phaseTimer = phaseTime;
            if (!(scene.overlay.currentTool?.Allowed() ?? false))
                scene.overlay.SelectDefaultTool();
        }

        void BeginStrategize()
        {
            NextWave();
            EnterPhase(Phase.Strategize, float.NaN);

            Vector2f pos = new Vector2f(scene.view.screenRatio, -1) * 0.9f;
            Vector2f sz = new Vector2f(scene.view.screenRatio, 1) * 0.1f;
            Button startGame = scene.overlay.CreateControl<Button>(pos, sz, "GO");

            void StartGame(Control _)
            {
                startGame.Dispose();
                phaseTimer = 0;
            }

            startGame.mouseDownLeft = StartGame;
        }

        void DrawPhaseText()
        {
            phaseText.transform =
                Matrix.Scaling(0.9f * scene.view.screenRatio * 0.3f) *
                Matrix.Translation(0, 0, 1) *
                scene.view.invView;

            string text =  phase switch
            {
                Phase.Startup => string.Empty,
                Phase.Strategize => "Prepare!",
                Phase.Horde => "Wave " + wave,
                Phase.Countdown => Math.Ceiling(phaseTimer).ToString(),
                Phase.Victory => "Victory",
                _ => string.Empty,
            };

            phaseText.color = new(
                phaseText.color.rgb,
                (phase == Phase.Strategize || phase == Phase.Horde)
                    ? Cos(Min(PI, t) / 2)
                    : 1
                );

            phaseText.UpdateText(scene.game.defaultFont, text, LAYOUT);

            scene.hudTexts.Add(phaseText);
        }
    }
}
