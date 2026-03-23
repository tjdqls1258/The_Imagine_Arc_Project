using System.Collections.Generic;
using UnityEngine;

namespace Util_Patten
{
    public interface ICommand
    {
        public void Execute();
        public void Undo();
    }
    public abstract class Command<T> : ICommand
    {
        public abstract void Execute();
        public abstract void Undo();
    }

    public class CommandInvoker : MonoBehaviour
    {
        private Stack<ICommand> m_commandHistory = new();

        // 명령 실행
        public void ExecuteCommand(ICommand command)
        {
            command.Execute();
            m_commandHistory.Push(command);
        }

        // 실행 취소 (Undo)
        public void UndoCommand()
        {
            if (m_commandHistory.Count > 0)
            {
                ICommand lastCommand = m_commandHistory.Pop();
                lastCommand.Undo();
            }
        }
    }
}

/* 
############### 사용 예시 #########################
public class MoveCommand : Command<Transform>
{
    private Vector3 m_distance;

    public MoveCommand(Transform receiver, Vector3 distance) : base(receiver)
    {
        m_distance = distance;
    }

    public override void Execute() => receiver.Translate(m_distance);
    public override void Undo() => receiver.Translate(-m_distance);
}

// 플레이어 컨트롤러에서의 활용
public class PlayerInput : MonoBehaviour
{
    [SerializeField] private CommandInvoker invoker;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            var moveUp = new MoveCommand(transform, Vector3.up);
            invoker.ExecuteCommand(moveUp);
        }

        if (Input.GetKeyDown(KeyCode.Z)) // Ctrl+Z 같은 Undo 기능
        {
            invoker.UndoCommand();
        }
    }
}
 */