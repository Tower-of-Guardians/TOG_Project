using JxModule;
using UnityEngine;

namespace Developers.KJM
{
    public class ShadowableObject : MonoBehaviour
    {
        [SerializeField] private bool hoverEnable;
        
        [BigHeader("Object References")]
        [SerializeField] private Transform rootTransform;
        [SerializeField] private Transform shadowTransform;
        [SerializeField] private Transform rotationAxisTransform;
        [SerializeField] private SpriteRenderer shadowRenderer;
        
        [Space(20f), BigHeader("Offset Settings")]
        [SerializeField] private float baseShadowPositionOffset = 0.02f;
        [SerializeField, ShowIf("hoverEnable")] private float hoverShadowPositionOffset = 0.14f;
        [SerializeField, ShowIf("hoverEnable")] private float positionInterpolationSpeed = 24f;
        
        [Space(20f), BigHeader("Alpha Settings")]
        [SerializeField] private float nearShadowAlpha = 0.75f;
        [SerializeField, ShowIf("hoverEnable")] private float farShadowAlpha = 0.5f;
        [SerializeField, ShowIf("hoverEnable")] private float alphaInterpolationSpeed = 4f;

        private Vector2 _lightPoint;
        private int _baseSortingOrder;
        private int _sortingOrderOffset;
        private bool _isHovering;
        
        private Vector3 _currentLocalPosition;
        private float _currentAlpha;
        
        private Vector3 _targetLocalPosition;
        private float _targetAlpha;

        private bool _isInitialized;
        
        private void Awake()
        {
            rootTransform ??= transform;

            if (shadowTransform == null)
            {
                Debug.LogError("Shadowable Object: shadow transform can not be null.");
                enabled = false;
                return;
            }

            if (shadowRenderer == null)
            {
                Debug.LogError("Shadowable Object: shadow renderer can not be null.");
                enabled = false;
                return;
            }

            _baseSortingOrder = shadowRenderer.sortingOrder;
            _sortingOrderOffset = 5;
            
            _currentAlpha = nearShadowAlpha;
            _targetAlpha = nearShadowAlpha;
            
            _isInitialized = true;
        }

        private void OnEnable()
        {
            RefreshImmediately();
        }

        private void Update()
        {
            UpdateShadow(false);
        }

        public void SetHoverState(bool isHovering)
        {
            if (!hoverEnable)
            {
                return;
            }
            
            _isHovering = isHovering;
            _targetAlpha = _isHovering ? farShadowAlpha 
                                       : nearShadowAlpha;
        }

        public void SetBaseSortingOrder(int order)
        {
            _baseSortingOrder = order;
            ApplySortingOrder();
        }

        public void SetSortingOrderOffset(int order)
        {
            _sortingOrderOffset = order;
            ApplySortingOrder();
        }

        public void ToggleRenderer(bool isActive)
        {
            if (isActive)
            {
                RefreshImmediately();
            }
            
            shadowRenderer.enabled = isActive;
        }

        public void RefreshImmediately()
        {
            if (!_isInitialized)
            {
                return;
            }

            UpdateShadow(true);
        }

        private void UpdateShadow(bool immediate)
        {
            if (!TryRefreshLightPoint())
            {
                return;
            }

            if (TryGetLightVector(rootTransform, out var lightVector))
            {
                UpdateShadowTarget(lightVector);
            }

            UpdateShadowRotation();
            UpdateShadowTransform(immediate);
            UpdateShadowAlpha(immediate);
            ApplySortingOrder();
        }

        private bool TryRefreshLightPoint()
        {
            if (JxVirtualLightPoint.Instance == null)
            {
                return false;
            }
            
            _lightPoint = JxVirtualLightPoint.Instance.Position;
            return true;
        }

        private void UpdateShadowRotation()
        {
            if (rotationAxisTransform == null)
            {
                return;
            }
            
            shadowTransform.localRotation = rotationAxisTransform.localRotation;
        }

        private void UpdateShadowTarget(Vector2 lightVector)
        {
            var normalizedLightVector = lightVector.normalized;
            var baseOffset = _isHovering ? hoverShadowPositionOffset
                                                : baseShadowPositionOffset;
            
            _targetLocalPosition = normalizedLightVector * baseOffset;
        }

        private void UpdateShadowTransform(bool immediate)
        {
            if (immediate || !hoverEnable)
            {
                _currentLocalPosition = _targetLocalPosition;
            }
            else
            {
                _currentLocalPosition = Vector3.Lerp(_currentLocalPosition, 
                                                     _targetLocalPosition, 
                                                     Time.deltaTime * positionInterpolationSpeed);   
            }
            
            shadowTransform.localPosition = _currentLocalPosition;
        }

        private void UpdateShadowAlpha(bool immediate)
        {
            if (immediate && !hoverEnable)
            {
                _currentAlpha = _targetAlpha;
            }
            else
            {
                _currentAlpha = Mathf.Lerp(_currentAlpha,
                                           _targetAlpha,
                                           Time.deltaTime * alphaInterpolationSpeed);     
            }
            
            SetShadowAlpha(_currentAlpha);
        }
        
        private void ApplySortingOrder()
        {
            shadowRenderer.sortingOrder = _isHovering ? _baseSortingOrder + _sortingOrderOffset
                                                      : _baseSortingOrder;
        }

        private void SetShadowAlpha(float alpha)
        {
            var color = shadowRenderer.color;
            color.a = alpha;
            shadowRenderer.color = color;
        }

        private bool TryGetLightVector(Transform targetTransform, out Vector2 lightVector)
        {
            lightVector = Vector2.zero;

            if (targetTransform == null)
            {
                return false;
            }
            
            var worldOffset = targetTransform.position - (Vector3)_lightPoint;
            var localOffset = rootTransform.InverseTransformDirection(worldOffset);
            lightVector = new Vector2(localOffset.x, localOffset.y);
            
            return true;
        }
    }
}