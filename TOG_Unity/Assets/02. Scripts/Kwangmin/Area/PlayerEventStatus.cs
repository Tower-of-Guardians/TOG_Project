using System;
using UnityEngine;

[Serializable]
public struct PlayerEventStatus
{
    public int ShopCountInStage;
    public int SmithyCountInStage;
    public int BlessingCooldownTurns;

    public PlayerEventStatus(int shopCountInStage, int smithyCountInStage, int blessingCooldownTurns)
    {
        ShopCountInStage = shopCountInStage;
        SmithyCountInStage = smithyCountInStage;
        BlessingCooldownTurns = blessingCooldownTurns;
    }

    public void ResetStageCounts()
    {
        ShopCountInStage = 0;
        SmithyCountInStage = 0;
    }

    public void DecreaseBlessingCooldown()
    {
        if (BlessingCooldownTurns > 0)
        {
            BlessingCooldownTurns--;
        }
    }
}
