using UnityEngine;

public class FieldContext : MonoBehaviour
{
    [SerializeField] private FieldType _fieldType;
    [SerializeField] private FieldUI _fieldUI;
    [SerializeField] private FieldCardLayoutController _fieldCardLayout;
    [SerializeField] private FieldCardEventController _fieldCardEvent;
    [SerializeField] private FieldCardFactory _fieldCardFactory;
    private CardContainer<IFieldCardUI, FieldCardPresenter> _fieldCardContainer;

    public FieldType FieldType => _fieldType;
    public FieldUI FieldUI => _fieldUI;
    public FieldCardLayoutController FieldCardLayout => _fieldCardLayout;
    public FieldCardEventController FieldCardEvent => _fieldCardEvent;
    public FieldCardFactory FieldCardFactory => _fieldCardFactory;
    public CardContainer<IFieldCardUI, FieldCardPresenter> FieldCardContainer => _fieldCardContainer;

    private void Awake()
    {
        _fieldCardContainer = new CardContainer<IFieldCardUI, FieldCardPresenter>();
    }
}