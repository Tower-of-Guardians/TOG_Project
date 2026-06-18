using UnityEngine;

/// <summary>
/// 클론. MonsterData(41001005) 공격 패턴만 사용합니다.
/// </summary>
public class Monster_ClonierClone : Monster
{
    [Header("클론 데이터 ID")]
    [SerializeField] private string monsterId = "41001005";

    private float summonSlotX = float.NaN;

    public float SummonSlotX => summonSlotX;

    public bool HasSummonSlot => !float.IsNaN(summonSlotX);

    protected override void Awake()
    {
        SetMonsterDataId(monsterId);
        base.Awake();
    }

    public void BindSummonSlot(float slotX)
    {
        summonSlotX = slotX;
    }
}
