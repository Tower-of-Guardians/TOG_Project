using UnityEngine;
using UnityEngine.InputSystem;

namespace JxModule
{
    public class JxCameraParallax : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform parallaxTransform;
        
        [Header("Settings")]
        [SerializeField] private float followTime = 0.15f;
        [SerializeField] private float maxOffsetX = 1f;
        [SerializeField] private float maxOffsetY = 0.75f;

        [Header("Options")]
        [SerializeField] private bool useUnscaledTime = false;
        
        private Vector3 _originPosition;

        private void Awake()
        {
            parallaxTransform ??= transform;
            _originPosition = parallaxTransform.position;
        }
        
        private void LateUpdate()
        {
            var mousePosition = Mouse.current.position.ReadValue();

            var normalizedX = Mathf.Clamp((mousePosition.x / Screen.width) * 2f - 1f, -1f, 1f);
            var normalizedY = Mathf.Clamp((mousePosition.y / Screen.height) * 2f - 1f, -1f, 1f);

            var targetPosition = _originPosition + new Vector3(normalizedX * maxOffsetX, 
                                                                       normalizedY * maxOffsetY, 
                                                                       0f);
            
            var deltaTime = useUnscaledTime ? Time.unscaledDeltaTime 
                                                  : Time.deltaTime;
            var time = 1f - Mathf.Exp(-followTime * deltaTime);
            
            transform.position = Vector3.Lerp(transform.position, targetPosition, time);
        }
    }
}