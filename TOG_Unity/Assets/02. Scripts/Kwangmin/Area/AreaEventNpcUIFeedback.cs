using UnityEngine;

[RequireComponent(typeof(InteractableObject))]
public sealed class AreaEventNpcUIFeedback : MonoBehaviour
{
    [SerializeField] private UIOutliner outliner;
    [SerializeField] private NameplateUI nameplate;
    [SerializeField] private InteractionTipUI interactionTip;

    private InteractableObject interactableObject;

    private void Awake()
    {
        interactableObject = GetComponent<InteractableObject>();

        if (outliner == null)
            outliner = GetComponentInChildren<UIOutliner>(true);

        if (nameplate == null)
            nameplate = GetComponentInChildren<NameplateUI>(true);

        if (interactionTip == null)
            interactionTip = GetComponentInChildren<InteractionTipUI>(true);
    }

    private void OnEnable()
    {
        if (interactableObject == null)
            interactableObject = GetComponent<InteractableObject>();

        interactableObject.OnMouseEnterAction += ShowFeedback;
        interactableObject.OnMouseExitAction += HideFeedback;
    }

    private void OnDisable()
    {
        if (interactableObject == null)
            return;

        interactableObject.OnMouseEnterAction -= ShowFeedback;
        interactableObject.OnMouseExitAction -= HideFeedback;
    }

    private void ShowFeedback()
    {
        outliner?.Show();
        nameplate?.Show();
        interactionTip?.Show();
    }

    private void HideFeedback()
    {
        outliner?.Hide();
        nameplate?.Hide();
        interactionTip?.Hide();
    }
}
