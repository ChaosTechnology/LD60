namespace LD60.World
{
    public enum Phase
    {
        /// <summary> TODO: When starting the game, initially do a camera drive or something. </summary>
        Startup,

        /// <summary> Freely purchase or place units without a time limit. </summary>
        Strategize,

        /// <summary> Start the wave, open the doors. </summary>
        Countdown,

        /// <summary> Battle goblins. </summary>
        Horde,

        /// <summary> Wave is over, close the doors. </summary>
        Victory
    }
}
