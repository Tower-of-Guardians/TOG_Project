public readonly struct RandomEventChoiceResult
{
    public readonly int HpLoss;
    public readonly int Heal;
    public readonly int AttackGain;
    public readonly int MaxHpGain;
    public readonly string MagicID;
    public readonly bool CardReward;
    public readonly bool Battle;

    public RandomEventChoiceResult(int hpLoss = 0, int heal = 0, int attackGain = 0, int maxHpGain = 0,
        string magicID = null, bool cardReward = false, bool battle = false)
    {
        HpLoss = hpLoss;
        Heal = heal;
        AttackGain = attackGain;
        MaxHpGain = maxHpGain;
        MagicID = magicID;
        CardReward = cardReward;
        Battle = battle;
    }
}

public static class RandomEventChoiceTable
{
    private const string VampireMagicID = "1201014";

    public static bool TryGet(string nodeID, int optionIndex, out RandomEventChoiceResult result)
    {
        switch (nodeID, optionIndex)
        {
            case ("Lucien_Regular_1", 0):
                result = new RandomEventChoiceResult(hpLoss: 12, attackGain: 2);
                return true;
            case ("Lucien_Regular_1", 1):
                result = new RandomEventChoiceResult(hpLoss: 12, maxHpGain: 8);
                return true;
            case ("Lucien_Regular_1", 2):
                result = new RandomEventChoiceResult(hpLoss: 24, magicID: VampireMagicID);
                return true;
            case ("Lucien_Regular_1", 3):
                result = new RandomEventChoiceResult(hpLoss: 4);
                return true;
            case ("EventPriest_First_14", 0):
            case ("EventPriest_Regular_1", 0):
                result = new RandomEventChoiceResult(heal: 30);
                return true;
            case ("EventPriest_First_14", 1):
            case ("EventPriest_Regular_1", 1):
                result = new RandomEventChoiceResult(cardReward: true);
                return true;
            case ("TrainingDoll_Regular_1", 0):
                result = new RandomEventChoiceResult(battle: true);
                return true;
            case ("TrainingDoll_Regular_1", 1):
                result = new RandomEventChoiceResult();
                return true;
            default:
                result = default;
                return false;
        }
    }
}
