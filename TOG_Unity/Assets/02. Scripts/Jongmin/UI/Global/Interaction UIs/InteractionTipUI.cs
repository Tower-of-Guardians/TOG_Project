using DG.Tweening;
using JxModule;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class InteractionTipUI : MonoBehaviour
{
    [BigHeader("UI")]
    [SerializeField] private Transform tagRoot;
    
    [Space(20f), BigHeader("Tag List")]
    [SerializeField] private string[] interactionTags;

    private CanvasGroup _canvasGroup;
    private Tween _fadeTween;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();

        var prefab = PrefabManager.CachePrefab<InteractionTag>();
        foreach (var interactionTag in interactionTags)
        {
            var tagObj = Instantiate(prefab, tagRoot);
            var targetTag = tagObj.GetComponent<InteractionTag>();
            targetTag.SetTag(interactionTag);
        }
        
        _canvasGroup.Hide();
    }

    public void Show()
    {
        _fadeTween?.Kill();
        _fadeTween = _canvasGroup.DOFade(1f, 0.25f);
    }

    public void Hide()
    {
        _fadeTween?.Kill();
        _fadeTween = _canvasGroup.DOFade(0f, 0.25f);
    }
}