using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace JxDialogueBox
{
    public sealed class TypeWriter : MonoBehaviour
    {
        [Header("Configure")]
        private float m_char_interval = 0.03f;

        [Header("Use unscaled time")]
        [SerializeField] private bool m_use_unscaled_time = false;

        [Header("Target Text")]
        [SerializeField] private TMP_Text m_target_text;

        private Coroutine m_write_coroutine;
        private bool m_is_typing;
        private string m_full_text = string.Empty;

        public event Action OnCompleted;

        public bool IsTyping => m_is_typing;
        public string FullText => m_full_text = string.Empty;

        public void Play(string text)
        {
            m_full_text = text ?? string.Empty;

            if(m_write_coroutine != null)
            {
                StopCoroutine(m_write_coroutine);
                m_write_coroutine = null;
            }

            m_write_coroutine = StartCoroutine(TypeRoutine(m_full_text));
        }

        public void Skip()
        {
            if(!m_is_typing)
                return;

            if(m_write_coroutine != null)
            {
                StopCoroutine(m_write_coroutine);
                m_write_coroutine = null;
            }

            m_is_typing = false;

            if(m_target_text)
                m_target_text.text = m_full_text;
        }

        public void Stop(bool clear = false)
        {
            if(m_write_coroutine != null)
            {
                StopCoroutine(m_write_coroutine);
                m_write_coroutine = null;
            }

            m_is_typing = false;

            if(clear && m_target_text)
                m_target_text.text = string.Empty;
        }

        public void SetInterval(float spc)
            => m_char_interval = Mathf.Max(0f, spc);

        private IEnumerator TypeRoutine(string text)
        {
            m_is_typing = true;

            if(m_target_text)
                m_target_text.text = string.Empty;

            for(int i = 0; i < text.Length; i++)
            {
                if(m_target_text)
                    m_target_text.text += text[i];

                if(m_char_interval > 0f)
                {
                    if(m_use_unscaled_time)
                        yield return new WaitForSecondsRealtime(m_char_interval);
                    else
                        yield return new WaitForSeconds(m_char_interval);
                }
                else
                {
                    m_target_text.text = text;
                    break;
                }
            }

            m_is_typing = false;
            m_write_coroutine = null;

            OnCompleted?.Invoke();
        }
    }
}
