using System;
using UnityEngine;

public class AreaEventSubUI : MonoBehaviour
{
    [SerializeField] private GameObject _obPanel;
    [SerializeField] private UnityEngine.UI.Button _nextButton;

    private Action _onNext;
    private Func<Action, bool> _onInteract;
    private GameObject[] _npcRoots;
    private bool[] _npcWasActive;

    public bool IsOpen { get; private set; }

    public bool IsUIOpen()
    {
        return isActiveAndEnabled && IsOpen;
    }

    public void Bind(Action onNext, GameObject[] npcRoots, Func<Action, bool> onInteract = null)
    {
        _onNext = onNext;
        _onInteract = onInteract;
        _npcRoots = npcRoots;
        if (_nextButton != null)
        {
            _nextButton.onClick.RemoveListener(Next);
            _nextButton.onClick.AddListener(Next);
            _nextButton.gameObject.SetActive(!IsOpen && _onNext != null);
        }
    }

    public void Next()
    {
        if (IsOpen || !isActiveAndEnabled) return;
        _onNext?.Invoke();
    }

    public void Interact()
    {
        if (IsOpen || !isActiveAndEnabled) return;
        if (_onInteract != null)
        {
            _onInteract.Invoke(Open);
            return;
        }
        Open();
    }

    public virtual void Open()
    {
        Open(true);
    }

    protected void Open(bool showPanel)
    {
        if (IsOpen) return;
        IsOpen = true;
        if (_nextButton != null) _nextButton.gameObject.SetActive(false);

        if (_npcRoots != null)
        {
            _npcWasActive = new bool[_npcRoots.Length];
            for (int i = 0; i < _npcRoots.Length; i++)
            {
                if (_npcRoots[i] == null) continue;
                _npcWasActive[i] = _npcRoots[i].activeSelf;
                _npcRoots[i].SetActive(false);
            }
        }

        if (_obPanel != null)
        {
            _obPanel.SetActive(showPanel);
        }
    }

    public virtual void Close()
    {
        if (!IsOpen) return;
        IsOpen = false;

        if (_obPanel != null)
        {
            _obPanel.SetActive(false);
        }

        if (_npcRoots != null && _npcWasActive != null)
        {
            for (int i = 0; i < _npcRoots.Length; i++)
            {
                if (_npcRoots[i] != null) _npcRoots[i].SetActive(_npcWasActive[i]);
            }
        }

        if (_nextButton != null) _nextButton.gameObject.SetActive(_onNext != null);
    }
}
