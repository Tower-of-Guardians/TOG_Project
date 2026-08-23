using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AttackButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private Animator animator;
    private Button button;
    private TurnManager turnManager;

    private bool isMouseOver;
    private bool isProcessingClick;
    private bool isPlayingIntro;
    private Coroutine introRoutine;
    private Coroutine clickRoutine;
    private Coroutine subscribeRoutine;
    private Coroutine preventDisabledAnimationCoroutine;

    private const string TRIGGER_NORMAL = "Normal";
    private const string TRIGGER_HIGHLIGHTED = "Highlighted";
    private const string TRIGGER_PRESSED = "Pressed";
    private const string TRIGGER_SELECTED = "Selected";
    private const string TRIGGER_DISABLED = "Disabled";
    private const string STATE_INTRO = "Intro";
    private const string STATE_NORMAL = "Normal";
    private const string STATE_HIGHLIGHTED = "Highlighted";
    private const string STATE_PRESSED = "Pressed";
    private const string STATE_SELECTED = "Selected";

    private void Awake()
    {
        animator = GetComponent<Animator>();
        button = GetComponent<Button>();
    }

    private void Start()
    {
        PlayIntroAnimation();

        if (turnManager == null && subscribeRoutine == null)
        {
            subscribeRoutine = StartCoroutine(SubscribeToTurnManager());
        }
    }

    private void PlayIntroAnimation()
    {
        if (animator == null)
        {
            return;
        }

        if (introRoutine != null)
        {
            StopCoroutine(introRoutine);
            introRoutine = null;
        }

        if (clickRoutine != null)
        {
            StopCoroutine(clickRoutine);
            clickRoutine = null;
        }

        if (preventDisabledAnimationCoroutine != null)
        {
            StopCoroutine(preventDisabledAnimationCoroutine);
            preventDisabledAnimationCoroutine = null;
        }

        isProcessingClick = false;
        isPlayingIntro = true;
        ResetAllTriggers();
        button.interactable = false;
        animator.Play(STATE_INTRO, 0, 0f);
        introRoutine = StartCoroutine(HandleIntroEnd());
    }

    private IEnumerator HandleIntroEnd()
    {
        if (animator == null)
        {
            isPlayingIntro = false;
            button.interactable = true;
            introRoutine = null;
            yield break;
        }

        float waitTime = 0f;
        while (waitTime < 0.2f && !IsInState(STATE_INTRO))
        {
            yield return null;
            waitTime += Time.deltaTime;
        }

        if (!IsInState(STATE_INTRO))
        {
            animator.Play(STATE_INTRO, 0, 0f);
            yield return null;
        }

        while (IsInState(STATE_INTRO))
        {
            animator.ResetTrigger(TRIGGER_DISABLED);
            yield return null;
        }

        animator.ResetTrigger(TRIGGER_DISABLED);
        isPlayingIntro = false;
        button.interactable = true;
        ApplyIdleVisual();
        introRoutine = null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isMouseOver = true;
        ApplyIdleVisual();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isMouseOver = false;
        ApplyIdleVisual();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!CanHandlePointer() || animator == null)
        {
            return;
        }

        ResetAllTriggers();
        animator.Play(STATE_PRESSED, 0, 0f);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!CanHandlePointer())
        {
            return;
        }

        if (isMouseOver)
        {
            clickRoutine = StartCoroutine(ClickSequence());
            return;
        }

        ApplyIdleVisual();
    }

    private IEnumerator ClickSequence()
    {
        isProcessingClick = true;

        if (animator != null)
        {
            ResetAllTriggers();
            animator.Play(STATE_SELECTED, 0, 0f);
            yield return null;

            if (IsInState(STATE_SELECTED))
            {
                yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
            }
        }

        button.interactable = false;

        if (preventDisabledAnimationCoroutine != null)
        {
            StopCoroutine(preventDisabledAnimationCoroutine);
        }

        preventDisabledAnimationCoroutine = StartCoroutine(PreventDisabledAnimation());
        button.onClick.Invoke();
        clickRoutine = null;
    }

    private IEnumerator PreventDisabledAnimation()
    {
        while (button != null && !button.interactable)
        {
            if (animator != null)
            {
                animator.ResetTrigger(TRIGGER_DISABLED);
            }

            yield return null;
        }

        preventDisabledAnimationCoroutine = null;
    }

    private void OnEnable()
    {
        if (subscribeRoutine == null && turnManager == null)
        {
            subscribeRoutine = StartCoroutine(SubscribeToTurnManager());
        }
    }

    private void OnDisable()
    {
        UnsubscribeFromTurnManager();
        subscribeRoutine = null;
    }

    private IEnumerator SubscribeToTurnManager()
    {
        float timeout = 5f;
        float elapsed = 0f;

        while (!DIContainer.IsRegistered<TurnManager>() && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        subscribeRoutine = null;
        if (!DIContainer.IsRegistered<TurnManager>())
        {
            yield break;
        }

        turnManager = DIContainer.Resolve<TurnManager>();
        if (turnManager != null)
        {
            turnManager.OnStartNewTurn += OnTurnStart;
        }
    }

    private void UnsubscribeFromTurnManager()
    {
        if (turnManager != null)
        {
            turnManager.OnStartNewTurn -= OnTurnStart;
            turnManager = null;
        }
    }

    private void OnTurnStart()
    {
        PlayIntroAnimation();
    }

    public void OnAttackButtonClicked() { }

    private bool CanHandlePointer()
    {
        return button != null && button.interactable && !isProcessingClick && !isPlayingIntro;
    }

    private void ApplyIdleVisual()
    {
        if (!CanHandlePointer() || animator == null)
        {
            return;
        }

        ResetAllTriggers();
        animator.Play(isMouseOver ? STATE_HIGHLIGHTED : STATE_NORMAL, 0, 0f);
    }

    private void ResetAllTriggers()
    {
        if (animator == null)
        {
            return;
        }

        animator.ResetTrigger(TRIGGER_NORMAL);
        animator.ResetTrigger(TRIGGER_HIGHLIGHTED);
        animator.ResetTrigger(TRIGGER_PRESSED);
        animator.ResetTrigger(TRIGGER_SELECTED);
        animator.ResetTrigger(TRIGGER_DISABLED);
    }

    private bool IsInState(string stateName)
    {
        return animator != null && animator.GetCurrentAnimatorStateInfo(0).IsName(stateName);
    }
}
