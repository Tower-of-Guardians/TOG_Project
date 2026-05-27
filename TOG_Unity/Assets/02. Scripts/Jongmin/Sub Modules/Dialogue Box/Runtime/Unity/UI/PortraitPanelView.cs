using UnityEngine;

namespace JxDialogueBox
{
    public class PortraitPanelView : MonoBehaviour
    {
        [Header("Player")]
        [SerializeField] private PortraitSlotView playerSlot;

        [Header("NPC")]
        [SerializeField] private PortraitSlotView npcSlot;

        [Header("Alpha")]
        [Range(0f, 1f)][SerializeField] private float activeAlpha = 1f;
        [Range(0f, 1f)][SerializeField] private float deactiveAlpha = 0.35f;

        private void Start()
        {
            playerSlot.SetCharacter("eccliss");
            playerSlot.SetPortraitByKey("default");
        }

        public void ApplySpeaker(SpeakerRef speaker, string portraitKey)
        {
            if (speaker.Speaker == Speaker.Player)
            {
                if (playerSlot)
                {
                    playerSlot.SetAlpha(activeAlpha);

                    if (!string.IsNullOrEmpty(portraitKey))
                    {
                        playerSlot.SetPortraitByKey(portraitKey);
                    }
                }

                if (npcSlot)
                {
                    npcSlot.SetAlpha(deactiveAlpha);
                }

                return;
            }

            if (playerSlot)
            {
                playerSlot.SetAlpha(deactiveAlpha);
            }

            if (npcSlot)
            {
                npcSlot.SetAlpha(activeAlpha);
                npcSlot.SetCharacter(speaker.CharacterID);

                if (!string.IsNullOrEmpty(portraitKey))
                {
                    npcSlot.SetPortraitByKey(portraitKey);
                }
            }
        }
    }
}

