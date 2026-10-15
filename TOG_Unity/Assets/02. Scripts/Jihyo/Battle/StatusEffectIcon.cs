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
    public const string IconBuffDebuffPrefabPath = "Assets/03. Prefabs/Battle/Icon_BuffDebuff.prefab";

    [SerializeField] private SpriteRenderer iconRenderer;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private float fontSizeMin = 0.1f;
    [SerializeField] private float fontSizeMax = 0.2f;

    public TMP_Text AmountText => amountText;

    private static GameObject sharedPrefab;
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

    public static void RegisterPrefab(GameObject prefab)
    {
        if (prefab != null)
        {
            sharedPrefab = prefab;
        }
    }

    public static GameObject ResolvePrefab(GameObject preferred)
    {
        if (preferred != null)
        {
            return preferred;
        }

        if (sharedPrefab != null)
        {
            return sharedPrefab;
        }

#if UNITY_EDITOR
        sharedPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(IconBuffDebuffPrefabPath);
#endif
        return sharedPrefab;
    }

    public static StatusEffectIcon Create(Transform parent, string objectName, Sprite iconSprite, GameObject prefab)
    {
        GameObject resolvedPrefab = ResolvePrefab(prefab);
        GameObject root = resolvedPrefab != null
            ? Object.Instantiate(resolvedPrefab, parent)
            : CreateFallbackRoot(parent);
        root.name = objectName;

        StatusEffectIcon icon = Resolve(root);
        icon.SetIcon(iconSprite);
        icon.SetAmount(string.Empty);
        return icon;
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

        if (amountText == null)
        {
            Transform textTransform = transform.Find(AmountTextName);
            if (textTransform != null)
            {
                amountText = textTransform.GetComponent<TMP_Text>();
            }
        }

        if (amountText == null)
        {
            amountText = GetComponentInChildren<TMP_Text>(true);
        }
    }

    private static GameObject CreateFallbackRoot(Transform parent)
    {
        GameObject root = new GameObject("StatusEffectIcon", typeof(RectTransform), typeof(Image));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.SetParent(parent, false);
        rootRect.localScale = Vector3.one;
        rootRect.sizeDelta = new Vector2(0.3f, 0.3f);

        GameObject textObject = new GameObject(AmountTextName, typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.SetParent(rootRect, false);
        textRect.anchorMin = new Vector2(1f, 0f);
        textRect.anchorMax = new Vector2(1f, 0f);
        textRect.pivot = new Vector2(1f, 0f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = new Vector2(0.3f, 0.3f);

        TextMeshProUGUI amount = textObject.GetComponent<TextMeshProUGUI>();
        amount.font = TMP_Settings.defaultFontAsset;
        amount.color = Color.white;
        amount.alignment = TextAlignmentOptions.BottomRight;
        amount.enableAutoSizing = true;
        amount.fontSizeMin = 0.1f;
        amount.fontSizeMax = 0.2f;
        amount.raycastTarget = false;

        StatusEffectIcon icon = root.AddComponent<StatusEffectIcon>();
        icon.AssignAmountText(amount);
        return root;
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
