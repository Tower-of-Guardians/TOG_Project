using System;

[Serializable]
public class ResultData
{
    public int Gold { get; }
    public int Exp { get; }
    public bool IsLevelUp { get; }

    public ResultData(int gold, int exp, bool isLevelUp = false)
    {
        Gold = gold;
        Exp = exp;
        IsLevelUp = isLevelUp;
    }

    public ResultData(ResultData resultData)
    {
        Gold = resultData.Gold;
        Exp = resultData.Exp;
        IsLevelUp = resultData.IsLevelUp;
    }
}