using TMPro;
using UnityEngine;

public class TurnUI : MonoBehaviour
{
    [Header("UI 관련 컴포넌트")]
    [Header("턴 번호 텍스트")]
    [SerializeField] private TMP_Text m_turn_number_text;

    private TurnManager m_turn_manager;

    private void Start()
    {
        StartCoroutine(InitializeDelayed());
    }

    private System.Collections.IEnumerator InitializeDelayed()
    {
        yield return new WaitUntil(() => DIContainer.IsRegistered<TurnManager>());

        m_turn_manager = DIContainer.Resolve<TurnManager>();

        if (m_turn_manager != null)
        {
            m_turn_manager.OnTurnNumberChanged += UpdateTurnNumber;

            // 초기 턴 번호 표시
            if (m_turn_manager.CurrentTurnNumber > 0)
            {
                UpdateTurnNumber(m_turn_manager.CurrentTurnNumber);
            }
        }
    }

    private void UpdateTurnNumber(int turn_number)
    {
        if (m_turn_number_text != null)
        {
            m_turn_number_text.text = $"{turn_number} 턴";
        }
    }

    private void OnDestroy()
    {
        if (m_turn_manager != null)
        {
            m_turn_manager.OnTurnNumberChanged -= UpdateTurnNumber;
        }
    }
}
