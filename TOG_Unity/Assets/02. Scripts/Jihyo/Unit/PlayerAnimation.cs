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
    private static readonly int SetPositionHash = Animator.StringToHash("SetPosition");

    [Header("Animation State Names")]
    [SerializeField] private string attack1StateName = "Player1_Attack1";
    [SerializeField] private string attack2StateName = "Player1_Attack2";
    [SerializeField] private string attack3StateName = "Player1_Attack3";
    [SerializeField] private string attack1EnforceStateName = "Player1_Attack1_Enforce";
    [SerializeField] private string attack2EnforceStateName = "Player1_Attack2_Enforce";
    [SerializeField] private string attack3EnforceStateName = "Player1_Attack3_Enforce";
    [SerializeField] private string attack1MoveStateName = "Player1_Attack1_Move";
    [SerializeField] private string attack2MoveStateName = "Player1_Attack2_Move";
    [SerializeField] private string attack3MoveStateName = "Player1_Attack3_Move";

    [Header("Animation Settings")]
    [SerializeField] private float fallbackMotionDuration = 0.3f;
    [SerializeField] private float attackEffectDelay = 0.08f;
    [SerializeField] private float attackDamageDelay = 0.08f;

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

    public void TriggerSetPosition()
    {
        if (animator != null)
        {
            animator.SetTrigger(SetPositionHash);
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
            animator.ResetTrigger(SetPositionHash);
        }
    }

    public IEnumerator WaitForEnforceAnimationComplete(int attackValue)
    {
        yield return WaitForStateComplete(GetEnforceStateName(attackValue));
    }

    public IEnumerator WaitUntilMoveState(int attackValue)
    {
        yield return WaitUntilState(GetMoveStateName(attackValue));
    }

    public IEnumerator WaitUntilAttackState(int attackValue)
    {
        yield return WaitUntilState(GetAttackStateName(attackValue));
    }

    public float GetAttackEffectDelay()
    {
        return attackEffectDelay;
    }

    public float GetAttackDamageDelay()
    {
        return attackDamageDelay;
    }

    public IEnumerator WaitForAttackAnimationComplete(int attackValue)
    {
        yield return WaitForCurrentStateFinish(GetAttackStateName(attackValue));
    }

    private IEnumerator WaitUntilState(string stateName)
    {
        if (animator == null || string.IsNullOrEmpty(stateName))
        {
            yield break;
        }

        int stateHash = Animator.StringToHash(stateName);

        while (true)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.shortNameHash == stateHash)
            {
                break;
            }

            yield return null;
        }
    }

    private IEnumerator WaitForStateComplete(string stateName)
    {
        yield return WaitUntilState(stateName);
        yield return WaitForCurrentStateFinish(stateName);
    }

    private IEnumerator WaitForCurrentStateFinish(string stateName)
    {
        if (animator == null || string.IsNullOrEmpty(stateName))
        {
            if (fallbackMotionDuration > 0f)
            {
                yield return new WaitForSeconds(fallbackMotionDuration);
            }

            yield break;
        }

        int stateHash = Animator.StringToHash(stateName);

        while (true)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.shortNameHash != stateHash)
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
        if (attackValue < lightAttack)
        {
            return attack1EnforceStateName;
        }

        if (attackValue < normalAttack)
        {
            return attack2EnforceStateName;
        }

        return attack3EnforceStateName;
    }

    private string GetMoveStateName(int attackValue)
    {
        if (attackValue < lightAttack)
        {
            return attack1MoveStateName;
        }

        if (attackValue < normalAttack)
        {
            return attack2MoveStateName;
        }

        return attack3MoveStateName;
    }
}
