public interface IDEFCardDropTarget : ICardDropTarget<IFieldCardUI>
{
    bool CanInteraction { get; }
    bool IsExist(IFieldCardUI cardUI);
}