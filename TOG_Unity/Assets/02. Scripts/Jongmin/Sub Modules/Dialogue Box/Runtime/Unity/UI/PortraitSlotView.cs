using JxModule;
using UnityEngine;
using UnityEngine.UI;

namespace JxDialogueBox
{
    public class PortraitSlotView : MonoBehaviour
    {
        [BigHeader("UI")]
        [SerializeField] private Image portraitImage;
        
        [Space(20f), BigHeader("Default Settings")]
        [SerializeField] private string defaultKey = "default";

        private static DialoguePortraitTable _portraitTable;
        private string _characterID;

        public string CharacterID => _characterID;

        private void Awake()
        {
            _portraitTable ??= new DialoguePortraitTable();
        }

        public void SetCharacter(string characterID, bool forceRefresh = false)
        {
            if (string.IsNullOrWhiteSpace(characterID))
            {
                return;
            }

            if (!forceRefresh && _characterID == characterID)
            {
                return;
            }

            _characterID = characterID;
            SetPortraitByKey(defaultKey);
        }

        public void SetPortraitByKey(string key)
        {
            if (portraitImage == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(_characterID))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                key = defaultKey;
            }

            var sprite = _portraitTable.GetPortraitSprite(_characterID, key);

            if (sprite == null && key != defaultKey)
            {
                sprite = _portraitTable.GetPortraitSprite(_characterID, defaultKey);
            }
            
            portraitImage.sprite = sprite;
        }

        public bool IsPortraitEmpty()
        {
            return portraitImage == null || portraitImage.sprite == null;
        }

        public void SetAlpha(float alpha)
        {
            if (portraitImage == null)
            {
                return;
            }

            var color = portraitImage.color;
            color.a = alpha;
            portraitImage.color = color;
        }
    }
}