using System;
using System.Collections.Generic;

public static class PauseMenu
{
    private static readonly HashSet<object> Blockers = new();
    private static PauseMenuController _controller;

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

        IsPaused = paused;
        OnPauseChanged?.Invoke(paused);
    }

    internal static void RaiseGiveUp()
    {
        OnGiveUp?.Invoke();
    }
}
