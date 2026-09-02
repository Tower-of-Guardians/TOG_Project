using JxModule;
using UnityEngine;
using UnityEngine.UI;

public class UIOutliner : MonoBehaviour
{
    [BigHeader("References")]
    [SerializeField, Required] private Image targetImage;
    [SerializeField, Required] private Material outlineMaterial;
    
    [Space(20f), BigHeader("Settings")]
    [SerializeField] private bool outlineEnable;
    [ShowIf("outlineEnable"), SerializeField, Range(0f, 2f)] private float outlineThickness = 1f;  
    [ShowIf("outlineEnable"), SerializeField] private Color outlineColor = Color.white;

    private Material runtimeOutlineMaterial;
    
    private static readonly int OutlineID = Shader.PropertyToID("_Outline");
    private static readonly int OutlineColorID = Shader.PropertyToID("_OutlineColor");
    private static readonly int OutlineThicknessID = Shader.PropertyToID("_OutlineSize");

    private void Awake()
    {
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
            if (targetImage == null)
            {
                DebugExtension.LogColor("UIOutliner: Target Image is null.", Color.red);
                enabled = false;
                return;
            }
        }

        if (outlineMaterial == null)
        {
            DebugExtension.LogColor("UIOutliner: Outline material is null.", Color.red);
            enabled = false;
        }
        
        runtimeOutlineMaterial = new Material(outlineMaterial);
        targetImage.material = runtimeOutlineMaterial;
        ApplyOutline(false);
    }

    private void OnDestroy()
    {
        if (runtimeOutlineMaterial == null)
            return;

        if (Application.isPlaying)
            Destroy(runtimeOutlineMaterial);
        else
            DestroyImmediate(runtimeOutlineMaterial);
    }

    public void Show()
    {
        if (!outlineEnable)
        {
            return;
        }
        
        ApplyOutline(true);
    }

    public void Hide()
    {
        if (!outlineEnable)
        {
            return;
        }
        
        ApplyOutline(false);
    }

    private void ApplyOutline(bool isActive)
    {
        if (runtimeOutlineMaterial == null)
            return;

        runtimeOutlineMaterial.SetFloat(OutlineID, isActive ? 1f : 0f);
        runtimeOutlineMaterial.SetColor(OutlineColorID, outlineColor);
        runtimeOutlineMaterial.SetFloat(OutlineThicknessID, outlineThickness);
    }
}
