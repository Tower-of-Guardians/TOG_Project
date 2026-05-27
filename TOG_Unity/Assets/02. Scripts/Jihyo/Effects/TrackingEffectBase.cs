using UnityEngine;

// 추적 이펙트용 컴포넌트
// 타겟을 따라다니는 이펙트에 사용됩니다.
public class TrackingEffectBase : MonoBehaviour
{
    [Header("추적 설정")]
    [SerializeField] private bool followPosition = true;
    [SerializeField] private bool followRotation = false;
    [SerializeField] private float followSpeed = 10f;
    [SerializeField] private Vector3 offset = Vector3.zero;

    private Transform target;
    private Vector3 targetPosition;
    private bool useTransformTarget = false;

    /// Transform 기반으로 초기화
    public void Initialize(Transform targetTransform, Vector3 positionOffset = default)
    {
        if (targetTransform == null)
        {
            Debug.LogError($"TrackingEffect: target Transform이 null입니다. ({gameObject.name})", this);
            return;
        }

        target = targetTransform;
        offset = positionOffset;
        useTransformTarget = true;
        transform.position = target.position + offset;
    }

    /// Vector3 위치 기반으로 초기화
    public void Initialize(Vector3 targetPosition, Vector3 positionOffset = default)
    {
        this.targetPosition = targetPosition;
        offset = positionOffset;
        useTransformTarget = false;
        transform.position = targetPosition + offset;
    }

    private void Update()
    {
        if (!followPosition)
            return;

        Vector3 targetPos;

        if (useTransformTarget)
        {
            if (target == null)
            {
                // 타겟이 파괴되었으면 이펙트도 반환
                ReturnEffect();
                return;
            }
            targetPos = target.position + offset;
        }
        else
        {
            targetPos = targetPosition + offset;
        }

        if (followSpeed > 0f)
        {
            // 부드럽게 따라감
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed);
        }
        else
        {
            // 즉시 따라감
            transform.position = targetPos;
        }

        if (followRotation && useTransformTarget && target != null)
        {
            transform.rotation = target.rotation;
        }
    }

    /// 타겟 업데이트
    public void UpdateTarget(Transform newTarget)
    {
        if (newTarget == null)
        {
            Debug.LogWarning($"TrackingEffect: 새로운 타겟이 null입니다. ({gameObject.name})", this);
            return;
        }

        target = newTarget;
        useTransformTarget = true;
    }

    /// 타겟 위치를 업데이트
    public void UpdateTargetPosition(Vector3 newPosition)
    {
        targetPosition = newPosition;
        useTransformTarget = false;
    }

    /// 오프셋을 업데이트
    public void UpdateOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }

    /// 이펙트를 수동으로 반환
    private void ReturnEffect()
    {
        if (ObjectPoolManager.Instance != null)
        {
            ObjectPoolManager.Instance.Return(gameObject);
        }
        else
        {
            Debug.LogWarning($"TrackingEffect: ObjectPoolManager를 찾을 수 없습니다. GameObject를 Destroy합니다. ({gameObject.name})", this);
            Destroy(gameObject);
        }
    }
}
