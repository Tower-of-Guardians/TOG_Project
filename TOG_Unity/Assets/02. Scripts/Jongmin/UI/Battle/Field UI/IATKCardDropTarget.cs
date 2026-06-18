namespace Jongmin
{
    public interface IATKCardDropTarget : ICardDropTarget<FieldDomain>
    {
        bool CanInteraction { get; }
        bool IsExist(Card card);
    } 
}
