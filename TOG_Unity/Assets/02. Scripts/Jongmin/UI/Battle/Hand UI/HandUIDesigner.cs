using UnityEngine;

namespace Jongmin
{
    [CreateAssetMenu(fileName = "Hand UI Designer", menuName = "SO/Design/Hand UI Designer")]
    public class HandUIDesigner : ScriptableObject
    {
        [Header("기획 옵션")]
        [Header("기본 배치 관련 설정")]
        [Header("카드 배치 반지름")]
        [SerializeField] private float radius = 360f;
        public float Radius => radius;

        [Header("카드의 최대 각도")]
        [SerializeField] private float arcAngle = 15f;
        public float Angle => arcAngle;
    
        [Header("Z축 깊이")]
        [SerializeField] private float depthMultiplier = 0.5f;
        public float Depth => depthMultiplier;

        [Space(20f)]
        [Header("강조 배치 관련 설정")]
        [Header("강조 강도")]
        [SerializeField] private float hoverYPosition = 120f;
        public float HoverY => hoverYPosition;

        [Header("강조 크기")]
        [SerializeField] private float hoverScale = 1.25f;
        public float Scale => hoverScale;

        [Header("밀림 강도")]
        [SerializeField] private float pushXPosition = 80f;
        public float Strength => pushXPosition;

        [Header("강조 속도")]
        [SerializeField] private float tweenDuration = 0.15f;
        public float AnimeSPD => tweenDuration;
    }
}

