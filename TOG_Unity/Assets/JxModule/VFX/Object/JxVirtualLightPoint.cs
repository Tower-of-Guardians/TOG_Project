using UnityEngine;

namespace JxModule
{
    public class JxVirtualLightPoint : MonoBehaviour
    {
        public static JxVirtualLightPoint Instance { get; private set; }
        
        public Vector3 Position => transform.position;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("VirtualLightPoint: Virtual light point already exists. This is not allowed.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}