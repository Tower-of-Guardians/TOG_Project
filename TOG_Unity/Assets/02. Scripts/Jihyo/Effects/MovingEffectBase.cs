using UnityEngine;
using DG.Tweening;
using DG.Tweening.Core;

/// <summary>
/// 이동 이펙트용 컴포넌트
/// 타겟에서 시전자로 이동하는 이펙트에 사용됩니다.
/// </summary>
public class MovingEffectBase : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float moveDuration = 1f;
    [SerializeField] private Ease moveEase = Ease.InOutQuad;
    [SerializeField] private bool useCurve = false; // 곡선 이동 사용 여부
    [SerializeField] private float curveHeight = 2f; // 곡선 높이 (useCurve가 true일 때)

    private Transform fromTarget;
    private Transform toTarget;
    private Vector3 fromPosition;
    private Vector3 toPosition;
    private bool useTransformTargets = false;
    private Tween moveTweener;

    // Transform 기반으로 초기화
    public void Initialize(Transform from, Transform to)
    {
        if (from == null || to == null)
        {
            Debug.LogError($"MovingEffect: from 또는 to Transform이 null입니다. ({gameObject.name})", this);
            return;
        }

        fromTarget = from;
        toTarget = to;
        useTransformTargets = true;
        transform.position = from.position;
        
        StartMovement();
    }

    // Vector3 위치 기반으로 초기화
    public void Initialize(Vector3 from, Vector3 to)
    {
        fromPosition = from;
        toPosition = to;
        useTransformTargets = false;
        transform.position = from;
        
        StartMovement();
    }

    private void StartMovement()
    {
        if (moveTweener != null && moveTweener.IsActive())
        {
            moveTweener.Kill();
        }

        Vector3 startPos = useTransformTargets ? fromTarget.position : fromPosition;
        Vector3 endPos = useTransformTargets ? toTarget.position : toPosition;

        if (useCurve)
        {
            // 곡선 이동 (DOJump 사용 - Sequence 반환)
            Sequence jumpSequence = transform.DOJump(endPos, curveHeight, 1, moveDuration)
                .SetEase(moveEase)
                .OnComplete(OnMovementComplete);
            moveTweener = jumpSequence;
        }
        else
        {
            // 직선 이동
            moveTweener = transform.DOMove(endPos, moveDuration)
                .SetEase(moveEase)
                .OnComplete(OnMovementComplete);
        }
    }

    private void OnMovementComplete()
    {
        // 이동 완료 후 추가 처리가 필요하면 작성
        // AutoReturnEffect가 있으면 자동으로 반환됨
    }

    private void Update()
    {
        // Transform 기반인 경우 실시간으로 목표 위치 업데이트
        if (useTransformTargets && toTarget != null && moveTweener != null && moveTweener.IsActive())
        {
            Vector3 currentEndPos = toTarget.position;
            // DOTween의 endValue를 업데이트하는 것은 직접 지원하지 않으므로
            // 필요시 재시작하거나 다른 방식 사용
        }
    }

    private void OnDisable()
    {
        if (moveTweener != null && moveTweener.IsActive())
        {
            moveTweener.Kill();
        }
    }

    private void OnDestroy()
    {
        if (moveTweener != null && moveTweener.IsActive())
        {
            moveTweener.Kill();
        }
    }
}
