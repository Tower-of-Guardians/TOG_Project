using System;
using System.Collections;
using JxModule;
using UnityEngine;

namespace Jongmin
{
    public class ResultDomain : MonoBehaviour
    {
        [SerializeField] private ResultView resultView;
        [SerializeField] private RewardView rewardView;
        [SerializeField] private GachaView gachaView;

        [SerializeField] private GachaSystem gachaSystem;
        [SerializeField] private GachaEventSystem gachaEventSystem;
        [SerializeField] private CardInfoDomain cardInfoDomain;
        [SerializeField] private CompactInvenDomain compactInvenDomain;

        private GachaSlotFactory _slotFactory;

        public void OnGUI()
        {
            if (GUI.Button(new Rect(new Vector2(1800, 150), new Vector2(100, 50)), "Result"))
            {
                TempShow();
            }
        }

        [Button("Test")]
        public void TempShow()
        {
            var resultData = new ResultData(46, 82, true);
            StartCoroutine(ShowRoutine(resultData));
        }
        
        public void Construct()
        {
            _slotFactory = new GachaSlotFactory(gachaView, gachaEventSystem);
            gachaSystem.Construct(gachaView, _slotFactory);
            
            BindEvents();
        }
        
        public void BindEvents()
        {
            resultView.Bind(this);
            gachaView.Bind(gachaSystem);

            DataCenter.Instance.playerMoneyEvent += gachaSystem.UpdateSlotsState;
            DataCenter.Instance.playerMoneyEvent += gachaSystem.UpdateRefreshState;

            gachaEventSystem.RequestShowCardInfo += HandleRequestShowCardInfo;
        }

        public void ReleaseEvents()
        {
            DataCenter.Instance.playerMoneyEvent -= gachaSystem.UpdateSlotsState;
            DataCenter.Instance.playerMoneyEvent -= gachaSystem.UpdateRefreshState;
            
            gachaEventSystem.RequestShowCardInfo -= HandleRequestShowCardInfo;
        }

        public void Show(ResultData resultData)
        {
            StartCoroutine(ShowRoutine(resultData));
        }

        public void Hide()
        {
            rewardView.Hide();
            gachaSystem.CloseView();
            resultView.Hide();
            compactInvenDomain.CloseView();
        }

        private void HandleRequestShowCardInfo(CardData cardData)
        {
            cardInfoDomain.System.OpenView(cardData);
        }

        private IEnumerator ShowRoutine(ResultData resultData)
        {
            yield return resultView.Show();
            yield return new WaitForSeconds(0.5f);
            yield return rewardView.Show(resultData.Gold, resultData.Exp, resultData.IsLevelUp);
            yield return new WaitForSeconds(1f);
            yield return gachaSystem.OpenView();
            compactInvenDomain.OpenView(CompactInvenType.Result);
            resultView.ShowCloseButton();
        }
        
        private void OnDestroy()
        {
            ReleaseEvents();
        }
    }
}