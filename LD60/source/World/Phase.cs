namespace LD60.World
{
    public enum Phase
    {
        /// <summary> Not entered any phase yet. </summary>
        None,

        /// <summary> Camera drive on game start. </summary>
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
