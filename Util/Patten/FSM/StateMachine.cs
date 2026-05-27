using System.Collections.Generic;
using UnityEngine;

namespace Util_Patten.FSM
{
    public abstract class Context
    {
        public virtual void Init() { }
    }

    public abstract class StateMachine<T, TState> : MonoBehaviour 
        where T : Context
        where TState : StateSO<T>
    {
        [Header("FSM Settings")]
        public TState startState;

        [SerializeField] protected T context;
        [Tooltip("실행 중 상태 확인용(디버깅)"), SerializeField]
        public TState currentState;


        protected virtual void Start()
        {
            context.Init();
            if (startState != null && context != null)
            {
                currentState = startState;
                startState.OnEnter(context);
            }
            else
                Debug.LogWarning($"{this.gameObject.name}: has Not context or StartState");
        }

        protected virtual void Update()
        {
            if (currentState == null) return;

            var nextState = currentState.UpdateState(context) as TState;

            if (nextState != currentState)
                ForeChangeState(nextState);

        }

        protected virtual void ForeChangeState(TState state)
        {
            currentState.OnExit(context);
            currentState = state;
            currentState.OnEnter(context);
        }
    }
}