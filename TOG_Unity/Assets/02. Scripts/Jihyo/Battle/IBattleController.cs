public interface IBattleController
{
    public void Initialize(BattleManager battleManager);
    public void Cleanup();
    public bool IsInitialized { get; }
}