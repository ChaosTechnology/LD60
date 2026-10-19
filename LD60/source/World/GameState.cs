using ChaosFramework.Components;
using ChaosFramework.Graphics;
using ChaosFramework.Math.Vectors;
using ChaosFramework.Menu.OpenGl;
using ChaosFramework.Menu.OpenGl.Controls;

namespace LD60.World
{
    public class GameState
        : Component<FloatingIslands>
    {
        public Phase phase { get; private set; }
        float phaseTime, phaseTimer;

        Label phaseLabel;

        public float percentageLeft => phaseTimer / phaseTime;

        public GameState()
        {
            EnterPhase(Phase.Startup, 3);
        }

        protected override void Create(CreateParameters cparams)
        {
            phaseLabel = scene.overlay.panel.CreateControl<Label>(new Vector2f(-scene.view.screenRatio, 1), 0.05f, phase.ToString());
            phaseLabel.textAlign = Align.TopLeft;
        }

        public override void SetUpdateCalls()
        {
            base.SetUpdateCalls();
            scene.updateLayers[(int)UpdateLayers.SetCamera].Add(UpdatePhaseLabel);
            scene.updateLayers[(int)UpdateLayers.SetCamera].Add(UpdatePhase);
        }

        void UpdatePhaseLabel()
            => phaseLabel.text = $"{phase}\n{phaseTimer}";

        void UpdatePhase()
        {
            if ((phaseTimer -= ftime) < 0)
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

        void EnterPhase(Phase phase, float phaseTime)
        {
            this.phase = phase;
            this.phaseTime = phaseTimer = phaseTime;
        }

        void BeginStrategize()
        {
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
    }
}
