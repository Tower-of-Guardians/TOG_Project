using UnityEngine;

namespace Jongmin
{
    [CreateAssetMenu(fileName = "Discard UI Designer", menuName = "SO/Design/Discard UI Designer")]
    public class DiscardUIDesigner : ScriptableObject
    {
        [Header("기획 옵션")]
        [Header("카드 간 간격")]
        [SerializeField] private float space;
        public float Space => space;

        [Header("애니메이션 시간")]
        [SerializeField] private float animationDuration;
        public float AnimeDuration => animationDuration;
    }
}
