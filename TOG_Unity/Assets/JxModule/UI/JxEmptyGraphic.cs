using UnityEngine.UI;

namespace JxModule
{
    public class JxEmptyGraphic : Graphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }
    }
}