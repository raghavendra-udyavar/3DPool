using System.Collections.Generic;
using UnityEngine;

namespace KsubakaPool.States
{
    public class FSM :  MonoBehaviour
    {
        private List<IState> _states;

        private IState _currentState;

        public void AddState(IState state)
        {
            // only add if this state is not 
            if(_states.Find(s => s.GetType() == state.GetType()) != null)
                _states.Add(state);
        }

        public void ChangeStateTo(IState newState)
        {
            // dont change the state if in that state already
            if (newState == _currentState)
                return;

            // exit from the current state if there is any
            if(_currentState != null)
                _currentState.OnExit();
            
            // enter the new state 
            if(newState != null)
            {
                _currentState = newState;
                _currentState.OnEnter();
            }          
        }

        public void Update()
        {
            // update the current state
            if (_currentState != null)
                _currentState.OnUpdate();
        }
    }
}
