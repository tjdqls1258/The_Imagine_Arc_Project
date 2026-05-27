using UnityEngine;
using Util_Patten.FSM;

namespace Character_State
{
    [CreateAssetMenu(fileName = "CharacterState", menuName = "FSM/State/Unit State")]
    public class CharacterStateScriptableObjcetBase : StateSO<CharacterContext>
    {
        public CharacterState StateType = CharacterState.DisableSpawn;
    }
}