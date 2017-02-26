
namespace KsubakaPool.EventHandlers
{
    public class GameStateEvent : IGameEvent
    {
        public enum State
        {
            Practise,
            Play,
            Complete
        }

        public State GameState;
        public string CurrPlayer;
    }
}
