namespace Jongmin
{
    public interface IEventProgress
    {
        bool HasSeen(string eventID);
    }

    public interface IDialogueProgress
    {
        int GetStep(string npcID);
    }
    
    public interface IShopProgress
    {
        bool HasPurchasedAllItems();
    }

    public interface IRunProgress
    {
        bool HasFirstReachedStage(int stage);
        bool HasReachedStage(int stage);
    }

    public interface IBattleRecord
    {
        int MaxSingleAttackDamage { get; }
    }

    public interface IRelicInventory
    {
        int RelicCount { get; }
        bool HasRelic(string relicID);
    }

    public interface ICardInventory
    {
        int CardCount { get; }
        bool HasCard(string cardID);
    }

    public interface IRunCardRecord
    {
        int GetGainedCountByGrade(int grade);
    }

    public interface ISynergyRecord
    {
        bool HasActivatedAtLeast(string synergyID, int count);
        bool HasFirstActivatedAtLeast(string synergyID, int count);
    }

    public interface IFlagStore
    {
        bool Has(string flagID);
    }
}