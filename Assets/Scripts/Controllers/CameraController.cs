using UnityEngine;
using ThreeDPool.EventHandlers;
using ThreeDPool.Managers;

namespace ThreeDPool.Controllers
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField]
        private Transform _cueBall = null;

        // distance from the cue ball
        private float _distFromCueBall;

        private Vector3 _initialPos;
        private Vector3 _initialDir;

        // this is the position to rotate around when the ball is been striked to stationary
        // the default value for this vector should be one to avoid unexpected behavior
        private Vector3 _posToRot = Vector3.one;

        // Use this for initialization
        private void Start()
        {
            // cache the initial position and rotation
            _initialPos = transform.position;
            _initialDir = transform.forward;

            // making sure the distance is same as what we started with
            _distFromCueBall = Vector3.Distance(_cueBall.position, transform.position);

            // subscribe for events
            EventManager.Subscribe(typeof(GameInputEvent).Name, OnGameInputEvent);
            EventManager.Subscribe(typeof(CueBallActionEvent).Name, OnCueBallEvent);
            EventManager.Subscribe(typeof(GameStateEvent).Name, OnGameStateEvent);
        }

        private void OnDestroy()
        {
            // unsubscribe for events
            EventManager.Unsubscribe(typeof(GameInputEvent).Name, OnGameInputEvent);
            EventManager.Unsubscribe(typeof(CueBallActionEvent).Name, OnCueBallEvent);
            EventManager.Unsubscribe(typeof(GameStateEvent).Name, OnGameStateEvent);
        }

        // handle camera action based on the cue ball event
        private void OnCueBallEvent(object sender, IGameEvent gameEvent)
        {
            CueBallActionEvent cueBallActionEvent = (CueBallActionEvent)gameEvent;

            switch (cueBallActionEvent.State)
            {
                case CueBallActionEvent.States.Stationary:
                case CueBallActionEvent.States.Default:
                    {
                        float yPos = transform.position.y;

                        // move the camera closer to the cue ball
                        transform.position = _cueBall.transform.position - transform.forward * _distFromCueBall;
                        transform.position = new Vector3(transform.position.x, yPos, transform.position.z);

                        // keep looking at the cue ball
                        transform.LookAt(_cueBall);

                        // reset the pos to rot vector
                        _posToRot = Vector3.one;
                    }
                    break;
                case CueBallActionEvent.States.Striked:
                    {
                        _posToRot = _cueBall.transform.position;
                    }
                    break;
            }
        }

        private void OnGameInputEvent(object sender, IGameEvent gameEvent)
        {
            GameInputEvent gameInputEvent = (GameInputEvent)gameEvent;

            switch (gameInputEvent.State)
            {
                case GameInputEvent.States.HorizontalAxisMovement:
                    {
                        if (_posToRot == Vector3.one)
                        {
                            // start rotating camera around the ball
                            transform.RotateAround(_cueBall.position, Vector3.up, 20f * gameInputEvent.axisOffset * Time.deltaTime);
                        }
                        else
                        {
                            // start rotating camera around _posToRot vector
                            transform.RotateAround(_posToRot, Vector3.up, 20f * gameInputEvent.axisOffset * Time.deltaTime);
                        }
                    }
                    break;
                case GameInputEvent.States.VerticalAxisMovement:
                    {
                        // nothing is done here for now
                    }
                    break;
            }
        }

        private void OnGameStateEvent(object sender, IGameEvent gameEvent)
        {
            GameStateEvent gameStateEvent = (GameStateEvent)gameEvent;
            switch (gameStateEvent.GameState)
            {
                case GameStateEvent.State.Play:
                    {
                        PlaceInInitialPosAndRot();
                    }
                    break;
            }
        }

        private void PlaceInInitialPosAndRot()
        {
            _posToRot = Vector3.one;

            transform.position = _initialPos;
            transform.forward = _initialDir;
        }
    }
}
