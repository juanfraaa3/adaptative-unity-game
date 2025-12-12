using Unity.FPS.Game;

public class WaveCompletedEvent : GameEvent
{
    public int WaveIndex;   // Índice de la wave que terminó

    public WaveCompletedEvent(int waveIndex)
    {
        WaveIndex = waveIndex;
    }
}
