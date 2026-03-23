using System;
using System.Collections.Generic;
using UnityEngine;

namespace Util_Patten
{ 
    public abstract class State<T, owner> where T : struct, Enum
    {
        public T stateType;
        public owner m_owner;

        public abstract void Enter();
        public abstract void Update();
        public abstract void Exit();
    }

    public abstract class MonoState<T, owner> : State<T, owner> where T : struct, Enum
    {
    }

    public class StateManager<T, owner> : MonoBehaviour where T : struct, Enum
    {
        [SerializeField] private T m_currentState;
        [SerializeField] private State<T, owner>[] m_states;

        Dictionary<T, State<T, owner>> m_stateList = new();
        private State<T, owner> m_activeState;

        protected virtual void Awake()
        {
            foreach (var state in m_states)
            {
                if (!m_stateList.ContainsKey(state.stateType))
                    m_stateList.Add(state.stateType, state);
            }

            // 2. 초기 상태 설정
            if (m_stateList.TryGetValue(m_currentState, out var startState))
            {
                m_activeState = startState;
            }
        }

        public void ChangeState(T nextState)
        {
            // 동일한 상태로 변경 요청 시 무시
            if (EqualityComparer<T>.Default.Equals(m_currentState, nextState))
                return;

            if (m_stateList.TryGetValue(nextState, out var newState))
            {
                // m_activeState?.Exit(); // 기존 상태 종료

                m_currentState = nextState;
                m_activeState = newState;

                // m_activeState.Enter(); // 새 상태 시작
                Debug.Log($"State Changed to: {nextState}");
            }
        }

        public void RunState()
        {
            if (m_activeState != null)
                m_activeState.Update();
        }
    }
}

/*
##############사용법####################

public enum PlayerState { Idle, Move, Attack }

// Player 전용 상태 매니저
public class PlayerBrain : StateManager<PlayerState, PlayerBrain> 
{
    // 추가적인 플레이어 공통 로직들...
}

// 구체적인 상태 구현
[Serializable]
public class IdleState : State<PlayerState, PlayerBrain>
{
    public override void Enter() => Debug.Log("숨 고르기 시작");
    public override void Update() { 대기 로직  }
    public override void Exit() => Debug.Log("대기 끝");
}
*/