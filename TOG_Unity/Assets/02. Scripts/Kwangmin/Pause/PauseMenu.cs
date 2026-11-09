using System;
using System.Collections.Generic;
using UnityEngine;

public static class PauseMenu
{
    private static readonly HashSet<object> Blockers = new();
    private static PauseMenuController _controller;
    private static float _timeScaleBeforePause = 1f;

    public static bool IsPaused { get; private set; }
    public static bool IsBlocked => Blockers.Count > 0;

    public static event Action<bool> OnPauseChanged;
    public static event Action OnGiveUp;

    public static bool Open()
    {
        return _controller != null && _controller.Open();
    }

    public static void Close()
    {
        if (_controller != null)
        {
            _controller.Close();
        }
    }

    public static void AddBlocker(object blocker)
    {
        if (blocker != null)
        {
            Blockers.Add(blocker);
        }
    }

    public static void RemoveBlocker(object blocker)
    {
        if (blocker != null)
        {
            Blockers.Remove(blocker);
        }
    }

    internal static void Register(PauseMenuController controller)
    {
        _controller = controller;
        SetPaused(false);
    }

    internal static void Unregister(PauseMenuController controller)
    {
        if (_controller != controller)
        {
            return;
        }

        _controller = null;
        SetPaused(false);
    }

    internal static void SetPaused(bool paused)
    {
        if (IsPaused == paused)
        {
            return;
        }

        // 스케일된 시간을 멈춰 전투 이동, 애니메이터, 파티클, DOTween을 멈춘 지점에 고정한다.
        // 일시정지 메뉴 페이드는 unscaled 트윈이라 계속 재생된다.
        if (paused)
        {
            _timeScaleBeforePause = Time.timeScale;
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = _timeScaleBeforePause;
        }

        IsPaused = paused;
        OnPauseChanged?.Invoke(paused);
    }

    internal static void RaiseGiveUp()
    {
        OnGiveUp?.Invoke();
    }
}
