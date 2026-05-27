using System.Collections;
using UnityEngine;

/// <summary>
/// 이펙트가 끝나면 자동으로 오브젝트 풀에 반환하는 컴포넌트
/// Animator나 ParticleSystem이 끝나면 자동으로 감지하여 반환합니다.
/// </summary>
public class AutoReturnEffect : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private bool checkAnimator = true;
    [SerializeField] private bool checkParticleSystem = true;
    [SerializeField] private float delayAfterEnd = 0f; // 이펙트 종료 후 추가 대기 시간

    private Animator animator;
    private new ParticleSystem particleSystem;
    private bool isReturning = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        particleSystem = GetComponentInChildren<ParticleSystem>();

        if ((!checkAnimator || animator == null) && (!checkParticleSystem || particleSystem == null))
        {
            Debug.LogWarning($"AutoReturnEffect: Animator 또는 ParticleSystem 컴포넌트가 필요합니다. ({gameObject.name})", this);
        }

        if (checkAnimator && animator == null)
        {
            Debug.LogWarning($"AutoReturnEffect: Animator 컴포넌트를 찾을 수 없습니다. ({gameObject.name})", this);
        }

        if (checkParticleSystem && particleSystem == null)
        {
            Debug.LogWarning($"AutoReturnEffect: ParticleSystem 컴포넌트를 찾을 수 없습니다. ({gameObject.name})", this);
        }
    }

    private void OnEnable()
    {
        isReturning = false;
        
        if (checkAnimator && animator != null)
        {
            StartCoroutine(CheckAnimatorEnd());
        }

        if (checkParticleSystem && particleSystem != null)
        {
            StartCoroutine(CheckParticleSystemEnd());
        }
    }

    private IEnumerator CheckAnimatorEnd()
    {
        if (animator == null || !checkAnimator)
            yield break;

        // Animator가 활성화되어 있고 애니메이션이 재생 중인지 확인
        while (animator.enabled && animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }

        // 애니메이션이 끝났지만 루프가 설정되어 있을 수 있으므로 추가 확인
        if (animator.enabled)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.loop)
            {
                yield break;
            }
        }

        // 애니메이션 종료 후 반환
        if (!isReturning)
        {
            StartCoroutine(ReturnToPool());
        }
    }

    private IEnumerator CheckParticleSystemEnd()
    {
        if (particleSystem == null || !checkParticleSystem)
            yield break;

        // ParticleSystem이 재생 중인 동안 대기
        while (particleSystem.isPlaying)
        {
            yield return null;
        }

        // 파티클이 모두 사라질 때까지 대기 (남은 파티클의 lifetime 고려)
        if (particleSystem.main.startLifetime.constantMax > 0)
        {
            yield return new WaitForSeconds(particleSystem.main.startLifetime.constantMax);
        }

        // 파티클 종료 후 반환
        if (!isReturning)
        {
            StartCoroutine(ReturnToPool());
        }
    }

    private IEnumerator ReturnToPool()
    {
        if (isReturning)
            yield break;

        isReturning = true;

        if (delayAfterEnd > 0f)
        {
            yield return new WaitForSeconds(delayAfterEnd);
        }

        // 오브젝트 풀에 반환
        if (ObjectPoolManager.Instance != null)
        {
            ObjectPoolManager.Instance.Return(gameObject);
        }
        else
        {
            Debug.LogWarning($"AutoReturnEffect: ObjectPoolManager를 찾을 수 없습니다. GameObject를 Destroy합니다. ({gameObject.name})", this);
            Destroy(gameObject);
        }
    }

    /// 수동으로 이펙트를 반환
    public void ReturnManually()
    {
        if (!isReturning)
        {
            StartCoroutine(ReturnToPool());
        }
    }
}
