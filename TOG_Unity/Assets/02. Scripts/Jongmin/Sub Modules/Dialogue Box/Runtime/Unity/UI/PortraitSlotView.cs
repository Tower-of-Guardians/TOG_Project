using JxModule;
using UnityEngine;

namespace JxDialogueBox
{
    public class PortraitSlotView : ImageView
    {
        [Space(20f), BigHeader("Default Settings")]
        [SerializeField] private string defaultKey = "Default";

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
            if (!Image)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(_characterID))
            {
                SetPortraitVisible(false);
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
            
            Image.sprite = sprite;
            SetPortraitVisible(sprite != null);
        }

        public bool IsPortraitEmpty()
        {
            return Image == null || Image.sprite == null;
        }

        public void SetColor(Color color)
        {
            if (!Image || !Image.sprite)
            {
                return;
            }

            Image.color = color;
        }

        private void SetPortraitVisible(bool isVisible)
        {
            if (Image == null)
            {
                return;
            }

            var color = Image.color;
            color.a = isVisible ? 1f : 0f;
            Image.color = color;
        }
    }
}
