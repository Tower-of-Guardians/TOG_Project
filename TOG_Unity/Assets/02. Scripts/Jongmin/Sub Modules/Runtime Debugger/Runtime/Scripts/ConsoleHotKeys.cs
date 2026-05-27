using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RuntimeLogger
{
    public class ConsoleHotkeys : MonoBehaviour
    {
        [SerializeField] private InputActionReference m_toggle;
        [SerializeField] private InputActionReference m_clear;

        public event Action ToggleRequested;
        public event Action ClearRequested;

        private void OnEnable()
        {
            Bind(m_toggle, OnToggle);
            Bind(m_clear, OnClear);
        }

        private void OnDisable()
        {
            Unbind(m_toggle, OnToggle);
            Unbind(m_clear, OnClear);
        }

        private void Bind(InputActionReference a, Action<InputAction.CallbackContext> cb)
        {
            if (a == null) return;
            a.action.performed += cb;
            a.action.Enable();
        }

        private void Unbind(InputActionReference a, Action<InputAction.CallbackContext> cb)
        {
            if (a == null) return;
            a.action.performed -= cb;
            a.action.Disable();
        }

        private void OnToggle(InputAction.CallbackContext _) => ToggleRequested?.Invoke();
        private void OnClear(InputAction.CallbackContext _) => ClearRequested?.Invoke();

        public string ToggleBinding() => m_toggle ? m_toggle.action.GetBindingDisplayString() : "Not set";
        public string ClearBinding() => m_clear ? m_clear.action.GetBindingDisplayString() : "Not set";
    }
}