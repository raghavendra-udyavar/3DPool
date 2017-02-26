using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KsubakaPool.EventHandlers;
using KsubakaPool.Controllers;
using KsubakaPool.UIControllers;

namespace KsubakaPool.Managers
{
    public class GameManager : Singleton<GameManager>
    {
        public enum GameType
        {
            JustCue = 1,
            ThreeBall = 3,
            SixBall = 6,
            SevenBall,
        }

        public enum GameState
        {
            Practise = 1,
            GetSet,
            Play,
            Pause,
            Complete
        }

        // update the players you would be playing this round
        [SerializeField]
        private string[] _playerNames;

        [SerializeField]
        private GameType _gameType;

        [SerializeField]
        private Transform _rackTransform;

        [SerializeField]
        private CueBallController _cueBall;

        [SerializeField]
        private GameUIScreen _gameUIScreen;

        // maintaining a queue here so that we dont have to worry about maintain a separate field for the player turn
        private Queue<Player> _players = new Queue<Player>();

        private List<CueBallController> _ballsPocketed;
        private List<CueBallController> _ballsHitOut;
        private GameState _currGameState;
        private GameState _prevGameState;
        private bool _ballsInstantiated;

        public int NumOfBallsStriked;

        public GameState CurrGameState { get { return _currGameState; } }
        public GameState PrevGameState { get { return _prevGameState;  } }

        public Queue<Player> Players { get { return _players;  } }

        public string[] Winners;

        public int NumOfTimesPlayed { private set; get; }

        protected override void Start()
        {
            base.Start();

            ChangeGameState(GameState.Practise);
            NumOfBallsStriked = 0;

            if (_playerNames != null)
            {
                foreach (var playerName in _playerNames)
                {
                    var player = new Player(playerName);

                    _players.Enqueue(player);
                }
            }

            // declare the ballspotted and ballshit out array
            // consider all the balls are either potted or hitout,
            // lets fix the array size based on game type + cueball
            int arraySize = (int)_gameType + 1;
            _ballsPocketed = new List<CueBallController>(arraySize);
            _ballsHitOut = new List<CueBallController>(arraySize);

            // create player uis
            _gameUIScreen.CreatePlayerUI();

            // the first player in the queue will be playing first

        }

        public void ChangeGameState(GameState newGameState)
        {
            // making sure that the prev game state is actually the prev game state
            if(newGameState != _currGameState)
            {
                _prevGameState = _currGameState;
                _currGameState = newGameState;
            }
        }

        public void OnGetSet()
        {
            ChangeGameState(GameState.GetSet);
        }

        public void OnPlay()
        {
            // make sure we start with clear list
            _ballsHitOut.Clear();
            _ballsPocketed.Clear();

            NumOfBallsStriked = 0;

            NumOfTimesPlayed++;

            foreach (var player in _players)
                player.ResetScore();

            ChangeGameState(GameState.Play);

            // place the cue ball in position
            _cueBall.PlaceBallInInitialPos();

            if (!_ballsInstantiated)
            {
                // place the ball in the rack position
                PlaceBallBasedOnGameType();

                _ballsInstantiated = true;
            }
        }

        public void OnPaused()
        {
            ChangeGameState(GameState.Pause);
        }

        public void OnContinue()
        {
            ChangeGameState(GameState.Play);
        }

        /// <summary>
        /// This function places the ball based on the game type
        /// There are prefabs created in a order to make the placement easy based on the selected game type
        /// </summary>
        private void PlaceBallBasedOnGameType()
        {
            string rackString = "Rack";
            Instantiate((Resources.Load(_gameType.ToString() + rackString, typeof(GameObject)) as GameObject), _rackTransform.position, _rackTransform.rotation);
        }

        // this function will be called by cueball when they come to rest after the shot is taken
        public void ReadyForNextRound()
        {
            // lets allow the player to take some shot while the game is not started yet
            if (CurrGameState == GameState.Practise)
            {
                // white ball might be pocketed or hit out
                // since there is only white before the Play state dont have to check for the ball type, just see which array has the ball object
                _cueBall.PlaceBallInPosWhilePractise();
            }
            // pause is added to just make sure its ready for next round
            else if(CurrGameState == GameState.Play || CurrGameState == GameState.Pause)
            {
                NumOfBallsStriked--;

                // all the balls in the pool table are now stationary, let the game continue
                if (NumOfBallsStriked == 0)
                    CalculateThePointAndNextTurn();
            }
            else
            {
                // do nothing
            }
        }

