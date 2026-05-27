public interface IATKCardDropTarget : ICardDropTarget<IFieldCardUI>
{
    bool CanInteraction { get; }
    bool IsExist(IFieldCardUI cardUI);
}