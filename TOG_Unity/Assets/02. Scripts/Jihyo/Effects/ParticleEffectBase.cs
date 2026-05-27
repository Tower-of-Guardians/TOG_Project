using UnityEngine;

/// <summary>
/// ParticleSystem 기반 이동 이펙트의 베이스 클래스
/// 파티클을 직접 조작하여 이동하는 이펙트에 사용됩니다.
/// </summary>
public abstract class ParticleEffectBase : MonoBehaviour
{
    [Header("파티클 시스템")]
    [SerializeField] protected ParticleSystem ps;

    protected Transform fromTarget;
    protected Transform toTarget;
    protected Vector3 fromPosition;
    protected Vector3 toPosition;
    protected bool useTransformTargets = false;
    protected ParticleSystem.Particle[] particles;

    protected virtual void Awake()
    {
        if (ps == null)
        {
            ps = GetComponentInChildren<ParticleSystem>();
        }
    }

    protected virtual void Start()
    {
        if (ps != null)
        {
            particles = new ParticleSystem.Particle[ps.main.maxParticles];
        }
    }

    // Transform 기반으로 초기화
    public virtual void Initialize(Transform from, Transform to)
    {
        if (from == null || to == null)
        {
            Debug.LogError($"ParticleMovingEffect: from 또는 to Transform이 null입니다. ({gameObject.name})", this);
            return;
        }

        fromTarget = from;
        toTarget = to;
        useTransformTargets = true;
        
        // 시작 위치로 이동
        transform.position = from.position;
        
        // ParticleSystem이 없으면 찾기 (자식 포함)
        if (ps == null)
        {
            ps = GetComponentInChildren<ParticleSystem>();
        }
        
        // 파티클 배열 초기화
        if (ps != null && particles == null)
        {
            particles = new ParticleSystem.Particle[ps.main.maxParticles];
        }
    }

    /// <summary>
    // Vector3 위치 기반으로 초기화
    /// </summary>
    public virtual void Initialize(Vector3 from, Vector3 to)
    {
        fromPosition = from;
        toPosition = to;
        useTransformTargets = false;
        
        // 시작 위치로 이동
        transform.position = from;
        
        // ParticleSystem이 없으면 찾기 (자식 포함)
        if (ps == null)
        {
            ps = GetComponentInChildren<ParticleSystem>();
        }
        
        // 파티클 배열 초기화
        if (ps != null && particles == null)
        {
            particles = new ParticleSystem.Particle[ps.main.maxParticles];
        }
    }

    protected virtual void LateUpdate()
    {
        if (ps == null || particles == null) return;

        // Transform 기반인 경우 타겟이 파괴되었는지 확인
        if (useTransformTargets)
        {
            if (fromTarget == null || toTarget == null)
            {
                return;
            }
        }

        // 파생 클래스에서 구현할 파티클 업데이트 로직 호출
        UpdateParticles();
    }

    /// 파티클을 업데이트하는 추상 메서드
    protected abstract void UpdateParticles();

    /// 현재 시작 위치를 가져오기
    protected Vector3 GetStartPosition()
    {
        return useTransformTargets ? fromTarget.position : fromPosition;
    }

    /// 현재 목표 위치를 가져오기
    protected Vector3 GetTargetPosition()
    {
        return useTransformTargets ? toTarget.position : toPosition;
    }
}
