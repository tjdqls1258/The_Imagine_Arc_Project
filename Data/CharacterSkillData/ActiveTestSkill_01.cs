using UnityEngine;

[System.Serializable]
public class ActiveTestSkill_01 : SkillActionBase
{
    public float damageAmount;

    public override bool ActiveSkill(InGameCharacterData data, IGamePlayCharacter targetControll = null)
    {
        if (targetControll == null) return false;
        Logger.Log("Active Skill Active");
        Logger.Log($"Atk Damage : {data.GetAtk() * damageAmount},  target : {(targetControll as EnemyController).name}");

        return true;
    }
}

[System.Serializable]
public class PassiveTestSkill_01 : SkillActionBase
{
    public override bool ActiveSkill(InGameCharacterData data, IGamePlayCharacter targetControll = null)
    {
        Logger.Log("Active Skill Passive");
        return true;
    }
}

[System.Serializable]
public class ActiveTestSkill_03 : SkillActionBase
{ 
    public override bool ActiveSkill(InGameCharacterData data, IGamePlayCharacter targetControll = null)
    {
        if (targetControll == null) return false;
        Logger.Log("Active Skill Buff");

        return true;
    }
}