using UnityEngine;

namespace Jongmin
{
    [System.Serializable]
    public class CardLayoutData
    {
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;

        public CardLayoutData(Vector3 position, Vector3 rotation, Vector3 scale)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }
    }
}