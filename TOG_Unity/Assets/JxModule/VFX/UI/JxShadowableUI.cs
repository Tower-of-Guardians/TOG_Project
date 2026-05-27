using UnityEngine;
using UnityEngine.UI;

namespace JxModule
{
    public class JxShadowableUI : MonoBehaviour
    {
        [SerializeField] private bool hoverEnable;

        [BigHeader("UI References")]
        [SerializeField] private RectTransform rootRectTransform;
        [SerializeField] private RectTransform shadowRectTransform;
        [SerializeField] private RectTransform rotationAxisRectTransform;
        [SerializeField] private Graphic shadowGraphic;

        [Space(20f), BigHeader("Offset Settings")]
        [SerializeField] private float baseShadowPositionOffset = 2f;
        [SerializeField, ShowIf("hoverEnable")] private float hoverShadowPositionOffset = 14f;
        [SerializeField, ShowIf("hoverEnable")] private float positionInterpolationSpeed = 24f;

        [Space(20f), BigHeader("Alpha Settings")]
        [SerializeField] private float nearShadowAlpha = 0.75f;
        [SerializeField, ShowIf("hoverEnable")] private float farShadowAlpha = 0.5f;
        [SerializeField, ShowIf("hoverEnable")] private float alphaInterpolationSpeed = 4f;

        private Vector2 _lightPoint;

        private int _baseSiblingIndex;
        private int _siblingIndexOffset;

        private bool _isHovering;
        private bool _isInitialized;

        private Vector2 _currentAnchoredPosition;
        private Vector2 _targetAnchoredPosition;

        private float _currentAlpha;
        private float _targetAlpha;

        private void Awake()
        {
            rootRectTransform ??= transform as RectTransform;

            if (rootRectTransform == null)
            {
                Debug.LogError("JxShadowableUI: root rect transform can not be null.");
                enabled = false;
                return;
            }

            if (shadowRectTransform == null)
            {
                Debug.LogError("JxShadowableUI: shadow rect transform can not be null.");
                enabled = false;
                return;
            }

            if (shadowGraphic == null)
            {
                Debug.LogError("JxShadowableUI: shadow graphic can not be null.");
                enabled = false;
                return;
            }

            _baseSiblingIndex = shadowRectTransform.GetSiblingIndex();
            _siblingIndexOffset = 1;

            _currentAlpha = nearShadowAlpha;
            _targetAlpha = nearShadowAlpha;

            _currentAnchoredPosition = shadowRectTransform.anchoredPosition;
            _targetAnchoredPosition = _currentAnchoredPosition;

            SetShadowAlpha(_currentAlpha);

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
            _targetAlpha = _isHovering ? farShadowAlpha : nearShadowAlpha;
        }

        public void SetBaseSiblingIndex(int index)
        {
            _baseSiblingIndex = index;
            ApplySiblingIndex();
        }

        public void SetSiblingIndexOffset(int offset)
        {
            _siblingIndexOffset = offset;
            ApplySiblingIndex();
        }

        public void ToggleRenderer(bool isActive)
        {
            if (isActive)
            {
                RefreshImmediately();
            }

            shadowGraphic.enabled = isActive;
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

            if (TryGetLightVector(rootRectTransform, out var lightVector))
            {
                UpdateShadowTarget(lightVector);
            }

            UpdateShadowRotation();
            UpdateShadowTransform(immediate);
            UpdateShadowAlpha(immediate);
            ApplySiblingIndex();
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
            if (rotationAxisRectTransform == null)
            {
                return;
            }

            shadowRectTransform.localRotation = rotationAxisRectTransform.localRotation;
        }

        private void UpdateShadowTarget(Vector2 lightVector)
        {
            var normalizedLightVector = lightVector.normalized;
            var offset = _isHovering ? hoverShadowPositionOffset
                                     : baseShadowPositionOffset;

            _targetAnchoredPosition = normalizedLightVector * offset;
        }

        private void UpdateShadowTransform(bool immediate)
        {
            if (immediate || !hoverEnable)
            {
                _currentAnchoredPosition = _targetAnchoredPosition;
            }
            else
            {
                _currentAnchoredPosition = Vector2.Lerp(
                    _currentAnchoredPosition,
                    _targetAnchoredPosition,
                    Time.deltaTime * positionInterpolationSpeed
                );
            }

            shadowRectTransform.anchoredPosition = _currentAnchoredPosition;
        }

        private void UpdateShadowAlpha(bool immediate)
        {
            if (immediate || !hoverEnable)
            {
                _currentAlpha = _targetAlpha;
            }
            else
            {
                _currentAlpha = Mathf.Lerp(
                    _currentAlpha,
                    _targetAlpha,
                    Time.deltaTime * alphaInterpolationSpeed
                );
            }

            SetShadowAlpha(_currentAlpha);
        }

        private void ApplySiblingIndex()
        {
            if (shadowRectTransform.parent == null)
            {
                return;
            }

            var targetIndex = _isHovering
                ? _baseSiblingIndex + _siblingIndexOffset
                : _baseSiblingIndex;

            targetIndex = Mathf.Clamp(
                targetIndex,
                0,
                shadowRectTransform.parent.childCount - 1
            );

            shadowRectTransform.SetSiblingIndex(targetIndex);
        }

        private void SetShadowAlpha(float alpha)
        {
            var color = shadowGraphic.color;
            color.a = alpha;
            shadowGraphic.color = color;
        }

        private bool TryGetLightVector(RectTransform targetRectTransform, out Vector2 lightVector)
        {
            lightVector = Vector2.zero;

            if (targetRectTransform == null)
            {
                return false;
            }

            var worldOffset = targetRectTransform.position - (Vector3)_lightPoint;
            var localOffset = rootRectTransform.InverseTransformDirection(worldOffset);

            lightVector = new Vector2(localOffset.x, localOffset.y);
            return lightVector.sqrMagnitude > 0.0001f;
        }
    }
}