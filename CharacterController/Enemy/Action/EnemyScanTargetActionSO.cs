using System.Collections.Generic;
using UnityEngine;
using Util_Patten.FSM;

[CreateAssetMenu(menuName = "FSM/Enemy/Actions/Scan Target (Ranged)")]
public class EnemyScanTargetActionSO : ActionSO<EnemyContext>
{
    private float scanInterval = 0.2f; // 매 프레임 Physics 연산을 막기 위한 최적화 타이머

    public override void OnUpdate(EnemyContext context)
    {
        if (context.currentTarget != null || context.isDie) return;

        if (Time.time >= context.nextScanTime)
        {
            context.nextScanTime = Time.time + scanInterval;
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(context.targetLayer);
            filter.useTriggers = false;

            if (context.attackType == EnemyAttackType.Ranged)
            {
                List<Collider2D> hits = new List<Collider2D>();
                int hitCount = Physics2D.OverlapCircle(context.transform.position, context.enemyStatManager.GetStat(StatType.AttackRange), filter, hits);

                if (hitCount > 0)
                {
                    context.currentTarget = hits[0].GetComponent<ITargetable>();
                }
                else
                {
                    context.currentTarget = null;
                }
            }
            else if (context.attackType == EnemyAttackType.Melee)
            {
                if (context.movePathList.Count > 0 && context.currentPathIndex < context.movePathList.Count)
                {
                    Vector2 currentPos = context.transform.position;
                    Vector2 targetPos = context.movePathList[context.currentPathIndex];
                    Vector2 direction = (targetPos - currentPos).normalized;

                    List<RaycastHit2D> hits = new List<RaycastHit2D>();
                    float castRadius = 0.2f;
                    float meleeRange = 0.2f;

                    int hitCount = Physics2D.CircleCast(currentPos, castRadius, direction, filter, hits, meleeRange);

#if UNITY_EDITOR
                    Debug.DrawRay(currentPos, direction * meleeRange, Color.red, scanInterval);
#endif

                    if (hitCount > 0)
                    {
                        var blocker = hits[0].collider.GetComponent<PlayerCharacterController>();

                        if (blocker != null && blocker.TryBlock(context.stateManager))
                        {
                            context.currentBlocker = blocker;
                            context.isBlocked = true;
                            context.currentTarget = blocker;
                        }
                    }
                    else
                    {
                        context.currentTarget = null;
                    }
                }
            }
        }
    }
}
