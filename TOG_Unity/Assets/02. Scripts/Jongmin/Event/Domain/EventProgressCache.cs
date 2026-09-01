namespace Jongmin
{
    public sealed class EventProgressCache
    {
        public RunEventProgress Run { get; } = new();
        public GlobalEventProgress Global { get; } = new();
        public PlayerCardInventoryProgress CardInventory { get; } = new();

        public void ResetRun()
        {
            Run.Reset();
        }
    }
}
