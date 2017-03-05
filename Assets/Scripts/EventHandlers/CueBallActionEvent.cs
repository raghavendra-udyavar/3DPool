
namespace ThreeDPool.EventHandlers
{
    public struct CueBallActionEvent : IGameEvent
    {
        public enum States
        {
            Default,
            Placing,
            Striked,
            InMotion,
            Stationary,
        }

        public States State;
    }
}
