using UnityEngine;

namespace Jongmin
{
    [RequireComponent(typeof(Animator))]
    public class PreviewCard : MonoBehaviour
    {
        private Animator _animator;
        private RectTransform _rectTransform;

        public RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                {
                    _rectTransform = GetComponent<RectTransform>();
                }
                
                return _rectTransform;
            }
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _rectTransform = transform as RectTransform;
        }

        private void OnEnable()
        {
            _animator.SetTrigger("Enable");
        }
    }
}
