using System.Collections;
using KsubakaPool.Managers;
using KsubakaPool.EventHandlers;
using UnityEngine;

namespace KsubakaPool.Controllers
{
    class CueController : MonoBehaviour
    {
        [SerializeField]
        private Transform _cueBall = null;

        // distance from the cue ball
        private float _defaultDistFromCueBall;

        private float _maxClampDist = 9;

        // the distance from cue ball to the cue determines the force gathered
        private float _forceGathered = 0.0f;

        // minimum force threshold required to consider a valid shot
        private float _forceThreshold = 0.5f;

        private float _speed = 10.0f;
        private bool _cueReleasedToStrike = false;

        private Vector3 _initialPos;
        private Vector3 _initialDir;

        // this is the position to rotate around when the ball is been striked to stationary
        // the default value for this vector should be one to avoid unexpected behavior
        private Vector3 _posToRot = Vector3.one;

        public float ForceGatheredToHit { get { return _forceGathered;  } }

        private void Start()
        {
            // cache the initial position and rotation
            _initialPos = transform.position;
            _initialDir = transform.forward;

            // making sure the distance is same as what we started with
            _defaultDistFromCueBall = Vector3.Distance(_cueBall.position, transform.position);

            EventManager.Subscribe(typeof(GameInputEvent).Name, OnGameInputEvent);
            EventManager.Subscribe(typeof(CueBallActionEvent).Name, OnCueBallEvent);
            EventManager.Subscribe(typeof(GameStateEvent).Name, OnGameStateEvent);
        }

        private void OnDestroy()
        {
            EventManager.Unsubscribe(typeof(GameInputEvent).Name, OnGameInputEvent);
            EventManager.Unsubscribe(typeof(CueBallActionEvent).Name, OnCueBallEvent);
            EventManager.Unsubscribe(typeof(GameStateEvent).Name, OnGameStateEvent);
        }

        private void OnGameInputEvent(object sender, IGameEvent gameEvent)
        {
            GameInputEvent gameInputEvent = (GameInputEvent)gameEvent;
            // start moving the cue stick towards the ball
            switch (gameInputEvent.State)
            {
                case GameInputEvent.States.HorizontalAxisMovement:
                    {
                        if (_posToRot == Vector3.one)
                            transform.RotateAround(_cueBall.position, Vector3.up, 20f * gameInputEvent.axisOffset * Time.deltaTime);
                        else
                            transform.RotateAround(_posToRot, Vector3.up, 20f * gameInputEvent.axisOffset * Time.deltaTime);
                    }
                    break;
                case GameInputEvent.States.VerticalAxisMovement:
                    {
                        // this means that the ball is moving
                        if (_posToRot != Vector3.one)
                            return;

                        // clamp the cue movement, else it will be frustrating for the player
                        var newPosition = transform.position + transform.forward * gameInputEvent.axisOffset;

                        _forceGathered = Vector3.Distance(_cueBall.position, newPosition);
                        if ((_forceGathered < _defaultDistFromCueBall + _maxClampDist) &&
                            _forceGathered > _defaultDistFromCueBall)
                        {
                            transform.position = newPosition;
                            EventManager.Notify(typeof(CueActionEvent).ToString(), this, new CueActionEvent() { ForceGathered = _forceGathered });
                        }
                        else
                        {
                        }

                    }
                    break;
                case GameInputEvent.States.Release:
                    {
                        // the cue ball is not stationary as of now
                        if (_posToRot != Vector3.one)
                            return;

                        if (_forceGathered > _defaultDistFromCueBall + _forceThreshold)
                            _cueReleasedToStrike = true;
                    }
                    break;
            }
        }

        /// <summary>
        /// handle cue controller based on the cue ball events 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="gameEvent"></param>
        private void OnCueBallEvent(object sender, IGameEvent gameEvent)
        {
            CueBallActionEvent cueBallActionEvent = (CueBallActionEvent)gameEvent;

            // start moving the cue stick towards the ball
            switch (cueBallActionEvent.State)
            {
                case CueBallActionEvent.States.Stationary:
                case CueBallActionEvent.States.Default:
                    {
                        // making sure everything is clean
                        _forceGathered = 0f;

                        // on ready for next shot position the cue controller closer to cue ball
                        transform.position = _cueBall.transform.position - transform.forward * _defaultDistFromCueBall;
                        transform.LookAt(_cueBall);

                        _posToRot = Vector3.one;
                    }
                    break;
                case CueBallActionEvent.States.Striked:
                    {
                        _cueReleasedToStrike = false;

                        // make the cue ball go back in the play state
                        if (GameManager.Instance.CurrGameState == GameManager.GameState.Play)
                        {
                            // move the cue backward after striking so that cue ball doesnt touch the cue 
                            StartCoroutine(MoveCueAfterStrike(transform.position, _cueBall.transform.position - transform.forward * _defaultDistFromCueBall * 1.5f, 1.0f));
                        }

                        transform.LookAt(_cueBall);

                        _posToRot = _cueBall.transform.position;
                    }
                    break;
            }
        }

        private void OnGameStateEvent(object sender, IGameEvent gameEvent)
        {
            GameStateEvent gameStateEvent = (GameStateEvent)gameEvent;
            switch(gameStateEvent.GameState)
            {
                case GameStateEvent.State.Play:
                    {
                        PlaceInInitialPosAndRot();
                    }
                    break;
            }
        }

        /// <summary>
        /// this function makes the cue move after striking the cueball
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="overTime"></param>
        /// <returns></returns>
        IEnumerator MoveCueAfterStrike(Vector3 source, Vector3 target, float overTime)
        {
            float startTime = Time.time;
            while (Time.time < startTime + overTime)
            {
                transform.position = Vector3.Lerp(source, target, (Time.time - startTime) / overTime);
                yield return null;
            }
            transform.position = target;
        }

        private void FixedUpdate()
        {
            if(_cueReleasedToStrike)
            {
                float step = _speed * Time.deltaTime * (_forceGathered/_speed);
                transform.position = Vector3.MoveTowards(transform.position, _cueBall.transform.position, step);

                // cue ball will now detect if the cue actually hit it and then behave accordingly
                // this event will be notifies by the cue ball
            }
        }

        private void PlaceInInitialPosAndRot()
        {
            _forceGathered = 0f;
            _cueReleasedToStrike = false;
            _posToRot = Vector3.one;

            transform.position = _initialPos;
            transform.forward = _initialDir;
        }
    }
}
