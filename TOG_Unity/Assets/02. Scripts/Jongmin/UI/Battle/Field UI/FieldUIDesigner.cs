using UnityEngine;

[CreateAssetMenu(fileName = "Field UI Designer", menuName = "SO/Design/Field UI Designer")]
public class FieldUIDesigner : ScriptableObject
{
    [Header("기획 옵션")]
    [Header("카드 개수")]
    [Header("공격 필드에 배치할 수 있는 최대 카드의 수")]
    [SerializeField] private int m_attack_field_limit;
    public int ATKLimit => m_attack_field_limit;

    [Header("방어 필드에 배치할 수 있는 최대 카드의 수")]
    [SerializeField] private int m_defend_field_limit;
    public int DEFLimit => m_defend_field_limit;

    [Header("카드 간 간격")]
    [SerializeField] private float m_space;
    public float Space => m_space;

    [Header("애니메이션 시간")]
    [SerializeField] private float m_anime_duration;
    public float AnimeDuration => m_anime_duration;
}