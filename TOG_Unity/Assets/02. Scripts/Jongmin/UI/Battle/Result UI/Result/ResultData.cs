using System;

[Serializable]
public class ResultData
{
    private readonly int _gold;
    public int Gold => _gold;

    private readonly int _exp;
    public int EXP => _exp;

    public ResultData(int gold,
                      int exp)
    {
        _gold = gold;
        _exp = exp;
    }

    public ResultData(ResultData resultData)
    {
        _gold = resultData.Gold;
        _exp = resultData.EXP;
    }
}