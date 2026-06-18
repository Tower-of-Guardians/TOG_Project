using UnityEngine;

namespace Jongmin
{
    public class ManualDomain : MonoBehaviour
    {
        [SerializeField] private ActionManualView actionView;
        [SerializeField] private DiscardManualView discardView;
        [SerializeField] private ActionManualSystem actionSystem;
        [SerializeField] private DiscardManualSystem discardSystem;
        [SerializeField] private TurnManager turnManager;
        
        public void Construct()
        {
            actionSystem.Construct(actionView);
            discardSystem.Construct(discardView);
            
            BindEvents();
            turnManager.Initialize();
        }

        public void BindEvents()
        {
            turnManager.OnUpdatedActionCount += actionSystem.UpdateView;
            turnManager.OnUpdatedThrowCount += discardSystem.UpdateView;
        }

        public void ReleaseEvents()
        {
            turnManager.OnUpdatedActionCount -= actionSystem.UpdateView;
            turnManager.OnUpdatedThrowCount -= discardSystem.UpdateView;
        }

        private void OnDestroy()
        {
            ReleaseEvents();
        }
    }
}