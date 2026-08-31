using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class AttackButton : MonoBehaviour
{
    private const string STATE_INTRO = "Intro";
    private const string STATE_NORMAL = "Normal";
    private const string STATE_HIGHLIGHTED = "Highlighted";
    private const string STATE_SELECTED = "Selected";

    private Animator animator;
    private Button button;
    private RectTransform hitRect;
    private TurnManager turnManager;

    private bool isInside;
    private bool isPlayingIntro;
    private bool isPlayingSelected;
    private bool isHiddenUntilNextTurn;
    private Coroutine introRoutine;
    private Coroutine selectedRoutine;
    private Coroutine subscribeRoutine;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        button = GetComponent<Button>();
        hitRect = transform as RectTransform;

        if (button != null)
        {
            button.interactable = false;
        }
    }

    private void Start()
    {
        PlayIntro();

        if (subscribeRoutine == null && turnManager == null)
        {
            subscribeRoutine = StartCoroutine(SubscribeToTurnManager());
        }
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

    private void Update()
    {
        bool inside = IsPointerInside();

        if (isHiddenUntilNextTurn || isPlayingIntro || isPlayingSelected)
        {
            isInside = inside;
            return;
        }

        if (inside && WasPressedThisFrame())
        {
            isInside = inside;
            PlaySelected();
            return;
        }

        if (inside == isInside)
        {
            return;
        }

        isInside = inside;
        PlayHover();
    }

    private void PlayIntro()
    {
        StopSelected();
        ShowButton();

        if (introRoutine != null)
        {
            StopCoroutine(introRoutine);
        }

        introRoutine = StartCoroutine(IntroRoutine());
    }

    private IEnumerator IntroRoutine()
    {
        isPlayingIntro = true;
        isHiddenUntilNextTurn = false;
        PlayState(STATE_INTRO, forceRestart: true);

        yield return null;
        while (IsInState(STATE_INTRO) && GetNormalizedTime() < 1f)
        {
            yield return null;
        }

        isPlayingIntro = false;
        introRoutine = null;
        isInside = IsPointerInside();
        PlayHover();
    }

    private void PlaySelected()
    {
        StopSelected();
        selectedRoutine = StartCoroutine(SelectedRoutine());
    }

    private IEnumerator SelectedRoutine()
    {
        isPlayingSelected = true;
        PlayState(STATE_SELECTED, forceRestart: true);

        yield return null;
        if (IsInState(STATE_SELECTED))
        {
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        }

        if (button != null)
        {
            button.onClick.Invoke();
        }

        isPlayingSelected = false;
        selectedRoutine = null;
        HideUntilNextTurn();
    }

    private void HideUntilNextTurn()
    {
        isHiddenUntilNextTurn = true;
        isInside = false;
        SetVisible(false);
    }

    private void ShowButton()
    {
        isHiddenUntilNextTurn = false;
        SetVisible(true);
    }

    private void SetVisible(bool visible)
    {
        if (animator != null)
        {
            animator.enabled = visible;
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(visible);
        }
    }

    private void StopSelected()
    {
        if (selectedRoutine == null)
        {
            return;
        }

        StopCoroutine(selectedRoutine);
        selectedRoutine = null;
        isPlayingSelected = false;
    }

    private void PlayHover()
    {
        PlayState(isInside ? STATE_HIGHLIGHTED : STATE_NORMAL);
    }

    private void PlayState(string stateName, bool forceRestart = false)
    {
        if (animator == null)
        {
            return;
        }

        if (!forceRestart && IsInState(stateName) && !animator.IsInTransition(0))
        {
            return;
        }

        animator.Play(stateName, 0, 0f);
    }

    private bool IsInState(string stateName)
    {
        return animator != null && animator.GetCurrentAnimatorStateInfo(0).IsName(stateName);
    }

    private float GetNormalizedTime()
    {
        return animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }

    private bool IsPointerInside()
    {
        if (hitRect == null)
        {
            return false;
        }

        return RectTransformUtility.RectangleContainsScreenPoint(hitRect, GetPointerScreenPosition(), GetEventCamera());
    }

    private static bool WasPressedThisFrame()
    {
        if (Mouse.current != null)
        {
            return Mouse.current.leftButton.wasPressedThisFrame;
        }

        return Input.GetMouseButtonDown(0);
    }

    private static Vector2 GetPointerScreenPosition()
    {
        if (Mouse.current != null)
        {
            return Mouse.current.position.ReadValue();
        }

        return Input.mousePosition;
    }

    private Camera GetEventCamera()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            return null;
        }

        return canvas.worldCamera;
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
        PlayIntro();
    }

    public void OnAttackButtonClicked() { }
}
