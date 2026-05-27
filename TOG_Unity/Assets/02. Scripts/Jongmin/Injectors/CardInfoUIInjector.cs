using UnityEngine;

public class CardInfoUIInjector : MonoBehaviour, IInjector
{
    [Header("의존성 목록")]
    [Header("카드 정보 뷰")]
    [SerializeField] private CardInfoUI m_card_info_ui;

    [Header("카드 어트리뷰트 뷰")]
    [SerializeField] private AttributeView m_attribute_view;

    [Header("카드 시리즈 뷰")]
    [SerializeField] private SeriesView m_series_view; 

    public void Inject()
    {
        InjectAttribute();
        InjectSeries();
        InjectInfo();
    }

    private void InjectAttribute()
    {
        DIContainer.Register<IAttributeView>(m_attribute_view);

        var attribute_presenter = new AttributePresenter(m_attribute_view);
        DIContainer.Register<AttributePresenter>(attribute_presenter);
    }

    private void InjectSeries()
    {
        DIContainer.Register<ISeriesView>(m_series_view);

        var m_series_presenter = new SeriesPresenter(m_series_view);
        DIContainer.Register<SeriesPresenter>(m_series_presenter);
    }

    private void InjectInfo()
    {
        m_card_info_ui.Inject(DIContainer.Resolve<AttributePresenter>(),
                              DIContainer.Resolve<SeriesPresenter>());
    }
}
