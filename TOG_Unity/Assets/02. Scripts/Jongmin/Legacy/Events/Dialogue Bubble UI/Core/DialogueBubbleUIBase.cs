using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public abstract class DialogueBubbleUIBase : MonoBehaviour, IDialogueBubbleUI
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("Text References")]
    [SerializeField] private TMP_Text dialogueLabel;
    [SerializeField] private float charPerSeconds = 10f;
    
    [Header("Animation References")]
    [SerializeField] private float animationDuration = 0.5f;
    
    private Coroutine _typingCoroutine;

    public virtual void OpenUI()
        => ToggleUI(true);

    public virtual void CloseUI()
        => ToggleUI(false);
    
    protected virtual void ToggleUI(bool active)
    {
        canvasGroup.DOKill();
        canvasGroup.DOFade(active ? 1f : 0f, animationDuration);
    }

    /// <summary>
    /// 말풍선의 텍스트를 타이핑 효과와 함께 설정합니다.
    /// </summary>
    public virtual void SetBubble(string dialogueString)
    {
        if(_typingCoroutine != null)
            StopCoroutine(_typingCoroutine);

        _typingCoroutine = StartCoroutine(TypeRoutine(dialogueString));
    }
    
    protected virtual IEnumerator TypeRoutine(string dialogueString)
    {
        dialogueLabel.text = string.Empty;

        float typingInterval = 1f / charPerSeconds;
        int stringIndex = 0;
        
        while (stringIndex < dialogueString.Length)
        {
            if (dialogueString[stringIndex] == '<')
            {
                var tagEndIndex = dialogueString.IndexOf('>', stringIndex);
                if (tagEndIndex == -1)
                    break;

                string tag = dialogueString.Substring(stringIndex, tagEndIndex - stringIndex + 1);
                dialogueLabel.text += tag;

                stringIndex = tagEndIndex + 1;
                continue;
            }

            dialogueLabel.text += dialogueString[stringIndex];
            stringIndex++;

            yield return new WaitForSeconds(typingInterval);
        }
    }
}
