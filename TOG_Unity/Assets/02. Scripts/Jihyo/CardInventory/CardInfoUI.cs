using UnityEngine;
using UnityEngine.UI;

public class CardInfoUI : MonoBehaviour
{
    [Header("UI 관련 컴포넌트")]
    [Header("캔버스 그룹")]
    [SerializeField] private CanvasGroup m_canvas_group;

    [Header("레이어 토글")]
    [SerializeField] private Toggle m_layer_toggle;

    [Header("나가기 버튼")]
    [SerializeField] private Button m_back_button;

    private AttributePresenter m_attribute_presenter;
    private SeriesPresenter m_series_presenter;

    private CardData m_current_card_data;

    private void OnDestroy()
    {
        if (m_layer_toggle != null)
            m_layer_toggle.onValueChanged.RemoveListener(OnEnhancementPreviewToggleChanged);
    }

    public void Inject(AttributePresenter attribute_presenter,
                       SeriesPresenter series_presenter)
    {
        m_attribute_presenter = attribute_presenter;
        m_series_presenter = series_presenter;

        m_layer_toggle.onValueChanged.AddListener(OnEnhancementPreviewToggleChanged);
        m_back_button.onClick.AddListener(HidePanel);
    }

    public void ShowCardInfo(CardData card_data)
    {
        if (card_data == null)
        {
            Debug.LogWarning("CardInfoUI: cardData가 null입니다.");
            return;
        }

        m_current_card_data = card_data;
        ShowPanel();
    }

    public void ShowPanel()
    {
        ToggleUI(true);
        OnEnhancementPreviewToggleChanged(false);
    }

    public void HidePanel()
    {
        m_layer_toggle.isOn = false;
        ToggleUI(false);

        m_attribute_presenter.CloseUI();
        m_series_presenter.CloseUI();
    }

    private void OnEnhancementPreviewToggleChanged(bool isOn)
    {
        if(isOn)
        {
            m_attribute_presenter.CloseUI();
            m_series_presenter.OpenUI(m_current_card_data);
        }
        else
        {
            m_attribute_presenter.OpenUI(m_current_card_data);
            m_series_presenter.CloseUI();
        }
    }

    private void ToggleUI(bool active)
    {
        // 코루틴으로 변경할 가능성 있음
        m_canvas_group.alpha = active ? 1f : 0f;
        m_canvas_group.interactable = active;
        m_canvas_group.blocksRaycasts = active;
    }
}

