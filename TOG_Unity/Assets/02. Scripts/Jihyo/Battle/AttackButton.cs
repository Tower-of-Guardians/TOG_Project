using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AttackButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private Animator animator;
    private Button button;
    private TurnManager turnManager;

    private bool isMouseOver = false;
    private bool isProcessingClick = false;
    private Coroutine preventDisabledAnimationCoroutine;

    private const string TRIGGER_NORMAL = "Normal";
    private const string TRIGGER_HIGHLIGHTED = "Highlighted";
    private const string TRIGGER_PRESSED = "Pressed";
    private const string TRIGGER_SELECTED = "Selected";
    private const string TRIGGER_DISABLED = "Disabled";
    private const string STATE_INTRO = "Intro";
    private const string STATE_SELECTED = "Selected";

    private void Awake()
    {
        animator = GetComponent<Animator>();
        button = GetComponent<Button>();
    }

    private void Start()
    {
        PlayIntroAnimation();
        
        // Start에서도 TurnManager 구독 시도 (OnEnable에서 실패했을 경우를 대비)
        if (turnManager == null)
        {
            StartCoroutine(SubscribeToTurnManager());
        }
    }

    private void PlayIntroAnimation()
    {
        if (animator == null) return;
        
        StopAllCoroutines();
        isProcessingClick = false;
        
        if (preventDisabledAnimationCoroutine != null)
        {
            StopCoroutine(preventDisabledAnimationCoroutine);
            preventDisabledAnimationCoroutine = null;
        }
        
        animator.ResetTrigger(TRIGGER_NORMAL);
        animator.ResetTrigger(TRIGGER_HIGHLIGHTED);
        animator.ResetTrigger(TRIGGER_PRESSED);
        animator.ResetTrigger(TRIGGER_SELECTED);
        animator.ResetTrigger(TRIGGER_DISABLED);
        
        button.interactable = false;
        
        animator.Play(STATE_INTRO, 0, 0f);
        StartCoroutine(HandleIntroEnd());
    }

    private IEnumerator HandleIntroEnd()
    {
        if (animator == null)
        {
            button.interactable = true;
            yield break;
        }

        float waitTime = 0f;
        while (waitTime < 0.2f && !animator.GetCurrentAnimatorStateInfo(0).IsName(STATE_INTRO))
        {
            yield return null;
            waitTime += Time.deltaTime;
        }

        if (!animator.GetCurrentAnimatorStateInfo(0).IsName(STATE_INTRO))
        {
            animator.Play(STATE_INTRO, 0, 0f);
            yield return null;
        }

        while (animator.GetCurrentAnimatorStateInfo(0).IsName(STATE_INTRO))
        {
            animator.ResetTrigger(TRIGGER_DISABLED);
            yield return null;
        }

        yield return new WaitUntil(() => !animator.GetCurrentAnimatorStateInfo(0).IsName(STATE_INTRO));

        animator.ResetTrigger(TRIGGER_DISABLED);
        button.interactable = true;
        
        if (isMouseOver)
        {
            animator.SetTrigger(TRIGGER_HIGHLIGHTED);
        }
        else
        {
            animator.SetTrigger(TRIGGER_NORMAL);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isMouseOver = true;
        if (!button.interactable || isProcessingClick) return;
        
        if (animator != null && animator.GetCurrentAnimatorStateInfo(0).IsName("Normal"))
        {
            animator.SetTrigger(TRIGGER_HIGHLIGHTED);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isMouseOver = false;
        if (!button.interactable || isProcessingClick) return;
        
        if (animator != null && animator.GetCurrentAnimatorStateInfo(0).IsName("Highlighted"))
        {
            animator.SetTrigger(TRIGGER_NORMAL);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!button.interactable || isProcessingClick) return;
        
        if (isMouseOver)
        {
            animator.SetTrigger(TRIGGER_PRESSED);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!button.interactable || isProcessingClick) return;

        if (isMouseOver)
        {
            StartCoroutine(ClickSequence());
        }
        else
        {
            animator.SetTrigger(TRIGGER_NORMAL);
        }
    }

    private IEnumerator ClickSequence()
    {
        isProcessingClick = true;
        
        animator.SetTrigger(TRIGGER_SELECTED);
        
        float waitTime = 0f;
        while (waitTime < 0.5f && !animator.GetCurrentAnimatorStateInfo(0).IsName(STATE_SELECTED))
        {
            yield return null;
            waitTime += Time.deltaTime;
        }
        
        if (animator.GetCurrentAnimatorStateInfo(0).IsName(STATE_SELECTED))
        {
            float selectedLength = animator.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(selectedLength);
        }
        
        button.interactable = false;
        
        if (preventDisabledAnimationCoroutine != null)
        {
            StopCoroutine(preventDisabledAnimationCoroutine);
        }
        preventDisabledAnimationCoroutine = StartCoroutine(PreventDisabledAnimation());

        button.onClick.Invoke();
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
        StartCoroutine(SubscribeToTurnManager());
    }

    private void OnDisable()
    {
        UnsubscribeFromTurnManager();
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
        
        if (!DIContainer.IsRegistered<TurnManager>())
        {
            turnManager = FindFirstObjectByType<TurnManager>();
            
            if (turnManager != null)
            {
                turnManager.OnStartNewTurn += OnTurnStart;
            }
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
}
