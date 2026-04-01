#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.U2D;

public class SkillBase : ScriptableObject, IUpdateDataFormSheet
{
    public int ID;
    public string SkillName;
    public string SkillDescription;
    public float SkillCoolTime;
    public float Duration;
    public string SkillIconName;
    public Sprite SkillIcon;

    public virtual void SkillUse(InGameCharacterData data) { }

#if UNITY_EDITOR
    [ContextMenu("Set Skill Icon")]
    public void SetSkillIcon()
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(string.Format(Util.SKILL_SPRITE_PATH, SkillIconName));
        SkillIcon = sprite;
        AssetDatabase.SaveAssetIfDirty(this);
        AssetDatabase.SaveAssets();
    }
#endif
}