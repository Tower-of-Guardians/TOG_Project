using UnityEngine;

/// <summary>
/// 클로니어. 전용 패턴 미정 — MonsterData(41002004) 기본 액션만 사용합니다.
/// </summary>
public class Monster_Clonier : Monster
{
    [Header("클로니어 데이터 ID")]
    [SerializeField] private string monsterId = "41002004";

    protected override void Awake()
    {
        SetMonsterDataId(monsterId);
        base.Awake();
    }
}
