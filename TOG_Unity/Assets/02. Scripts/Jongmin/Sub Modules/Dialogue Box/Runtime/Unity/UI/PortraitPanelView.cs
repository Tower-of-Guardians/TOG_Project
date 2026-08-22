using DG.Tweening;
using Jongmin;
using JxModule;
using UnityEngine;

namespace JxDialogueBox
{
    public class PortraitPanelView : MonoBehaviour
    {
        [BigHeader("UI")]
        [Header("Player")]
        [SerializeField] private PortraitSlotView playerSlot;

        [Header("NPC")]
        [SerializeField] private PortraitSlotView npcSlot;

        [Header("Alpha")]
        [SerializeField] private Color activeColor = Color.white;
        [SerializeField] private Color inactiveColor = new (0.3f, 0.3f, 0.3f, 1f);

        [Space(30f)]
        [BigHeader("Effect")]
        [SerializeField] private PortraitEffect portraitEffect;
        
        private Vector2 _playerSlotOriginAnchoredPosition;
        private Vector2 _npcSlotOriginAnchoredPosition;
        
        private void Awake()
        {
            _playerSlotOriginAnchoredPosition = playerSlot.RectTransform.anchoredPosition;
            _npcSlotOriginAnchoredPosition = npcSlot.RectTransform.anchoredPosition;
        }

        private void Start()
        {
            if (playerSlot == null)
            {
                return;
            }

            playerSlot.SetCharacter("Eclis");
            playerSlot.SetPortraitByKey("default");
        }

        public Tween Show()
        {
            return portraitEffect.PlayShowEffect(playerSlot.RectTransform, 
                                                 npcSlot.RectTransform, 
                                                 _playerSlotOriginAnchoredPosition, 
                                                 _npcSlotOriginAnchoredPosition);
        }

        public Tween Hide()
        {
            return portraitEffect.PlayHideEffect(playerSlot.RectTransform,
                                                 npcSlot.RectTransform, 
                                                 _playerSlotOriginAnchoredPosition, 
                                                 _npcSlotOriginAnchoredPosition);
        }

        public void SetActiveColor()
        {
            if (playerSlot)
            {
                playerSlot.SetColor(activeColor);
            }

            if (npcSlot)
            {
                npcSlot.SetColor(activeColor);
            }
        }

        public void PrepareSpeaker(SpeakerRef speaker, string portraitKey)
        {
            if (speaker.Speaker == Speaker.Player)
            {
                if (playerSlot)
                {
                    playerSlot.SetColor(activeColor);

                    if (!string.IsNullOrEmpty(portraitKey))
                    {
                        playerSlot.SetPortraitByKey(portraitKey);
                    }
                }

                return;
            }

            if (npcSlot)
            {
                npcSlot.SetColor(activeColor);
                npcSlot.SetCharacter(speaker.CharacterID);

                if (!string.IsNullOrEmpty(portraitKey))
                {
                    npcSlot.SetPortraitByKey(portraitKey);
                }
            }
        }

        public void ApplySpeaker(SpeakerRef speaker, string portraitKey)
        {
            if (speaker.Speaker == Speaker.Player)
            {
                if (playerSlot)
                {
                    playerSlot.SetColor(activeColor);

                    if (!string.IsNullOrEmpty(portraitKey))
                    {
                        playerSlot.SetPortraitByKey(portraitKey);
                    }
                }

                if (npcSlot)
                {
                    npcSlot.SetColor(inactiveColor);
                }

                return;
            }

            if (playerSlot)
            {
                playerSlot.SetColor(inactiveColor);
            }

            if (npcSlot)
            {
                npcSlot.SetColor(activeColor);
                npcSlot.SetCharacter(speaker.CharacterID);

                if (!string.IsNullOrEmpty(portraitKey))
                {
                    npcSlot.SetPortraitByKey(portraitKey);
                }
            }
        }
    }
}

