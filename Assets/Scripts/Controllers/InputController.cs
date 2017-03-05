using UnityEngine;
using ThreeDPool.EventHandlers;
using ThreeDPool.Managers;

namespace ThreeDPool.Controllers
{
    class InputController : MonoBehaviour 
    {
        private void Update()
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                // game paused
                EventManager.Notify(typeof(GameInputEvent).Name, this, new GameInputEvent() { State = GameInputEvent.States.Paused });
            }

            // dont send any input when the game is either paused or in getset state
            if (GameManager.Instance.CurrGameState == GameManager.GameState.GetSet ||
                GameManager.Instance.CurrGameState == GameManager.GameState.Pause)
                return;

            float x = 0.0f;
            float y = 0f;
            if (Input.GetMouseButton(0))
            {
                // on A or D or left arrow or right arrow or LMB along x are the cue controllers
                x = Input.GetAxis("Mouse X") - Input.GetAxis("Horizontal");
                y = Input.GetAxis("Mouse Y");
            }
            else if(Input.GetMouseButtonUp(0))
            {
                // the LMB is been released
                EventManager.Notify(typeof(GameInputEvent).Name, this, new GameInputEvent() { State = GameInputEvent.States.Release });
            }
            else
            {

            }

            // notify the event for the input along x
            if(x != 0.0f)
                EventManager.Notify(typeof(GameInputEvent).Name, this, new GameInputEvent() { State = GameInputEvent.States.HorizontalAxisMovement, axisOffset = x });
 
            // notify the event for the input along y
            if(y != 0.0f)
                EventManager.Notify(typeof(GameInputEvent).Name, this, new GameInputEvent() { State = GameInputEvent.States.VerticalAxisMovement, axisOffset = y });
        }
    }
}
