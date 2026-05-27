using System.Collections.Generic;
using UnityEngine;

public class PassTargetModule_Tick : TargetingModule
{
    public override bool ExecuteTargeting(SkillContext context, List<EffectModule> logicEffects)
    {
        return true;
    }
}
