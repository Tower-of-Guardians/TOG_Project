using TMPro;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PopupNotifierUI : MonoBehaviour, IPopupNotifierUI
{
    [Header("Object References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text notifyLabel;
    [SerializeField] private Animator animator;

    private void OnEnable()
        => animator.SetTrigger("Popup");

    public void OpenUI(string noticeString)
        => notifyLabel.text = noticeString;

    public void CloseUI()
        => ObjectPoolManager.Instance.Return(gameObject);
}