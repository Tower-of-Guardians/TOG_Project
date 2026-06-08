using System.Collections;
using UnityEngine;

public class MonsterAnimation : MonoBehaviour
{
    [Header("Animation")]
    private Animator animator;

    [Header("Animator Parameters")]
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int HitHash = Animator.StringToHash("Hit");
    private static readonly int DeadHash = Animator.StringToHash("Dead");
    private static readonly int CurseHash = Animator.StringToHash("Curse");
    private static readonly int DefenseHash = Animator.StringToHash("Defense");
    private static readonly int SummonHash = Animator.StringToHash("Summon");

    private void Awake()
    {
        InitializeAnimator();
    }

    private void InitializeAnimator()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    public void PlayAttackAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(AttackHash);
        }
    }

    public void PlayHitAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(HitHash);
        }
    }

    public void PlayDeadAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(DeadHash);
        }
    }

    public void PlayCurseAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(CurseHash);
        }
    }

    public void PlayDefenseAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(DefenseHash);
        }
    }

    public void PlaySummonAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(SummonHash);
        }
    }

    public void ResetAnimationState()
    {
        if (animator != null)
        {
            animator.ResetTrigger(AttackHash);
            animator.ResetTrigger(HitHash);
            animator.ResetTrigger(DeadHash);
            animator.ResetTrigger(CurseHash);
            animator.ResetTrigger(DefenseHash);
            animator.ResetTrigger(SummonHash);
        }
    }

    /// <summary>
    /// 공격 애니메이션이 완료될 때까지 대기합니다.
    /// </summary>
    public IEnumerator WaitForAttackAnimationComplete()
    {
        yield return WaitForAnimationComplete(new[] { "Attack", "WhiteDog_Attack", "Clonier_Attack" });
    }

    public IEnumerator WaitForActionAnimationComplete(MonsterActionType actionType)
    {
        string[] stateNames = GetStateNamesByActionType(actionType);
        if (stateNames == null || stateNames.Length == 0)
        {
            yield break;
        }

        yield return WaitForAnimationComplete(stateNames);
    }

    private static string[] GetStateNamesByActionType(MonsterActionType actionType)
    {
        switch (actionType)
        {
            case MonsterActionType.Attack:
                return new[] { "Attack", "WhiteDog_Attack", "Clonier_Attack" };
            case MonsterActionType.Guard:
                return new[] { "Defense", "WhiteDog_Defense" };
            case MonsterActionType.ApplyStatus:
                return new[] { "Curse", "WhiteDog_Curse" };
            case MonsterActionType.Summon:
                return new[] { "Summon", "Clonier_Summon" };
            default:
                return null;
        }
    }

    private IEnumerator WaitForAnimationComplete(string[] stateNames)
    {
        if (animator == null)
        {
            yield break;
        }

        float transitionTimeout = 1f;
        float elapsedTime = 0f;
        bool attackStateFound = false;

        while (elapsedTime < transitionTimeout)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            
            if (IsAnyStateName(stateInfo, stateNames))
            {
                attackStateFound = true;
                break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (!attackStateFound)
        {
            // Attack 상태를 찾지 못했으면 바로 종료
            yield break;
        }

        int previousStateHash = animator.GetCurrentAnimatorStateInfo(0).fullPathHash;

        while (true)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            int currentStateHash = stateInfo.fullPathHash;

            bool isActionState = IsAnyStateName(stateInfo, stateNames);
            if (currentStateHash != previousStateHash && !isActionState)
            {
                break;
            }

            if (isActionState && stateInfo.normalizedTime >= 1.0f)
            {
                yield return null;
                break;
            }

            previousStateHash = currentStateHash;
            yield return null;
        }
    }

    private static bool IsAnyStateName(AnimatorStateInfo stateInfo, string[] stateNames)
    {
        if (stateNames == null)
        {
            return false;
        }

        for (int i = 0; i < stateNames.Length; i++)
        {
            if (stateInfo.IsName(stateNames[i]))
            {
                return true;
            }
        }

        return false;
    }
}

