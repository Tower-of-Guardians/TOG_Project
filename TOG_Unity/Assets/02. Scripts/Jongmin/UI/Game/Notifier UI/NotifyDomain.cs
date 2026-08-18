using UnityEngine;

namespace Jongmin
{
    public class NotifyDomain : MonoBehaviour
    {
        [SerializeField] private NotifySystem notifySystem;

        public NotifySystem System => notifySystem;
    }
}