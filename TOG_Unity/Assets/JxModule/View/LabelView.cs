using TMPro;

namespace JxModule
{
    public class LabelView : ViewBase
    {
        private TMP_Text _label;
        public TMP_Text Label => _label ??= GetComponentInChildren<TMP_Text>();
    }
}