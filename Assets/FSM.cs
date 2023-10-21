using IA.FSM.Villager;
using System;
using System.Collections.Generic;

namespace IA.FSM
{
    public class FSM
    {
        public int currentStateIndex = 0;
        Dictionary<int, State> states;
        private int[,] relations;

        public FSM(int states, int flags)
        {
            currentStateIndex = -1;
            relations = new int[states, flags];
            for (int i = 0; i < states; i++)
            {
                for (int j = 0; j < flags; j++)
                {
                    relations[i, j] = -1;
                }
            }
            this.states = new Dictionary<int, State>();
        }

        public void SetCurrentStateForced(int state)
        {
            currentStateIndex = state;
        }

        public void SetRelation(int sourceState, int flag, int destinationState)
        {
            relations[sourceState, flag] = destinationState;
        }

        public void SetFlag(int flag)
        {
            if (relations[currentStateIndex, flag] != -1)
            {
                foreach (Action OnExit in states[currentStateIndex].GetOnExitBehaviours())
                    OnExit?.Invoke();

                currentStateIndex = relations[currentStateIndex, flag];

                foreach (Action OnEnter in states[currentStateIndex].GetOnEnterBehaviours())
                    OnEnter?.Invoke();
            }
        }

        public void AddState<T>(int stateIndex) where T : State, new()
        {
            if (!states.ContainsKey(stateIndex)) 
            {
                State newState = new T();
                newState.SetFlag += SetFlag;
                states.Add(stateIndex, newState);
            }
        }

        public void Update(params object[] parameters)
        {
            if (states.ContainsKey(currentStateIndex))
            {
                foreach (Action behaviour in states[currentStateIndex].GetBehaviours(parameters))
                {
                    behaviour?.Invoke();
                }
            }
        }
    }
}