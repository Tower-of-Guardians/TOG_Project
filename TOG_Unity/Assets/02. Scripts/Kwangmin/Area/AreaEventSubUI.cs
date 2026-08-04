using UnityEngine;

public class AreaEventSubUI : MonoBehaviour
{
    [SerializeField] private GameObject _obPanel;

    public virtual void Open()
    {
        if (_obPanel != null)
        {
            _obPanel.SetActive(true);
        }
    }

    public virtual void Close()
    {
        if (_obPanel != null)
        {
            _obPanel.SetActive(false);
        }
    }
}
