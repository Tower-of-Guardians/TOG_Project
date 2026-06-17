using UnityEngine;

namespace Jongmin
{
    public class ActionManualSystem : MonoBehaviour
    {
        private ActionManualView _view;

        public void Construct(ActionManualView view)
        {
            _view = view;
        }

        public void UpdateView(ActionData actionData, bool canAction)
        {
            _view.UpdateUI(actionData, canAction);
        }
    }
}
