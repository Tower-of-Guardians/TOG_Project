using System.Collections;
using JxModule;
using UnityEngine;

namespace JxDialogueBox
{
    [RequireComponent(typeof(DialogueView))]
    public class DialogueAutoAdvanceDriver : MonoBehaviour
    {
        [BigHeader("References")]
        [SerializeField,Required, AssetOnly] private DialogueSettings dialogueSettings;
        [SerializeField, SceneOnly] private DialogueView dialogueView;
        [SerializeField, SceneOnly] private TypeWriter typeWriter;

        private Coroutine _autoCoroutine;

        private void OnEnable()
        {
            if (typeWriter)
            {
                typeWriter.OnCompleted += HandleTypingCompleted;
            }
        }

        private void OnDisable()
        {
            if (typeWriter)
            {
                typeWriter.OnCompleted -= HandleTypingCompleted;
            }

            StopAutoAdvance();
        }

        private void HandleTypingCompleted()
        {
            if (dialogueSettings == null || !dialogueSettings.autoAdvanceAllowed)
            {
                return;
            }
            
            StopAutoAdvance();
            _autoCoroutine = StartCoroutine(AutoRoutine());
        }

        private IEnumerator AutoRoutine()
        {
            var delay = Mathf.Max(0f, dialogueSettings.autoAdvanceDelay);
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            _autoCoroutine = null;
            dialogueView.RequestAdvance();
        }

        public void StopAutoAdvance()
        {
            if (_autoCoroutine != null)
            {
                StopCoroutine(_autoCoroutine);
                _autoCoroutine = null;
            }
        }
    }
}