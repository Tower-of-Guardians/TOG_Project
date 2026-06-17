using JxModule;

namespace Jongmin
{
    public class GachaSlotFactory
    {
        private readonly GachaView _gachaView;
        private readonly GachaEventSystem _gachaEventSystem;
        private readonly GachaSlot _prefab;
        
        public GachaSlotFactory(GachaView gachaView, GachaEventSystem gachaEventSystem)
        {
            _gachaView = gachaView;
            _gachaEventSystem = gachaEventSystem;
            _prefab = PrefabManager.CachePrefab<GachaSlot>("PF_GachaSlot");
        }

        public GachaSlot Create()
        {
            var slotObject = ObjectPoolManager.Instance.Get(_prefab.gameObject);
            slotObject.transform.SetParent(_gachaView.CardRoot);
            
            var gachaSlot = slotObject.GetComponent<GachaSlot>();
            _gachaEventSystem.Subscribe(gachaSlot.Card);
            return gachaSlot;
        }

        public void Release(GachaSlot gachaSlot)
        {
            _gachaEventSystem.Unsubscribe(gachaSlot.Card);
            ObjectPoolManager.Instance.Return(gachaSlot.gameObject);
        }
    }
}