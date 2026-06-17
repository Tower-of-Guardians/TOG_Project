using UnityEngine;

namespace Jongmin
{
    public class NotifyDomain : MonoBehaviour
    {
        public NotifySystem System { get; private set; }

        public void Construct()
        {
            System = new NotifySystem();
        }
    }
}