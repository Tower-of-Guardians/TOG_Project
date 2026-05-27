using JxModule;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace JxDialogueBox
{
    [RequireComponent(typeof(DialogueView))]
    public sealed class DialogueInputDriver : MonoBehaviour
    {
        [BigHeader("References")]
        [SerializeField, Required, AssetOnly] private DialogueSettings dialogueSettings;
        [SerializeField, SceneOnly] private DialogueView dialogueView;

#if ENABLE_INPUT_SYSTEM
        private InputAction _advanceAction;
        private InputAction _selectAction;
#endif

        private void OnEnable()
        {
            if (!dialogueView)
            {
                return;
            }

#if ENABLE_INPUT_SYSTEM
            if (dialogueSettings != null && dialogueSettings.keyboardInputAllowed)
            {
                if (dialogueSettings.advanceAction)
                {
                    _advanceAction = dialogueSettings.advanceAction.action;
                    _advanceAction.performed += OnAdvanced;
                    _advanceAction.Enable();
                }

                if (dialogueSettings.selectionAction)
                {
                    _selectAction = dialogueSettings.selectionAction.action;
                    _selectAction.performed += OnSelected;
                    _selectAction.Enable();
                }
            }
#endif
        }

        private void OnDisable()
        {
#if ENABLE_INPUT_SYSTEM
            if (_advanceAction != null)
            {
                _advanceAction.performed -= OnAdvanced;
                _advanceAction.Disable();
                _advanceAction = null;
            }

            if (_selectAction != null)
            {
                _selectAction.performed -= OnSelected;
                _selectAction.Disable();
                _selectAction = null;
            }
#endif            
        }

#if ENABLE_INPUT_SYSTEM
        private void OnAdvanced(InputAction.CallbackContext ctx)
        {
            if (dialogueSettings == null || !dialogueSettings.keyboardInputAllowed)
            {
                return;
            }

            var device = ctx.control?.device;
            if (device is Pointer)
            {
                return;
            }

            dialogueView.RequestAdvance();
        }

        private void OnSelected(InputAction.CallbackContext ctx)
        {
            if (dialogueSettings == null || !dialogueSettings.keyboardInputAllowed)
            {
                return;
            }

            if (!dialogueView.ChoiceMode)
            {
                return;
            }

            var v = ctx.ReadValue<Vector2>();

            switch (v.y)
            {
                case > 0.5f:
                    dialogueView.MoveChoice(-1);
                    break;
                
                case < -0.5f:
                    dialogueView.MoveChoice(1);
                    break;
            }
        }
#endif
    }
}