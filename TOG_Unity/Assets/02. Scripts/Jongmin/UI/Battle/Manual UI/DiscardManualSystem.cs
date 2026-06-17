using UnityEngine;

namespace Jongmin
{
    public class DiscardManualSystem : MonoBehaviour
    {
        private DiscardManualView _view;

        public void Construct(DiscardManualView view)
        {
            _view = view;
        }

        public void UpdateView(ActionData actionData, bool canDiscard)
        {
            _view.UpdateUI(actionData, canDiscard);
        }
    }
}
