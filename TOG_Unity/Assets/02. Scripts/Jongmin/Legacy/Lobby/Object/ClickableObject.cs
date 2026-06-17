using UnityEngine;

[RequireComponent(typeof(InteractableObject), typeof(Animator))]
public class ClickableObject : MonoBehaviour
{
    protected InteractableObject m_interactable_object;
    private Animator m_animator;

    private static readonly int m_clicked_parameter = Animator.StringToHash("Clicked");

    private void Awake()
    {
        m_interactable_object = GetComponent<InteractableObject>();
        m_animator = GetComponent<Animator>();
    }

    protected virtual void OnEnable()
    {
        m_interactable_object.OnMouseDownAction += MouseDownAction;
        m_interactable_object.OnMouseUpAction += MouseUpAction;
    }

    protected virtual void OnDisable()
    {
        m_interactable_object.OnMouseDownAction -= MouseDownAction;
        m_interactable_object.OnMouseUpAction -= MouseUpAction;
    }

    private void MouseDownAction()
        => m_animator.SetBool(m_clicked_parameter, true);

    private void MouseUpAction()
        => m_animator.SetBool(m_clicked_parameter, false);
}
