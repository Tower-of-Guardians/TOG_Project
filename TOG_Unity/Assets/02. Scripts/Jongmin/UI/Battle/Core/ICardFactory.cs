public interface ICardFactory<T> where T : ICardUI
{
    T Create();
    void Release(T cardUI);
}