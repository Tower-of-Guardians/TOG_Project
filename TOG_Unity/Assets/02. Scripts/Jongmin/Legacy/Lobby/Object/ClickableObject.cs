using UnityEngine;

[RequireComponent(typeof(InteractableObject), typeof(Animator))]
public class ClickableObject : MonoBehaviour
{
    protected InteractableObject m_interactable_object;
    private Animator m_animator;
    private bool m_has_clicked_parameter;

    private static readonly int m_clicked_parameter = Animator.StringToHash("Clicked");

    private void Awake()
    {
        m_interactable_object = GetComponent<InteractableObject>();
        m_animator = GetComponent<Animator>();

        foreach (AnimatorControllerParameter parameter in m_animator.parameters)
        {
            if (parameter.nameHash != m_clicked_parameter || parameter.type != AnimatorControllerParameterType.Bool)
                continue;

            m_has_clicked_parameter = true;
            break;
        }
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
    {
        if (m_has_clicked_parameter)
            m_animator.SetBool(m_clicked_parameter, true);
    }

    private void MouseUpAction()
    {
        if (m_has_clicked_parameter)
            m_animator.SetBool(m_clicked_parameter, false);
    }
}
