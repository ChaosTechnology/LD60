using System.Linq;
using ChaosFramework.Collections;
using ChaosFramework.Components;
using ChaosFramework.Graphics;
using ChaosFramework.Math.Vectors;
using ChaosFramework.Menu.OpenGl;
using ChaosFramework.Menu.OpenGl.Controls;
using LD60.World.Enemies;

namespace LD60.World
{
    public class GameState
        : Component<FloatingIslands>
    {
        const int MIN_GOBLINS = 1;
        const float BASE_GOBLIN_FAC = 0.5f;
        const float RANDOM_GOBLIN_FAC = 0.33f;
        public int wave {get; private set;} = 0;

        public Phase phase { get; private set; }
        float phaseTime, phaseTimer;

#if DEBUG
        Label phaseLabel;
#endif

        public float percentageLeft => phaseTimer / phaseTime;

        protected override void Create(CreateParameters cparams)
        {
            EnterPhase(Phase.Startup, 1);
#if DEBUG
            phaseLabel = scene.overlay.panel.CreateControl<Label>(new Vector2f(-scene.view.screenRatio, 0.8f), 0.05f, phase.ToString());
            phaseLabel.textAlign = Align.TopLeft;
#endif
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
            if ((phaseTimer -= ftime) < 0)
            {
                SwitchState();
                return;
            }

            if (phase == Phase.Horde)
                RateGoblinSlayer();
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
    }
}
