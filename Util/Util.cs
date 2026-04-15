using UnityEngine;

public static class Util
{
#if UNITY_EDITOR
    public const string SKILL_SPRITE_PATH = "Assets/Image/Skill/{0}.png";
#endif
    //######### Character Data
    public const string CHARACTER_SPRITE_PATH = "Assets/Sprite/TestCharacterImage/{0}.spriteatlas";
    public const string CHARACTER_IMAGE_NAME = "CharacterImage";
    public const string CHARACTER_MODLED_PATH = "Character/{0}.prefab";
    public const string CHARACTER_SKILL_PATH = "ScriptableObject/SkillBase/SkillBase_{0}.asset";

    //######### Enemy Data
    public const string ENEMY_MODLED_PATH = "Enemy/{0}.prefab";

    //######### Vector
    public static readonly Vector3 REVERSE_2D = new Vector3(-1, 1, 1);


    //#### Map
    public const string MAP_DATAPATH_FORMAT = "MapData-{0}-{1}"; //  (MapData-{Main}-{Sub})
    public const string STAGE_NAME = "Stage-{0}-{1}";
    public const string MAP_DATA_FOLDER = "MapData/{0}.asset";
    public const string SPRITE_ATLAS_FOLDER = "SpriteAltas/{0}.spriteatlas";
}
