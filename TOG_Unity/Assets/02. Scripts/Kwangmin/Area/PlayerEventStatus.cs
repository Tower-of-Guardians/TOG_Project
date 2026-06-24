using UnityEngine;

public struct PlayerEventStatus
{
    public int ShopCountInStage;
    public int SmithyCountInStage;
    public int BlessingCooldownTurns;

    public PlayerEventStatus(int shop, int smith, int blessing)
    {
        this.ShopCountInStage = shop;
        this.SmithyCountInStage = smith;
        this.BlessingCooldownTurns = blessing;
    }
}
