using KsubakaPool.EventHandlers;
using KsubakaPool.Managers;

namespace KsubakaPool
{
    public class Player
    {
        public string Name { private set; get; }
        public int Score { private set; get; }

        public bool HasStrikedBall { private set; get; }

        private bool _isPlaying;

        public Player(string name)
        {
            // initializing fields
            Name = name;
            Score = 0;

            EventManager.Subscribe(typeof(CueBallActionEvent).Name, OnCueBallStriked);
        }

        private void OnCueBallStriked(object sender, IGameEvent gameEvent)
        {
            CueBallActionEvent actionEvent = (CueBallActionEvent)gameEvent;
            if (_isPlaying && actionEvent.State == CueBallActionEvent.States.Striked)
                HasStrikedBall = true;
        }

        public void SetPlayingState(bool isPlaying)
        {
            _isPlaying = isPlaying;
            HasStrikedBall = false;
        }

        public void CalculateScore(int score)
        {
            Score += score;

            // score will never fo negative
            if (Score < 0)
                Score = 0;

            EventManager.Notify(typeof(ScoreUpdateEvent).Name, this, new ScoreUpdateEvent());
        }

        public void ResetScore()
        {
            Score = 0;
        }
    }
}
