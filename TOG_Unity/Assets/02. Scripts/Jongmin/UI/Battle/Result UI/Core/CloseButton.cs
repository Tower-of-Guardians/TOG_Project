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
    private bool _isReady;
    private UnityAction _clickHandlers;
    
    private const string TriggerNormal = "Normal";
    private const string TriggerHighlighted = "Highlighted";
    private const string TriggerPressed = "Pressed";
    private const string TriggerSelected = "Selected";
    private const string TriggerDisabled = "Disabled";
    private const string StateInit = "Init";
    private const string StateIntro = "Intro";
    private const string StateSelected = "Selected";

    private void Awake()
    {
        buttonModel.onClick.AddListener(HandleClick);
    }

    public void Bind(UnityAction action)
        => _clickHandlers += action;

    public void Unbind(UnityAction action)
        => _clickHandlers -= action;
    
    public void Show()
    {
        ResetInteraction();
        if (animator == null || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null)
        {
            _isReady = true;
            buttonModel.interactable = true;
            return;
        }
        
        ResetTrigger();
        buttonModel.interactable = false;
        
        animator.Play(StateIntro, 0, 0f);
        StartCoroutine(HandleIntroEnd());
    }

    public void Hide()
    {
        ResetInteraction();
        _isMouseOver = false;
        buttonModel.interactable = false;
        if (animator != null && animator.isActiveAndEnabled)
        {
            ResetTrigger();
            animator.Play(StateInit, 0, 0f);
        }
    }

    private void ResetInteraction()
    {
        StopAllCoroutines();
        _isProcessingClick = false;
        _isReady = false;
    }

    private void OnDisable()
    {
        ResetInteraction();
        _isMouseOver = false;
    }

    private void OnDestroy()
    {
        if (buttonModel != null)
        {
            buttonModel.onClick.RemoveListener(HandleClick);
        }
        _clickHandlers = null;
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
        if (!_isReady || !buttonModel.interactable || _isProcessingClick)
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
        if (!_isReady || !buttonModel.interactable || _isProcessingClick)
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
        if (!_isReady || !buttonModel.interactable || _isProcessingClick)
        {
            return;
        }

        if (_isMouseOver && animator != null)
        {
            animator.SetTrigger(TriggerPressed);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_isReady || !buttonModel.interactable || _isProcessingClick)
        {
            return;
        }

        if (!_isMouseOver && animator != null)
        {
            animator.SetTrigger(TriggerNormal);
        }
    }

    private void HandleClick()
    {
        if (!isActiveAndEnabled || !_isReady || _isProcessingClick || !buttonModel.IsInteractable())
        {
            return;
        }

        StartCoroutine(ClickRoutine());
    }
    
    private IEnumerator ClickRoutine()
    {
        _isProcessingClick = true;
        _isReady = false;
        buttonModel.interactable = false;

        if (animator != null && animator.isActiveAndEnabled && animator.runtimeAnimatorController != null)
        {
            ResetTrigger();
            animator.Play(StateSelected, 0, 0f);

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
        }

        buttonModel.interactable = false;
        _clickHandlers?.Invoke();
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
        _isReady = true;
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

}
