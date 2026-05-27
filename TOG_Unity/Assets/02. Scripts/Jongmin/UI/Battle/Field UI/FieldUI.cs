using UnityEngine;

public class FieldUI : MonoBehaviour, IFieldUI
{
    [SerializeField] private GameObject _previewObject;

    public void TogglePreview(bool isActive)
    {
        _previewObject.SetActive(isActive);
        _previewObject.transform.SetAsFirstSibling();
    }
}