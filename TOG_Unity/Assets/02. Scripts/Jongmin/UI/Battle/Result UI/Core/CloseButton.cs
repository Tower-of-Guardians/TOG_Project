using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CloseButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Button buttonModel;
    [SerializeField] private Animator animator;
    
    private bool _isMouseOver;
    private bool _isProcessingClick;
    private Coroutine _preventDisabledAnimationCoroutine;
    
    private const string TriggerNormal = "Normal";
    private const string TriggerHighlighted = "Highlighted";
    private const string TriggerPressed = "Pressed";
    private const string TriggerSelected = "Selected";
    private const string TriggerDisabled = "Disabled";
    private const string StateIntro = "Intro";
    private const string StateSelected = "Selected";

    public void Bind(UnityAction action)
        => buttonModel.onClick.AddListener(action);

    public void Unbind(UnityAction action)
        => buttonModel.onClick.RemoveListener(action);
    
    public void Show()
    {
        if (animator == null)
        {
            return;
        }
        
        StopAllCoroutines();
        _isProcessingClick = false;

        if (_preventDisabledAnimationCoroutine != null)
        {
            StopCoroutine(_preventDisabledAnimationCoroutine);
            _preventDisabledAnimationCoroutine = null;
        }
        
        ResetTrigger();
        buttonModel.interactable = false;
        
        animator.Play(StateIntro, 0, 0f);
        StartCoroutine(HandleIntroEnd());
    }

    public void Hide()
    {
        animator.SetTrigger(TriggerNormal);
    }

    private void ResetTrigger()
    {
        animator.ResetTrigger(TriggerNormal);
        animator.ResetTrigger(TriggerHighlighted);
        animator.ResetTrigger(TriggerPressed);
        animator.ResetTrigger(TriggerSelected);
        animator.ResetTrigger(TriggerDisabled);        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isMouseOver = true;
        if (!buttonModel.interactable || _isProcessingClick)
        {
            return;
        }

        if (animator != null && animator.GetCurrentAnimatorStateInfo(0).IsName("Normal"))
        {
            animator.SetTrigger(TriggerHighlighted);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isMouseOver = false;
        if (!buttonModel.interactable || _isProcessingClick)
        {
            return;
        }

        if (animator != null && animator.GetCurrentAnimatorStateInfo(0).IsName("Highlighted"))
        {
            animator.SetTrigger(TriggerNormal);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!buttonModel.interactable || _isProcessingClick)
        {
            return;
        }

        if (_isMouseOver)
        {
            animator.SetTrigger(TriggerPressed);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!buttonModel.interactable || _isProcessingClick)
        {
            return;
        }

        if (_isMouseOver)
        {
            StartCoroutine(ClickRoutine());
        }
        else
        {
            animator.SetTrigger(TriggerNormal);
        }
    }
    
    private IEnumerator ClickRoutine()
    {
        _isProcessingClick = true;
        
        animator.SetTrigger(TriggerSelected);
        
        float elapsedTime = 0f;
        while (elapsedTime < 0.5f && !animator.GetCurrentAnimatorStateInfo(0).IsName(StateSelected))
        {
            yield return null;
            elapsedTime += Time.deltaTime;
        }

        if (animator.GetCurrentAnimatorStateInfo(0).IsName(StateSelected))
        {
            float selectedLength = animator.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(selectedLength);            
        }
        
        if (_preventDisabledAnimationCoroutine != null)
        {
            StopCoroutine(_preventDisabledAnimationCoroutine);
        }

        _preventDisabledAnimationCoroutine = StartCoroutine(PreventDisableRoutine());
        buttonModel.onClick.Invoke();
        buttonModel.interactable = false;
    }

    private IEnumerator HandleIntroEnd()
    {
        if (animator == null)
        {
            buttonModel.interactable = false;
            yield break;
        }

        float elapsedTime = 0f;
        while (elapsedTime < 0.2f && !animator.GetCurrentAnimatorStateInfo(0).IsName(StateIntro))
        {
            yield return null;
            elapsedTime += Time.deltaTime;
        }

        if (!animator.GetCurrentAnimatorStateInfo(0).IsName(StateIntro))
        {
            animator.Play(StateIntro, 0, 0f);
            yield return null;
        }

        while (animator.GetCurrentAnimatorStateInfo(0).IsName(StateIntro))
        {
            animator.ResetTrigger(TriggerDisabled);
            yield return null;
        }
        
        yield return new WaitUntil(() => !animator.GetCurrentAnimatorStateInfo(0).IsName(StateIntro));
        
        animator.ResetTrigger(TriggerDisabled);
        buttonModel.interactable = true;

        if (_isMouseOver)
        {
            animator.SetTrigger(TriggerHighlighted);
        }
        else
        {
            animator.SetTrigger(TriggerNormal);
        }
    }

    private IEnumerator PreventDisableRoutine()
    {
        while (buttonModel != null && !buttonModel.interactable)
        {
            if (animator != null)
            {
                animator.ResetTrigger(TriggerDisabled);
            }

            yield return null;
        }

        _preventDisabledAnimationCoroutine = null;
    }
}
