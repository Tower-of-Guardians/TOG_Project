using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace JxDialogueBox
{
    public sealed class TypeWriter : MonoBehaviour
    {
        [Header("Configure")]
        [SerializeField]
        private float charInterval = 0.03f;
 
        [Header("Use unscaled time")]
        [SerializeField]
        private bool useUnscaledTime = false;

        [Header("Target Text")]
        [SerializeField]
        private TMP_Text targetText;

        private Coroutine _writeCoroutine;
        private bool _isTyping;
        private string _fullText = string.Empty;

        public event Action OnCompleted;

        public bool IsTyping => _isTyping;
        public string FullText => _fullText;

        public void Play(string text)
        {
            _fullText = text ?? string.Empty;

            if (_writeCoroutine != null)
            {
                StopCoroutine(_writeCoroutine);
                _writeCoroutine = null;
            }

            _writeCoroutine = StartCoroutine(TypeRoutine(_fullText));
        }

        public void Skip()
        {
            if (!_isTyping)
            {
                return;
            }

            if (_writeCoroutine != null)
            {
                StopCoroutine(_writeCoroutine);
                _writeCoroutine = null;
            }

            _isTyping = false;

            if (targetText)
            {
                targetText.text = _fullText;
            }
        }

        public void Stop(bool clear = false)
        {
            if (_writeCoroutine != null)
            {
                StopCoroutine(_writeCoroutine);
                _writeCoroutine = null;
            }

            _isTyping = false;

            if (clear && targetText)
            {
                targetText.text = string.Empty;
            }
        }

        public void SetInterval(float spc)
        {
            charInterval = Mathf.Max(0f, spc);
        }

        private IEnumerator TypeRoutine(string text)
        {
            _isTyping = true;

            if (targetText)
            {
                targetText.text = string.Empty;
            }

            foreach (var ch in text)
            {
                if (targetText)
                {
                    targetText.text += ch;
                }

                if (charInterval > 0f)
                {
                    if (useUnscaledTime)
                    {
                        yield return new WaitForSecondsRealtime(charInterval);
                    }
                    else
                    {
                        yield return new WaitForSeconds(charInterval);
                    }
                }
                else
                {
                    if (targetText)
                    {
                        targetText.text = text;
                    }

                    break;
                }
            }

            _isTyping = false;
            _writeCoroutine = null;

            OnCompleted?.Invoke();
        }
    }
}
