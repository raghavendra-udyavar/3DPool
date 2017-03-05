
namespace ThreeDPool.EventHandlers
{
    public struct GameInputEvent : IGameEvent
    {
        // these states are named very generic here, so this avoids confusion when porting to other platform
        public enum States{
            Default,
            HorizontalAxisMovement,
            VerticalAxisMovement,
            Release,
            Paused
        }

        public float axisOffset;

        public States State;
    }
}
