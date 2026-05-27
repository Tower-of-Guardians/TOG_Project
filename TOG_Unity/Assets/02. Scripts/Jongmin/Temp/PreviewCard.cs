using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PreviewCard : MonoBehaviour
{
    private Animator m_animator;

    private void Awake()
        => m_animator = GetComponent<Animator>();

    private void OnEnable()
        => m_animator.SetTrigger("Enable");
}