        private void CalculateThePointAndNextTurn()
        {
            // now that all the ball are stationary, lets decide on  next state of game
            // check the balls pocketed list, first for the cueball and then to the count
            // if there is a cue ball in the list then the current player looses a point, and the cue ball is placed in the table
            // else then the score is count for the current player
            // then check if there are any ball in the floor
            // if there is any they get placed in their respective position
            Player currPlayer = _players.Peek();

            // check if the player has striked the ball
            if (currPlayer.HasStrikedBall)
            {
                CueBallController whiteBall = _ballsPocketed.FirstOrDefault(b => b.BallType == CueBallController.CueBallType.White);
                if (whiteBall != null)
                {
                    // player looses score
                    currPlayer.CalculateScore(-1);

                    // remove the white ball from pocket
                    _ballsPocketed.Remove(whiteBall);

                    // set all pocketed balls to true, as the ball pocketed along with the white ball is considered already pocketed
                    _ballsPocketed.ForEach(b => b.IsPocketedInPrevTurn = true);

                    // place the cue ball back in the table
                    whiteBall.PlaceBallInInitialPos();

                    SetNewPlayerTurn();
                }
                else
                {
                    if (_ballsPocketed.Count() > 0)
                    {
                        // get the balls that are currently pockted, the currently pocketed ball will have a value set to false
                        var ballsCurrentlyPocketed = _ballsPocketed.Where(b => b.IsPocketedInPrevTurn == false);
                        Debug.Log("Balls Currently Pocketed" + ballsCurrentlyPocketed.Count());
                        if (ballsCurrentlyPocketed.Count() > 0)
                        {
                            // count for the number of balls pocketed, and increment the player score
                            currPlayer.CalculateScore(ballsCurrentlyPocketed.Count());

                            // set all pocketed balls to true
                            _ballsPocketed.ForEach(b => b.IsPocketedInPrevTurn = true);

                            // player continues to play
                        }
                        else
                        {
                            SetNewPlayerTurn();
                        }

                    }
                    else
                    {
                        SetNewPlayerTurn();
                    }
                }

                // place these balls back in the pool table
                foreach (var ballHitOut in _ballsHitOut)
                    ballHitOut.PlaceBallInInitialPos();
            }

            // clear up every information
            _ballsHitOut.Clear();

            // reset players state
            foreach(var player in _players)
            {
                // here we are checking if the current player in iteration is the first player and setting the state accordingly
                // only the first player in the queue is in playing state
                player.SetPlayingState((player == _players.Peek()));
            }

            if (IsGameComplete())
            {
                StartCoroutine(OnGameComplete());
            }
            else {
                EventManager.Notify(typeof(CueBallActionEvent).Name, this, new CueBallActionEvent() { State = CueBallActionEvent.States.Stationary });
            }
        }

        private bool IsGameComplete()
        {
            if (_ballsPocketed.Count() == (int)_gameType)
                return true;

            return false;
        }

        private IEnumerator OnGameComplete()
        {
            yield return new WaitForEndOfFrame();

            int winningScore = 0;

            // check the highest scorer
            foreach (var player in _players)
            {
                if (player.Score >= winningScore)
                    winningScore = player.Score;
            }

            // now that we have found the winning score, check if there is anyone else with the same score
            Winners = _players.Where(p => p.Score == winningScore).Select(p => p.Name).ToArray();

            // give enough time for the ball, cue and camera to return back to its original position
            EventManager.Notify(typeof(GameStateEvent).Name, this, new GameStateEvent() { GameState = GameStateEvent.State.Complete });
        }

        private void SetNewPlayerTurn()
        {
            // next player takes the chance
            Player player = _players.Dequeue();
            _players.Enqueue(player);

            // get the player on peek to diplay the turn
            Player newPlayer = _players.Peek();
            EventManager.Notify(typeof(GameStateEvent).Name, this, new GameStateEvent() { CurrPlayer = newPlayer.Name });
        }

        public void AddToBallPocketedList(CueBallController ball)
        {
            // making sure we are not adding a ball multiple times
            if (!_ballsPocketed.Contains(ball))
                _ballsPocketed.Add(ball);
        }

        public void AddToBallHitOutList(CueBallController ball)
        {
            // these balls will be back on pool table
            // check if the ball is already potted too and then fell on floor
            if (!_ballsHitOut.Contains(ball) && !_ballsPocketed.Contains(ball))
                _ballsHitOut.Add(ball);
        }
    }
}
