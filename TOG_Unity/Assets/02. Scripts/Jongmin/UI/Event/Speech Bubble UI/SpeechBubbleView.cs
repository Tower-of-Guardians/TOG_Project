using DG.Tweening;
using JxModule;
using TMPro;

namespace Jongmin
{
    public class SpeechBubbleView : LabelBoxView
    {
        private Tween _typingTween;
        
        public void SetLineLabel(string lineText)
        {
            TypeText(Label, lineText);
        }

        private void TypeText(TMP_Text label, string lineText)
        {
            _typingTween?.Kill();

            label.text = lineText ?? string.Empty;
            
            label.ForceMeshUpdate();

            var visibleCount = label.textInfo.characterCount;

            label.maxVisibleCharacters = 0;

            var duration = visibleCount / 25f;

            _typingTween = DOTween.To(
                    () => label.maxVisibleCharacters,
                    x => label.maxVisibleCharacters = x,
                    visibleCount,
                    duration
                )
                .SetEase(Ease.Linear)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    label.maxVisibleCharacters = int.MaxValue;
                    _typingTween = null;
                });
        }
    }
}