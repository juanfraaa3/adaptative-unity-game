using Unity.FPS.Game;

public class WaveStartedEvent : GameEvent
{
    public int WaveIndex;

    public WaveStartedEvent(int waveIndex)
    {
        WaveIndex = waveIndex;
    }
}
