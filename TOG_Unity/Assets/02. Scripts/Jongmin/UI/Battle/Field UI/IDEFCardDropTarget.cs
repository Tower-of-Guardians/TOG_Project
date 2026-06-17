namespace Jongmin
{
    public interface IDEFCardDropTarget : ICardDropTarget<FieldDomain>
    {
        bool CanInteraction { get; }
        bool IsExist(Card card);
    }
}
