using System;
using UnityEngine;

public class AreaEventSubUI : MonoBehaviour
{
    [SerializeField] private GameObject _obPanel;

    private Action _onClosed;
    private GameObject[] _npcRoots;
    private bool[] _npcWasActive;

    public bool IsOpen { get; private set; }

    public void Bind(Action onClosed, GameObject[] npcRoots)
    {
        _onClosed = onClosed;
        _npcRoots = npcRoots;
    }

    public virtual void Open()
    {
        Open(true);
    }

    protected void Open(bool showPanel)
    {
        if (IsOpen) return;
        IsOpen = true;

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

        _onClosed?.Invoke();
    }
}
