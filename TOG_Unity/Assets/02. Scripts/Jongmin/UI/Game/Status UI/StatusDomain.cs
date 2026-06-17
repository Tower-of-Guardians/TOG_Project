using UnityEngine;

namespace Jongmin
{
    public class StatusDomain : MonoBehaviour
    {
        [SerializeField] private StatusView statusView;
        [SerializeField] private StatusSystem statusSystem;

        private void Awake()
        {
            Construct();
        }
        
        public void Construct()
        {
            statusSystem.Construct(statusView);
            
            BindEvents();
        }

        public void BindEvents()
        {
            DataCenter.Instance.playerLevelEvent += statusSystem.UpdateLevel;
            DataCenter.Instance.playerMoneyEvent += statusSystem.UpdateGold;
            DataCenter.Instance.playerStateEvent += statusSystem.Initialize;
            DataCenter.Instance.SetPlayerState();
        }

        public void ReleaseEvents()
        {
            DataCenter.Instance.playerLevelEvent -= statusSystem.UpdateLevel;
            DataCenter.Instance.playerMoneyEvent -= statusSystem.UpdateGold;
            DataCenter.Instance.playerStateEvent -= statusSystem.Initialize;
        }

        private void OnDestroy()
        {
            ReleaseEvents();
        }
    }
}