using UnityEngine;
using Util_Patten.FSM;

[CreateAssetMenu(menuName = "FSM/Enemy/Actions/Attack")]
public class EnemyAttackActionSO : ActionSO<EnemyContext>
{
    public override void OnEnter(EnemyContext context)
    {
        // 공격 상태 진입 시 즉시 타격하지 않고 한 템포 쉬거나 바로 때리도록 타이머 조정
        context.lastAttackTime = Time.time - context.enemyStatManager.GetStat(StatType.AttackSpeed);
    }

    public override void OnUpdate(EnemyContext context)
    {
        if (context.currentTarget == null || context.isDie) return;

        if (Time.time >= context.lastAttackTime + context.enemyStatManager.GetStat(StatType.AttackSpeed))
        {
            context.lastAttackTime = Time.time;

            context.animController.PlayAnimation_Trigger(CharacterAnimationController.AnimationTrigger.ATK);
            ExecuteAttack(context);
        }
    }

    private void ExecuteAttack(EnemyContext context)
    {
        context.skillcontext.PrimaryTarget = context.currentTarget;
        context.nomalSkill.ExecuteActive(context.skillcontext);
        //if (context.attackType == EnemyAttackType.Melee)
        //{
        //    EffectPayload payload = new EffectPayload { Value = context.enemyStatManager.GetStat(StatType.AttackDamage)};
        //    ((ITargetable)context.currentTarget).ApplyEffect(payload);
        //}
        //else
        //{
        //    // 원거리
        //}
    }
}