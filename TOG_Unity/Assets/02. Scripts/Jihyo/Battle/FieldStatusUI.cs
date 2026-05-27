using TMPro;
using UnityEngine;

public class FieldStatusUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TMP_Text attackPowerText;
    [SerializeField] private TMP_Text defensePowerText;
    
    [Header("Update Settings")]
    [SerializeField] private float updateInterval = 0.1f;
    
    private float currentAttackPower;
    private float currentDefensePower;
    private float lastUpdateTime;
    
    private void Start()
    {
        UpdateFieldStatus();
    }
    
    private void Update()
    {
        // 일정 간격으로 필드 상태 체크 및 업데이트
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            lastUpdateTime = Time.time;
            CheckAndUpdateFieldStatus();
        }
    }
    
    private void CheckAndUpdateFieldStatus()
    {
        if (GameData.Instance != null)
        {
            float newAttackPower = GameData.Instance.AttackField();
            float newDefensePower = GameData.Instance.DefenseField();
            
            // 값이 변경되었을 때만 UI 업데이트
            if (Mathf.Abs(newAttackPower - currentAttackPower) > 0.01f ||
                Mathf.Abs(newDefensePower - currentDefensePower) > 0.01f)
            {
                currentAttackPower = newAttackPower;
                currentDefensePower = newDefensePower;
                UpdateDisplay();
            }
        }
    }
    
    public void UpdateFieldStatus()
    {
        if (GameData.Instance != null)
        {
            currentAttackPower = GameData.Instance.AttackField();
            currentDefensePower = GameData.Instance.DefenseField();
            UpdateDisplay();
        }
    }
    
    private void UpdateDisplay()
    {
        if (attackPowerText != null)
        {
            int attackValue = Mathf.RoundToInt(currentAttackPower);
            attackPowerText.text = attackValue.ToString();
        }
        
        if (defensePowerText != null)
        {
            int defenseValue = Mathf.RoundToInt(currentDefensePower);
            defensePowerText.text = defenseValue.ToString();
        }
    }
    
    public float GetCurrentAttackPower()
    {
        return currentAttackPower;
    }
    
    public float GetCurrentDefensePower()
    {
        return currentDefensePower;
    }
}

