using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAnimation : MonoBehaviour
{
    public const string AttackHitFunctionName = "OnAttackHit";
    public const string ActionApplyFunctionName = "OnActionApply";

    [Header("Animation")]
    private Animator animator;

    [Header("Animator Parameters")]
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int HitHash = Animator.StringToHash("Hit");
    private static readonly int DeadHash = Animator.StringToHash("Dead");
    private static readonly int CurseHash = Animator.StringToHash("Curse");
    private static readonly int BuffHash = Animator.StringToHash("Buff");
    private static readonly int DefenseHash = Animator.StringToHash("Defense");
    private static readonly int SummonHash = Animator.StringToHash("Summon");

    private static readonly string[] AttackStateNames =
    {
        "Attack", "WhiteDog_Attack", "Clonier_Attack", "ClonierClone_Attack", "Grounder_Attack", "Flightmare_Attack", "Uranodon_Attack"
    };

    private static readonly string[] HitStateNames =
    {
        "Hit", "WhiteDog_Hit", "Clonier_Hit", "ClonierClone_Hit", "Grounder_Hit", "Flightmare_Hit", "Uranodon_Hit"
    };

    private static readonly string[] DeadStateNames =
    {
        "Dead", "WhiteDog_Dead", "Clonier_Dead", "ClonierClone_Dead", "Grounder_Dead", "Flightmare_Dead", "Uranodon_Death"
    };

    private readonly Dictionary<string, AttackClipHitTiming> attackClipTimingCache = new Dictionary<string, AttackClipHitTiming>();
    private bool applyFrameTriggered;

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
        applyFrameTriggered = true;
    }

    /// <summary>
    /// 비공격 행동 클립 Animation Event에서 호출됩니다.
    /// </summary>
    public void OnActionApply()
    {
        applyFrameTriggered = true;
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

    public void PlayBuffAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(BuffHash);
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
            animator.ResetTrigger(BuffHash);
            animator.ResetTrigger(DefenseHash);
            animator.ResetTrigger(SummonHash);
        }

        applyFrameTriggered = false;
    }

    /// <summary>
    /// 행동 클립의 적용 이벤트(OnAttackHit / OnActionApply)까지 대기합니다.
    /// </summary>
    public IEnumerator WaitUntilActionApplyFrame(MonsterActionType actionType)
    {
        string eventName = GetApplyEventName(actionType);
        string[] stateNames = GetApplyWaitStateNames(actionType);
        if (string.IsNullOrEmpty(eventName) || stateNames == null || stateNames.Length == 0)
        {
            yield break;
        }

        applyFrameTriggered = false;

        if (animator == null)
        {
            yield break;
        }

        string stateName = null;
        float elapsedTime = 0f;
        const float transitionTimeout = 1f;

        while (elapsedTime < transitionTimeout)
        {
            stateName = FindMatchingStateName(animator.GetCurrentAnimatorStateInfo(0), stateNames);
            if (stateName != null)
            {
                break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (string.IsNullOrEmpty(stateName))
        {
            yield break;
        }

        AttackClipHitTiming timing = ResolveClipHitTiming(stateName, eventName);
        if (!timing.HasHitEvent)
        {
            yield break;
        }

        int stateHash = Animator.StringToHash(stateName);
        if (HasReachedHitFrame(stateHash, timing.HitNormalizedTime))
        {
            yield break;
        }

        float timeout = timing.ClipLength + 0.5f;
        float waitElapsed = 0f;

        while (waitElapsed < timeout)
        {
            if (applyFrameTriggered)
            {
                break;
            }

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.shortNameHash != stateHash)
            {
                break;
            }

            if (stateInfo.normalizedTime >= timing.HitNormalizedTime)
            {
                break;
            }

            waitElapsed += Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>
    /// 공격 애니메이션이 완료될 때까지 대기합니다.
    /// </summary>
    public IEnumerator WaitForAttackAnimationComplete()
    {
        yield return WaitForAnimationComplete(AttackStateNames);
    }

    public IEnumerator WaitForHitAnimationComplete()
    {
        yield return WaitForAnimationComplete(HitStateNames);
    }

    public IEnumerator WaitForDeadAnimationComplete()
    {
        yield return WaitForAnimationComplete(DeadStateNames);
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

    private static string GetApplyEventName(MonsterActionType actionType)
    {
        switch (actionType)
        {
            case MonsterActionType.Attack:
            case MonsterActionType.Heal:
                return AttackHitFunctionName;
            case MonsterActionType.Guard:
            case MonsterActionType.ApplyStatus:
            case MonsterActionType.Summon:
                return ActionApplyFunctionName;
            default:
                return null;
        }
    }

    private static string[] GetApplyWaitStateNames(MonsterActionType actionType)
    {
        if (actionType == MonsterActionType.Heal)
        {
            return AttackStateNames;
        }

        return GetStateNamesByActionType(actionType);
    }

    private static string[] GetStateNamesByActionType(MonsterActionType actionType)
    {
        switch (actionType)
        {
            case MonsterActionType.Attack:
                return AttackStateNames;
            case MonsterActionType.Guard:
                return new[] { "Defense", "WhiteDog_Defense", "Uranodon_Defense" };
            case MonsterActionType.ApplyStatus:
                return new[] { "Curse", "WhiteDog_Curse", "Grounder_Debuff", "Uranodon_Buff" };
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
        return FindMatchingStateName(stateInfo, stateNames) != null;
    }

    private static string FindMatchingStateName(AnimatorStateInfo stateInfo, string[] stateNames)
    {
        if (stateNames == null)
        {
            return null;
        }

        for (int i = 0; i < stateNames.Length; i++)
        {
            if (stateInfo.IsName(stateNames[i]))
            {
                return stateNames[i];
            }
        }

        return null;
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
            if (clip == null)
            {
                continue;
            }

            CacheClipEventTiming(clip, AttackHitFunctionName);
            CacheClipEventTiming(clip, ActionApplyFunctionName);
        }
    }

    private void CacheClipEventTiming(AnimationClip clip, string functionName)
    {
        attackClipTimingCache[BuildTimingCacheKey(clip.name, functionName)] = BuildClipHitTiming(clip, functionName);
    }

    private AttackClipHitTiming ResolveClipHitTiming(string clipOrStateName, string functionName)
    {
        string cacheKey = BuildTimingCacheKey(clipOrStateName, functionName);
        if (!string.IsNullOrEmpty(clipOrStateName) && attackClipTimingCache.TryGetValue(cacheKey, out AttackClipHitTiming cachedTiming))
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

    private static AttackClipHitTiming BuildClipHitTiming(AnimationClip clip, string functionName)
    {
        AttackClipHitTiming timing = new AttackClipHitTiming
        {
            ClipLength = clip.length,
            HitNormalizedTime = 1f,
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
                : 1f;
            break;
        }

        return timing;
    }

    private static string BuildTimingCacheKey(string clipName, string functionName)
    {
        return clipName + "|" + functionName;
    }

    private static AttackClipHitTiming CreateFallbackHitTiming()
    {
        return new AttackClipHitTiming
        {
            HitNormalizedTime = 1f,
            HasHitEvent = false,
            ClipLength = 0f
        };
    }

    private bool HasReachedHitFrame(int stateHash, float hitNormalizedTime)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.shortNameHash == stateHash && stateInfo.normalizedTime >= hitNormalizedTime;
    }
}

