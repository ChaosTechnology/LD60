using ChaosUtil.Primitives;
using System.Text.RegularExpressions;
using ChaosFramework.Collections;
using ChaosFramework.IO.Streams;
using ChaosFramework.Sound.OpenAL;
using System.Linq;

namespace LD60.World.Enemies
{
    public class Sounds
    {
        public enum Kind
        {
            GoblinAmbient,
            GoblinDamage,
            GoblinFalling,
        }
        static readonly string[] KIND_PATHS = ["goblin/ambient", "goblin/damage", "goblin/falling"];

        readonly SoundPool soundPool;

        string[][] files;
        public Sounds(Game game)
        {
            soundPool = game.soundPool;
            files = new string[System.Enum.GetValues<Kind>().Length][];

            for (int i = 0; i < files.Length; i++)
                files[i] = game.archive.EnumerateKeys(new Regex($"sounds/{KIND_PATHS[i]}/[0-9].wav")).ToArray();
        }

        public void Play(Kind kind)
        {
            float randomChoice = Random.instance.Rnd(0.8f, 1.0f);
            float kindChoice = kind switch
            {
                Kind.GoblinAmbient => 0.5f,
                Kind.GoblinDamage => 1.0f,
                Kind.GoblinFalling => 0.7f,
                _ => 1.0f,
            };
            float volume = Confix.instance.sfxVolume * Confix.instance.masterVolume * randomChoice * kindChoice;
            soundPool.PlaySound(files[(int)kind].RandomElement(), volume, false);
        }
    }
}
