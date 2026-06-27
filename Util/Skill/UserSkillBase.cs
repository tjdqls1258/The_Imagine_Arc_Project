using UnityEngine;

[CreateAssetMenu(menuName = "Skill System/User Skill Template")]
public class UserSkillBase : SkillBase
{
    [SerializeField] private int Cost;

    public int GetCost() { return Cost; }
}
