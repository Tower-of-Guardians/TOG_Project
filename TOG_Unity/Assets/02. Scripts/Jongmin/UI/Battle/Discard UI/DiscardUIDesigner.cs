using UnityEngine;

[CreateAssetMenu(fileName = "Discard UI Designer", menuName = "SO/Design/Discard UI Designer")]
public class DiscardUIDesigner : ScriptableObject
{
    [Header("기획 옵션")]
    [Header("카드 간 간격")]
    [SerializeField] private float _space;
    public float Space => _space;

    [Header("애니메이션 시간")]
    [SerializeField] private float _animationDuration;
    public float AnimeDuration => _animationDuration;
}
