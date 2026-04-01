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

    //######### Enemy Data
    public const string ENEMY_MODLED_PATH = "Enemy/{0}.prefab";

    //######### Vector
    public static readonly Vector3 REVERSE_2D = new Vector3(-1, 1, 1);
}
