using UnityEngine;
using DG.Tweening;

namespace JxModule
{
    [RequireComponent(typeof(Camera))]
    public class JxCameraShaker : MonoBehaviour
    {
        private Transform _cameraTransform;
        private Vector3 _originLocalPosition;
        private Vector3 _originLocalRotation;
        
        private Tween _positionTween;
        private Tween _rotationTween;
        private Tween _bothTween;

        private void Awake()
        {
            Camera targetCamera = GetComponent<Camera>();
            
            _cameraTransform = targetCamera?.transform;

            if (_cameraTransform == null)
            {
                Debug.LogWarning("CameraShaker: Target camera is null. so, set main camera will be target.");
                _cameraTransform = Camera.main.transform;

                if (_cameraTransform == null)
                {
                    Debug.LogWarning("CameraShaker: Main camera is also null.");
                    enabled = false;
                    return;
                }
            }
            
            _originLocalPosition = _cameraTransform.localPosition;
            _originLocalRotation = _cameraTransform.localRotation.eulerAngles;
        }

        public Tween ShakePosition(float duration, float strength, int vibrato)
        {
            ResetState();
            _positionTween = CreateShakePosition(duration, strength, vibrato);
            
            return _positionTween;
        }

        public Tween ShakeRotation(float duration, Vector3 strength, int vibrato)
        {
            ResetState();
            _rotationTween = CreateShakeRotation(duration, strength, vibrato);
            
            return _rotationTween;
        }

        public Sequence Shake(float duration,
                              float positionStrength, 
                              int positionVibrato, 
                              Vector3 rotationStrength,
                              int rotationVibrato)
        {
            ResetState();
            
            Sequence shakeSequence = DOTween.Sequence();
            shakeSequence.Join(CreateShakePosition(duration, positionStrength, positionVibrato));
            shakeSequence.Join(CreateShakeRotation(duration, rotationStrength, rotationVibrato));
            shakeSequence.OnComplete(() =>
            {
                _cameraTransform.localPosition = _originLocalPosition;
                _cameraTransform.localEulerAngles = _originLocalRotation;
            });
            
            _bothTween = shakeSequence;
            
            return shakeSequence;
        }
        
        private Tween CreateShakePosition(float duration, float strength, int vibrato)
            => _cameraTransform.DOShakePosition(duration, strength, vibrato);
        
        private Tween CreateShakeRotation(float duration, Vector3 strength, int vibrato)
            => _cameraTransform.DOShakeRotation(duration, strength, vibrato);

        private void ResetState()
        {
            _positionTween?.Kill();
            _rotationTween?.Kill();
            _bothTween?.Kill();

            ResetTransform();
        }
        
        private void ResetTransform()
        {
            if (_cameraTransform == null)
            {
                return;
            }
            
            _cameraTransform.localPosition = _originLocalPosition;
            _cameraTransform.localEulerAngles = _originLocalRotation;
        }
    }
}