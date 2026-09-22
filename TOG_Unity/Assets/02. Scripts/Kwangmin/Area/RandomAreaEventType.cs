using System;

public enum RandomAreaEventType
{
    Vampire,
    Spirit,
    Cleric,
    Jester,
    RestRoom,
    Ambush,
    TrainingDummy,
    Archive,
    Sanctuary
}

public static class RandomAreaEventSelector
{
    private static readonly RandomAreaEventType[] Types =
        (RandomAreaEventType[])Enum.GetValues(typeof(RandomAreaEventType));

    public static RandomAreaEventType Select()
    {
        return Types[UnityEngine.Random.Range(0, Types.Length)];
    }

    public static string GetName(RandomAreaEventType type)
    {
        return type switch
        {
            RandomAreaEventType.Vampire => "흡혈귀",
            RandomAreaEventType.Spirit => "정령",
            RandomAreaEventType.Cleric => "성직자",
            RandomAreaEventType.Jester => "어릿광대",
            RandomAreaEventType.RestRoom => "휴게실",
            RandomAreaEventType.Ambush => "습격",
            RandomAreaEventType.TrainingDummy => "훈련용 인형",
            RandomAreaEventType.Archive => "기록실",
            RandomAreaEventType.Sanctuary => "성소",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
