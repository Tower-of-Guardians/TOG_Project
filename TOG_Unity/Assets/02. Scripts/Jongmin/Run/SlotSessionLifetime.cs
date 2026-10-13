using UnityEngine;

namespace Jongmin
{
    public sealed class SlotSessionLifetime : MonoBehaviour
    {
        private double _nextSave;

        private void OnEnable()
        {
            _nextSave = Time.realtimeSinceStartupAsDouble + 30;
        }
        
        private void Update()
        {
            if (Time.realtimeSinceStartupAsDouble < _nextSave)
            {
                return;
            }
            
            RunSaveService.SaveSession();
            _nextSave = Time.realtimeSinceStartupAsDouble + 30;
        }
        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                RunSaveService.SaveSession();
            }
        }
        private void OnApplicationQuit() => RunSaveService.EndSession();
    }
}
