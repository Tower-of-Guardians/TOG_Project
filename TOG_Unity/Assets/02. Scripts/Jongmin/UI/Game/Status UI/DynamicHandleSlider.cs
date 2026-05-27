using UnityEngine;
using UnityEngine.UI;

public class DynamicHandleSlider : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider slider;
    [SerializeField] private Image handleImage;
    
    [Header("Sprite References")]
    [SerializeField] private Sprite leftSprite;
    [SerializeField] private Sprite middleSprite;
    [SerializeField] private Sprite rightSprite;

    [Header("Offset References")]
    [SerializeField] private float leftLimitOffset = 0.05f;
    [SerializeField] private float rightLimitOffset = 0.95f;
    
    private void Awake()
    {
        slider ??= GetComponent<Slider>();
    }

    public void SetValue(float amount)
    {
        if (amount <= leftLimitOffset)
        {
            handleImage.sprite = leftSprite;
        }
        else if (amount >= rightLimitOffset)
        {
            handleImage.sprite = rightSprite;
        }
        else
        {
            handleImage.sprite = middleSprite;
        }
        
        slider.value = amount;
    }
}