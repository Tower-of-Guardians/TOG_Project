using System.Collections;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [Header("Animation")]
    private Animator animator;

    [Header("Animator Parameters")]
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int Attack1Hash = Animator.StringToHash("Attack1");
    private static readonly int Attack2Hash = Animator.StringToHash("Attack2");
    private static readonly int Attack3Hash = Animator.StringToHash("Attack3");
    private static readonly int HitHash = Animator.StringToHash("Hit");
    private static readonly int DeadHash = Animator.StringToHash("Dead");

    [Header("Animation State Names")]
    [SerializeField] private string attack1StateName = "Player1_Attack1";
    [SerializeField] private string attack2StateName = "Player1_Attack2";
    [SerializeField] private string attack3StateName = "Player1_Attack3";
    [SerializeField] private string attack2EnforceStateName = "Player1_Attack2_Enforce";
    [SerializeField] private string attack3EnforceStateName = "Player1_Attack3_Enforce";
    [SerializeField] private string attack2IdleStateName = "Player1_Attack2_Idle";
    [SerializeField] private string attack3IdleStateName = "Player1_Attack3_Idle";

    [Header("Animation Settings")]
    [SerializeField] private float attackMotionDuration = 0.3f;

    [Header("Attack Thresholds")]
    [SerializeField] private int lightAttack = 10;
    [SerializeField] private int normalAttack = 20;

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

    public void TriggerAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger(AttackHash);
        }
    }

    public void TriggerAttackByValue(int attackValue)
    {
        if (animator == null)
        {
            return;
        }

        if (attackValue < lightAttack)
        {
            animator.SetTrigger(Attack1Hash);
        }
        else if (attackValue < normalAttack)
        {
            animator.SetTrigger(Attack2Hash);
        }
        else
        {
            animator.SetTrigger(Attack3Hash);
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

    public void ResetAnimationState()
    {
        if (animator != null)
        {
            animator.ResetTrigger(AttackHash);
            animator.ResetTrigger(Attack1Hash);
            animator.ResetTrigger(Attack2Hash);
            animator.ResetTrigger(Attack3Hash);
            animator.ResetTrigger(HitHash);
        }
    }

    public IEnumerator WaitForEnforceAnimationComplete(int attackValue)
    {
        // 연출시간 1초 대기(하드코딩)
        yield return new WaitForSeconds(1.0f);
    }

    public IEnumerator WaitForAttackAnimationComplete(int attackValue)
    {
        string stateName = GetAttackStateName(attackValue);

        if (animator == null || string.IsNullOrEmpty(stateName))
        {
            if (attackMotionDuration > 0f)
            {
                yield return new WaitForSeconds(attackMotionDuration);
            }
            yield break;
        }

        int attackStateHash = Animator.StringToHash(stateName);

        // Attack 상태로 전환될 때까지 대기
        while (true)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            if (stateInfo.shortNameHash == attackStateHash)
            {
                break;
            }

            yield return null;
        }

        // Attack 상태가 끝날 때까지 대기
        while (true)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            if (stateInfo.shortNameHash != attackStateHash)
            {
                break;
            }

            if (stateInfo.normalizedTime >= 1.0f)
            {
                break;
            }

            yield return null;
        }
    }

    private string GetAttackStateName(int attackValue)
    {
        if (attackValue < lightAttack)
        {
            return attack1StateName;
        }
        else if (attackValue < normalAttack)
        {
            return attack2StateName;
        }
        else
        {
            return attack3StateName;
        }
    }

    private string GetEnforceStateName(int attackValue)
    {
        if (attackValue < normalAttack)
        {
            return attack2EnforceStateName;
        }
        else
        {
            return attack3EnforceStateName;
        }
    }

    private string GetIdleStateName(int attackValue)
    {
        if (attackValue < normalAttack)
        {
            return attack2IdleStateName;
        }
        else
        {
            return attack3IdleStateName;
        }
    }
}
