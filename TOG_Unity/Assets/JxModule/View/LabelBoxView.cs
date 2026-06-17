using TMPro;
using UnityEngine;

namespace JxModule
{
    public class LabelBoxView : ImageView
    {
        [SerializeField] private TMP_Text label;
        public TMP_Text Label => label ??= GetComponentInChildren<TMP_Text>();
    }
}