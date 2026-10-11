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
        var available = Array.FindAll(Types, type => !Jongmin.RunSaveService.HasExperiencedRandomEvent(type.ToString()));
        if (available.Length == 0)
        {
            available = Types;
        }
        return available[Jongmin.RunRandom.Range(Jongmin.RunRandom.Event, 0, available.Length)];
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
