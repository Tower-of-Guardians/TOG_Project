using UnityEngine;

public class NotifierUI : MonoBehaviour, INotifierUI
{
    [Header("Object References")]
    [SerializeField] private GameObject noticePrefab;

    public void Notify(string notifyString)
    {
        var popupNoticeObject = ObjectPoolManager.Instance.Get(noticePrefab);

        var popupNoticeUI = popupNoticeObject.GetComponent<IPopupNotifierUI>();
        popupNoticeUI.OpenUI(notifyString);
    }
}
