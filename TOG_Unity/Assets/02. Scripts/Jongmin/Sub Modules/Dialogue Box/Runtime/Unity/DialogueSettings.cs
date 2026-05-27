using UnityEngine;
using UnityEngine.InputSystem;
using JxModule;

namespace JxDialogueBox
{
    [CreateAssetMenu(fileName = "Dialogue Settings", menuName = "Dialogue Box/Dialogue Settings")]
    public sealed class DialogueSettings : ScriptableObject
    {
        [BigHeader("Typewriter")]
        public bool typingEnabled = true;

        [Min(0f)]
        [ShowIf("typingEnabled")]
        public float typingSecondsPerCharacter = 0.03f;
        
        [ShowIf("typingEnabled")]
        public bool typingSkipAllowed = true;

        [Space(30f), BigHeader("Input")]
        public bool keyboardInputAllowed = true;

        #if ENABLE_INPUT_SYSTEM
        [ShowIf("keyboardInputAllowed")]
        public InputActionReference advanceAction;
        #endif

        #if ENABLE_INPUT_SYSTEM
        [ShowIf("keyboardInputAllowed")]
        public InputActionReference selectionAction;
        #endif

        [Space(30f), BigHeader("Auto Advance")]
        public bool autoAdvanceAllowed;

        [Min(0f), ShowIf("autoAdvanceAllowed")]
        public float autoAdvanceDelay = 3f;
    }
}

