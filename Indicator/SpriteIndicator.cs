using UnityEngine;

public class SpriteIndicator : IndicatorObject
{
    public override void SettingRange(float range)
    {
        transform.localScale = new Vector3(range, range, range);
    }
}
