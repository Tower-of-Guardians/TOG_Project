using UnityEngine;

namespace Jongmin
{
    public class CompactInvenDomain : MonoBehaviour
    {
        [SerializeField] private CompactInvenView invenView;
        [SerializeField] private ResultInvenSystem resultInvenSystem;
        [SerializeField] private CraftmanInvenSystem craftmanInvenSystem;
        
        private CompactInvenSlotFactory _slotFactory;
        
        public CompactInvenSystem System { get; private set; }

        public void Construct()
        {
            _slotFactory = new CompactInvenSlotFactory(invenView);
            
            resultInvenSystem.Construct(invenView, _slotFactory);
            craftmanInvenSystem.Construct(invenView, _slotFactory);

            BindEvents();
        }

        public void BindEvents()
        {
            
        }

        public void ReleaseEvents()
        {
            
        }
        
        public void OpenView(CompactInvenType inventoryType)
        {
            ChangeSystem(inventoryType);
            System?.OpenView();
        }

        public void CloseView(bool systemClear = true)
        {
            System?.CloseView();

            if (systemClear)
            {
                System = null;
            }
        }

        private void ChangeSystem(CompactInvenType inventoryType)
        {
            if (System != null)
            {
                System.CloseView();
            }

            System = GetInvenSystem(inventoryType);
        }

        private CompactInvenSystem GetInvenSystem(CompactInvenType inventoryType)
        {
            return inventoryType switch
            {
                CompactInvenType.Result => resultInvenSystem,
                CompactInvenType.Craftman => craftmanInvenSystem,
                _ => null
            };
        }

        private void OnDestroy()
        {
            ReleaseEvents();
        }
    }
}