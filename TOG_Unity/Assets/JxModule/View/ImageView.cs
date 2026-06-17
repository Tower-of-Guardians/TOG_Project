using UnityEngine.UI;

namespace JxModule
{
    public class ImageView : ViewBase
    {
        private Image _image;
        public Image Image => _image ??= GetComponent<Image>();
    }
}