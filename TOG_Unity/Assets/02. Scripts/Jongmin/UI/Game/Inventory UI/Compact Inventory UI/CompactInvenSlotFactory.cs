using JxModule;
using UnityEngine;

namespace Jongmin
{
    public class CompactInvenSlotFactory
    {
        private readonly CompactInvenView _view;
        private readonly CompactInvenSlot _prefab;

        public CompactInvenSlotFactory(CompactInvenView view)
        {
            _view = view;
            _prefab = PrefabManager.CachePrefab<CompactInvenSlot>("PF_CompactInvenSlot");
        }
        
        public CompactInvenSlot Create(CompactInvenType invenType)
        {
            var slotObject = ObjectPoolManager.Instance.Get(_prefab.gameObject);
            slotObject.transform.SetParent(_view.CardRoot, false);
            slotObject.transform.localRotation = Quaternion.identity;
            slotObject.transform.localScale = Vector3.one;

            var invenSlot = slotObject.GetComponent<CompactInvenSlot>();
            return invenSlot;
        }

        public void Release(CompactInvenSlot slot)
        {
            ObjectPoolManager.Instance.Return(slot.gameObject);
        }
    }
}