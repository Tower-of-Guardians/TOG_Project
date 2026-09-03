using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    public const string AttackHitFunctionName = "OnAttackHit";
    public const string SynergyHitFunctionName = "OnSynergyHit";

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
    private static readonly int SynergyHash = Animator.StringToHash("Synergy");

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
    [SerializeField] private string synergyIntroStateName = "Player1_SynergyAttack_Intro";
    [SerializeField] private string synergyLoopStateName = "Player1_SynergyAttack_Loop";
    [SerializeField] private string idleStateName = "Player1_Idle";

    [Header("Animation Settings")]
    [SerializeField] private float fallbackMotionDuration = 0.3f;
    [SerializeField] private float fallbackHitNormalizedTime = 0.488f;
    [SerializeField] private float fallbackHitDelaySeconds = 0.08f;

    [Header("Attack Thresholds")]
    [SerializeField] private int lightAttack = 10;
    [SerializeField] private int normalAttack = 20;

    private readonly Dictionary<string, AttackClipHitTiming> attackClipTimingCache = new Dictionary<string, AttackClipHitTiming>();
    private bool attackHitTriggered;
    private bool synergyHitTriggered;

    private struct AttackClipHitTiming
    {
        public float HitNormalizedTime;
        public bool HasHitEvent;
        public float ClipLength;
    }

    private void Awake()
    {
        InitializeAnimator();
        RebuildAttackClipTimingCache();
    }

    private void InitializeAnimator()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    /// <summary>
    /// Attack 클립 Animation Event에서 호출됩니다.
    /// </summary>
    public void OnAttackHit()
    {
        attackHitTriggered = true;
    }

    /// <summary>
    /// Synergy Loop 클립 Animation Event에서 호출됩니다.
    /// </summary>
    public void OnSynergyHit()
    {
        synergyHitTriggered = true;
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

    public void PlayEnforce(int attackValue)
    {
        if (animator == null)
        {
            return;
        }

        string stateName = GetEnforceStateName(attackValue);
        if (string.IsNullOrEmpty(stateName))
        {
            return;
        }

        animator.Play(stateName, 0, 0f);
        animator.Update(0f);
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
            animator.ResetTrigger(SynergyHash);
        }

        attackHitTriggered = false;
        synergyHitTriggered = false;
    }

    public IEnumerator PlaySynergyIntro()
    {
        if (animator == null)
        {
            if (fallbackMotionDuration > 0f)
            {
                yield return new WaitForSeconds(fallbackMotionDuration);
            }

            yield break;
        }

        animator.ResetTrigger(SynergyHash);
        animator.Play(synergyIntroStateName, 0, 0f);
        animator.Update(0f);
        yield return WaitForStateComplete(synergyIntroStateName);
    }

    public void StartSynergyLoop()
    {
        if (animator == null || string.IsNullOrEmpty(synergyLoopStateName))
        {
            return;
        }

        synergyHitTriggered = false;
        animator.Play(synergyLoopStateName, 0, 0f);
        animator.Update(0f);
    }

    public IEnumerator WaitForSynergyLoopHit(int cycleIndex)
    {
        synergyHitTriggered = false;

        if (animator == null || string.IsNullOrEmpty(synergyLoopStateName))
        {
            if (fallbackHitDelaySeconds > 0f)
            {
                yield return new WaitForSeconds(fallbackHitDelaySeconds);
            }

            yield break;
        }

        int stateHash = Animator.StringToHash(synergyLoopStateName);
        AttackClipHitTiming timing = ResolveClipHitTiming(synergyLoopStateName, SynergyHitFunctionName);
        float hitThreshold = cycleIndex + timing.HitNormalizedTime;
        float timeout = Mathf.Max(3f, timing.ClipLength + 1f);
        float elapsed = 0f;

        yield return WaitUntilState(synergyLoopStateName);

        while (elapsed < timeout)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.shortNameHash != stateHash)
            {
                break;
            }

            if (stateInfo.normalizedTime >= hitThreshold)
            {
                break;
            }

            if (synergyHitTriggered
                && stateInfo.normalizedTime >= cycleIndex
                && stateInfo.normalizedTime < cycleIndex + 1f)
            {
                break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    public IEnumerator WaitForSynergyLoopCycleEnd(int cycleIndex)
    {
        if (animator == null || string.IsNullOrEmpty(synergyLoopStateName))
        {
            if (fallbackMotionDuration > 0f)
            {
                yield return new WaitForSeconds(fallbackMotionDuration);
            }

            yield break;
        }

        int stateHash = Animator.StringToHash(synergyLoopStateName);
        float cycleEnd = cycleIndex + 1f;
        AttackClipHitTiming timing = ResolveClipHitTiming(synergyLoopStateName, SynergyHitFunctionName);
        float timeout = Mathf.Max(3f, timing.ClipLength + 1f);
        float elapsed = 0f;

        while (elapsed < timeout)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.shortNameHash != stateHash)
            {
                break;
            }

            if (stateInfo.normalizedTime >= cycleEnd)
            {
                break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    public void StopSynergyMotion()
    {
        if (animator == null)
        {
            return;
        }

        animator.ResetTrigger(SynergyHash);
        if (!string.IsNullOrEmpty(idleStateName))
        {
            animator.Play(idleStateName, 0, 0f);
            animator.Update(0f);
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

    /// <summary>
    /// Attack 상태 진입 후, 클립의 OnAttackHit 이벤트(또는 이벤트 normalized time)까지 대기합니다.
    /// </summary>
    public IEnumerator WaitUntilAttackHitFrame(int attackValue)
    {
        attackHitTriggered = false;

        string stateName = GetAttackStateName(attackValue);
        if (animator == null || string.IsNullOrEmpty(stateName))
        {
            if (fallbackHitDelaySeconds > 0f)
            {
                yield return new WaitForSeconds(fallbackHitDelaySeconds);
            }

            yield break;
        }

        int stateHash = Animator.StringToHash(stateName);
        AttackClipHitTiming timing = ResolveAttackClipHitTiming(attackValue);

        yield return WaitUntilAttackState(attackValue);

        if (HasReachedHitFrame(stateHash, timing.HitNormalizedTime))
        {
            yield break;
        }

        if (timing.HasHitEvent)
        {
            while (!attackHitTriggered)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.shortNameHash != stateHash)
                {
                    break;
                }

                if (HasReachedHitFrame(stateHash, timing.HitNormalizedTime))
                {
                    break;
                }

                yield return null;
            }
        }
        else
        {
            while (true)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.shortNameHash != stateHash)
                {
                    break;
                }

                if (stateInfo.normalizedTime >= timing.HitNormalizedTime)
                {
                    break;
                }

                yield return null;
            }
        }
    }

    public float GetResolvedAttackHitDelaySeconds(int attackValue)
    {
        AttackClipHitTiming timing = ResolveAttackClipHitTiming(attackValue);
        if (timing.ClipLength > 0f)
        {
            return timing.ClipLength * timing.HitNormalizedTime;
        }

        return fallbackHitDelaySeconds;
    }

    public IEnumerator WaitForAttackAnimationComplete(int attackValue)
    {
        yield return WaitForCurrentStateFinish(GetAttackStateName(attackValue));
    }

    private void RebuildAttackClipTimingCache()
    {
        attackClipTimingCache.Clear();

        if (animator == null || animator.runtimeAnimatorController == null)
        {
            return;
        }

        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        for (int i = 0; i < clips.Length; i++)
        {
            AnimationClip clip = clips[i];
            if (clip == null || attackClipTimingCache.ContainsKey(clip.name))
            {
                continue;
            }

            attackClipTimingCache[clip.name] = BuildClipHitTiming(clip, AttackHitFunctionName);
        }
    }

    private AttackClipHitTiming BuildClipHitTiming(AnimationClip clip, string functionName)
    {
        AttackClipHitTiming timing = new AttackClipHitTiming
        {
            ClipLength = clip.length,
            HitNormalizedTime = fallbackHitNormalizedTime,
            HasHitEvent = false
        };

        AnimationEvent[] events = clip.events;
        for (int i = 0; i < events.Length; i++)
        {
            if (events[i].functionName != functionName)
            {
                continue;
            }

            timing.HasHitEvent = true;
            timing.HitNormalizedTime = clip.length > 0f
                ? Mathf.Clamp01(events[i].time / clip.length)
                : fallbackHitNormalizedTime;
            break;
        }

        return timing;
    }

    private AttackClipHitTiming ResolveAttackClipHitTiming(int attackValue)
    {
        return ResolveClipHitTiming(GetAttackStateName(attackValue), AttackHitFunctionName);
    }

    private AttackClipHitTiming ResolveClipHitTiming(string clipOrStateName, string functionName)
    {
        string cacheKey = clipOrStateName + "|" + functionName;
        if (!string.IsNullOrEmpty(clipOrStateName) && attackClipTimingCache.TryGetValue(cacheKey, out AttackClipHitTiming cachedTiming))
        {
            return cachedTiming;
        }

        if (!string.IsNullOrEmpty(clipOrStateName) && attackClipTimingCache.TryGetValue(clipOrStateName, out cachedTiming)
            && functionName == AttackHitFunctionName)
        {
            return cachedTiming;
        }

        AttackClipHitTiming timing = FindClipHitTiming(clipOrStateName, functionName);
        if (!string.IsNullOrEmpty(clipOrStateName))
        {
            attackClipTimingCache[cacheKey] = timing;
        }

        return timing;
    }

    private AttackClipHitTiming FindClipHitTiming(string clipOrStateName, string functionName)
    {
        if (animator == null || animator.runtimeAnimatorController == null || string.IsNullOrEmpty(clipOrStateName))
        {
            return CreateFallbackHitTiming();
        }

        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        for (int i = 0; i < clips.Length; i++)
        {
            AnimationClip clip = clips[i];
            if (clip == null || clip.name != clipOrStateName)
            {
                continue;
            }

            return BuildClipHitTiming(clip, functionName);
        }

        return CreateFallbackHitTiming();
    }

    private AttackClipHitTiming CreateFallbackHitTiming()
    {
        return new AttackClipHitTiming
        {
            HitNormalizedTime = fallbackHitNormalizedTime,
            HasHitEvent = false,
            ClipLength = 0f
        };
    }

    private bool HasReachedHitFrame(int stateHash, float hitNormalizedTime)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.shortNameHash == stateHash && stateInfo.normalizedTime >= hitNormalizedTime;
    }

    private IEnumerator WaitUntilState(string stateName)
    {
        if (animator == null || string.IsNullOrEmpty(stateName))
        {
            yield break;
        }

        int stateHash = Animator.StringToHash(stateName);
        float elapsed = 0f;
        const float timeoutSeconds = 2f;

        while (elapsed < timeoutSeconds)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.shortNameHash == stateHash)
            {
                break;
            }

            elapsed += Time.deltaTime;
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
