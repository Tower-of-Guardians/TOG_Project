using System.Collections;
using DG.Tweening;
using JxModule;
using UnityEngine;

namespace Jongmin
{
    public class RewardView : ViewBase
    {
        [SerializeField] private LabelBoxView goldLabel;
        [SerializeField] private LabelBoxView expLabel;
        [SerializeField] private LabelView levelUpLabel;

        private bool _isLevelUp;
        private Tween _toggleTween;

        public IEnumerator Show(int gold, int exp, bool isLevelUp)
        {
            ResetBeforeEffect();
            SetReward(gold, exp, isLevelUp);
            
            _toggleTween?.Kill();
            
            var sequence = DOTween.Sequence();
            sequence.Join(CanvasGroup.DOFade(1f, 0.5f));
            sequence.Append(goldLabel.CanvasGroup.DOFade(1f, 0.5f));
            sequence.Append(expLabel.CanvasGroup.DOFade(1f, 0.5f));

            _toggleTween = sequence;
            yield return _toggleTween.WaitForCompletion();
            
            ToggleLevelUpIndicator(_isLevelUp);
        }

        public void Hide()
        {
            _toggleTween?.Kill();
            _toggleTween = CanvasGroup.DOFade(0f, 0.5f).OnComplete(() =>
            {
                goldLabel.CanvasGroup.Hide();
                expLabel.CanvasGroup.Hide();
                ToggleLevelUpIndicator(false);
            });
        }

        private void ResetBeforeEffect()
        {
            CanvasGroup.Hide();
            goldLabel.CanvasGroup.Hide();
            expLabel.CanvasGroup.Hide();
            ToggleLevelUpIndicator(false);
        }

        private void SetReward(int gold, int exp, bool isLevelUp)
        {
            goldLabel.Label.text = $"· 골드 +{gold}";
            expLabel.Label.text = $"· 경험치 +{exp}";
            _isLevelUp = isLevelUp;
        }

        private void ToggleLevelUpIndicator(bool isLevelUp)
        {
            levelUpLabel.transform?.DOKill();
            
            if (isLevelUp)
            {
                levelUpLabel.gameObject.SetActive(true);
                levelUpLabel.transform.DOLocalJump(new Vector3(100f, -2f, 0f), 10f, 2, 0.5f).SetDelay(0.5f).SetLoops(-1);
            }
            else
            {
                levelUpLabel.transform.localPosition = new Vector3(100f, -5f, 0f);
                levelUpLabel.gameObject.SetActive(false);
            }
        }
    }
}