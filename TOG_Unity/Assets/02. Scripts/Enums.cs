public enum MonsterType
{
    Normal = 1,
    Boss = 2
}

public enum MonsterBehaviorPattern
{
    Random = 1,
    Loop = 2
}

public enum DeckType
{
    Draw = 1,
    Throw = 2,
}

public enum FieldType
{
    Attack = 1,
    Defense = 2,
}

public enum SortType
{
    Time, // ���� ȹ��� id������
    Grade,
    Attack,
    Defense
}

public enum TabType
{
    Card,
    Magic
}

public enum CardWhen
{
    None = 0,           // 효과 없음
    BeforeAttack = 1,   // 공격 전
    AfterAttack = 2,    // 공격 후
    BeforeThenAfter = 3 // 공격 전 Effect1, 공격 후 Effect2
}

public enum CardTrigger
{
    None = 0,
    FirstTurn = 1,
    AttackPlace = 2,
    DefensePlace = 3,
    OnKill = 4,
    TargetCursed = 5,
    Frenzy = 6,
    UsedMagicThisTurn = 7,
    TargetNotCursed = 8
}

public enum CardEffectTarget
{
    None = 0,
    Self = 1,
    ChosenTarget = 2,
    RandomEnemy = 3,
    AllEnemies = 4,
    AttackHitTarget = 5,
    ThisCard = 6,
    CardsToTheRight = 7,
    ChosenCard = 8,
    DiscardPileCard = 9,
    CreateMagic = 11,
    RandomMagicByGrade = 12,
    NamedCards = 13,
    ThisCardCastCount = 14
}