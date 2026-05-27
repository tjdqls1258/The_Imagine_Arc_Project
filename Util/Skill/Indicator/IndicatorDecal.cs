using UnityEngine;
using UnityEngine.Rendering.Universal;

public class IndicatorDecal : IndicatorObject
{
    [SerializeField] private DecalProjector m_decalProjecdtor;
 
    public override void SettingRange(float range)
    {
        m_decalProjecdtor.size = new Vector3(range * 2, range * 2, range * 2);
    }
}
