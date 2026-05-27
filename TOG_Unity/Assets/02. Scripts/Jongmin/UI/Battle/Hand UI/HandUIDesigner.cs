using UnityEngine;

[CreateAssetMenu(fileName = "Hand UI Designer", menuName = "SO/Design/Hand UI Designer")]
public class HandUIDesigner : ScriptableObject
{
    [Header("기획 옵션")]
    [Header("기본 배치 관련 설정")]
    [Header("카드 배치 반지름")]
    [SerializeField] private float m_radius = 130f;
    public float Radius => m_radius;

    [Header("카드의 최대 각도")]
    [SerializeField] private float m_arc_angle = 30f;
    public float Angle => m_arc_angle;
    
    [Header("Z축 깊이")]
    [SerializeField] private float m_depth_multiplier = 0.5f;
    public float Depth => m_depth_multiplier;

    [Space(20f)]
    [Header("강조 배치 관련 설정")]
    [Header("강조 강도")]
    [SerializeField] private float m_hover_y_position = 100f;
    public float HoverY => m_hover_y_position;

    [Header("강조 크기")]
    [SerializeField] private float m_hover_scale = 1.2f;
    public float Scale => m_hover_scale;

    [Header("밀림 강도")]
    [SerializeField] private float m_push_x_position = 50f;
    public float Strength => m_push_x_position;

    [Header("강조 속도")]
    [SerializeField] private float m_anime_duration = 0.5f;
    public float AnimeSPD => m_anime_duration;
}
