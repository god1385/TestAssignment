using UniRx;

public class GameEventsHandler
{
    public Subject<GameEventType> textEvent = new();
}

public enum GameEventType
{
    CubePlaced,
    CubeMissed,
    CubeThrown,
    HeightLimitReached
}
