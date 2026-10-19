using ChaosUtil.Serialization.Text;

namespace LD60.Settings
{
    public class ConfixClass
    {
        [Gui(Cat.GENERAL, null, 30u, 666u)]
        [Ini.Field(Cat.GENERAL, "The maximum framerate for the game.")]
        public uint frameRate = 60u;

        [Gui(Cat.GENERAL, null, 0, 1, 2)]
        [Ini.Field(Cat.GENERAL, "The brightness of the default light that illuminates your game. 0 is off, 1 is full")]
        public float brightness = 1f;

        [Gui(Cat.GRAPHICS, null, 0.25f, 1, 2)]
        [Ini.Field(Cat.GRAPHICS, "Determines the transparency renderer resolution relative to solid world resolution.")]
        public float transparencyShaderScale = 1;

        [Gui(Cat.GRAPHICS, null, 5, 50)]
        [Ini.Field(Cat.GRAPHICS, "Determines the maximum number of correctly ordered transparency layers.")]
        public uint numTransparencyLayers = 20;

        [Gui(Cat.GRAPHICS, null)]
        [Ini.Field(Cat.GRAPHICS,
            "Determines whether the font renderer tries to use MultiDrawIndirect feature of your graphics card. "
            + "If your text appears broken (or not at all), try disabling this feature."
            )]
        public bool tryToUseMultiDrawIndirect = false;

        [Gui(Cat.SOUND, null, 0, 1, 3)]
        [Ini.Field(Cat.SOUND, "The master volume of the game. SFX and Music scale with this one.")]
        public float masterVolume = 1;

        [Gui(Cat.SOUND, null, 0, 1, 3)]
        [Ini.Field(Cat.SOUND, "The sfx volume of the game. Also depends on master volume.")]
        public float sfxVolume = 1;

        [GuiDisabled(nameof(musicMuted), true)]
        [Gui(Cat.SOUND, null, 0, 1, 3)]
        [Ini.Field(Cat.SOUND, "The music volume of the game. Also depends on master volume.")]
        public float musicVolume = 0.5f;

        [Gui(Cat.SOUND, null)]
        [Ini.Field(Cat.SOUND, "Determines whether the music is muted.")]
        public bool musicMuted = false;

        [Gui(Cat.INPUT, null, 0.001f, 10.0f, 3)]
        [Ini.Field(Cat.INPUT, "A mouse senstivity multiplier for camera movement.")]
        public float mouseSensitivity = 1.0f;
    }
}
