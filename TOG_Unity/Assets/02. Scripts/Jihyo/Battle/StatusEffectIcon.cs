using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Icon_BuffDebuff의 스택/턴 텍스트에 Auto Size를 유지한 채 값을 적용합니다.
/// TMP는 fontSize를 직접 넣으면 enableAutoSizing이 풀립니다.
/// </summary>
public class StatusEffectIcon : MonoBehaviour
{
    public const string AmountTextName = "BuffDebuff_Text";

    [SerializeField] private SpriteRenderer iconRenderer;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private float fontSizeMin = 0.1f;
    [SerializeField] private float fontSizeMax = 0.2f;

    public TMP_Text AmountText => amountText;

    private bool hasCapturedAutoSize;

    private void Awake()
    {
        CacheReferences();
        CaptureAutoSizeFromText();
        RestoreAutoSize();
    }

    private void OnEnable()
    {
        RestoreAutoSize();
    }

    public void SetIcon(Sprite sprite)
    {
        CacheReferences();
        if (sprite == null)
        {
            return;
        }

        if (iconRenderer != null)
        {
            iconRenderer.sprite = sprite;
        }

        Image image = GetComponent<Image>();
        if (image != null)
        {
            image.sprite = sprite;
        }
    }

    public void SetAmount(string amount)
    {
        CacheReferences();
        if (amountText == null)
        {
            return;
        }

        RestoreAutoSize();
        amountText.text = amount ?? string.Empty;
        if (amountText.gameObject.activeInHierarchy)
        {
            amountText.ForceMeshUpdate();
        }

        RestoreAutoSize();
    }

    public static StatusEffectIcon Resolve(GameObject root)
    {
        if (root == null)
        {
            return null;
        }

        StatusEffectIcon icon = root.GetComponent<StatusEffectIcon>();
        if (icon == null)
        {
            icon = root.AddComponent<StatusEffectIcon>();
        }

        icon.CacheReferences();
        icon.CaptureAutoSizeFromText();
        icon.RestoreAutoSize();
        return icon;
    }

    public void AssignAmountText(TMP_Text text)
    {
        amountText = text;
        hasCapturedAutoSize = false;
        CaptureAutoSizeFromText();
        RestoreAutoSize();
    }

    private void CacheReferences()
    {
        if (iconRenderer == null)
        {
            iconRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void CaptureAutoSizeFromText()
    {
        if (hasCapturedAutoSize || amountText == null)
        {
            return;
        }

        if (amountText.fontSizeMin > 0f)
        {
            fontSizeMin = amountText.fontSizeMin;
        }

        if (amountText.fontSizeMax > 0f)
        {
            fontSizeMax = amountText.fontSizeMax;
        }

        hasCapturedAutoSize = true;
    }

    private void RestoreAutoSize()
    {
        if (amountText == null)
        {
            return;
        }

        amountText.enableAutoSizing = true;
        amountText.fontSizeMin = fontSizeMin;
        amountText.fontSizeMax = fontSizeMax;
    }
}
