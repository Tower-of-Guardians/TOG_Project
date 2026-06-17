using JxModule;

namespace Jongmin
{
    public class NotifySystem
    {
        private readonly NotifyView _prefab;

        public NotifySystem()
        {
            _prefab = PrefabManager.CachePrefab<NotifyView>("PF_NotifyView");
        }

        public void Notify(string text)
        {
            var notifyObject = ObjectPoolManager.Instance.Get(_prefab.gameObject);
            var notifyView = notifyObject.GetComponent<NotifyView>();
            if (notifyView == null)
            {
                return;
            }
            
            notifyView.Label.text = text;
        }
    }
}