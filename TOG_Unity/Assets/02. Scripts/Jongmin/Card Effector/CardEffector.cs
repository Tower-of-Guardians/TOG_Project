using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(TemporaryCardController))]
public abstract class CardEffector : MonoBehaviour
{
    [Header("Start Point")]
    [FormerlySerializedAs("m_start_transform")]
    [SerializeField] protected Transform _startTransform;

    [Header("End Point")]
    [FormerlySerializedAs("m_end_transform")]
    [SerializeField] protected Transform _endTransform;

    protected TemporaryCardController _tempCardController;
    protected TemporaryCardSettings _tempCardSettings;
    protected TemporaryCardAnimeRequest _tempCardAnimeRequest;

    private void Awake()
    {
        _tempCardController = GetComponent<TemporaryCardController>();

        _tempCardSettings ??= new();
        _tempCardAnimeRequest ??= new();

        _tempCardController.OnCardAnimationBegin += OnTempCardAnimeStart;
        _tempCardController.OnCardAnimationEnd += OnTempCardAnimeEnd;
        _tempCardController.OnFinalAnimationEnd += OnFinalAnimeEnd;
    }

    private void OnDestroy()
    {
        if(_tempCardController != null)
        {
            _tempCardController.OnCardAnimationBegin -= OnTempCardAnimeStart;
            _tempCardController.OnCardAnimationEnd -= OnTempCardAnimeEnd;
            _tempCardController.OnFinalAnimationEnd -= OnFinalAnimeEnd;
        }
    }

    public virtual void Execute()
    {
        _tempCardController.Play(_tempCardAnimeRequest);
    }

    protected virtual void OnTempCardAnimeStart(BattleCardData battleCardData) {}
    protected virtual void OnTempCardAnimeEnd(BattleCardData battleCardData) {}
    protected virtual void OnFinalAnimeEnd() {}
}
