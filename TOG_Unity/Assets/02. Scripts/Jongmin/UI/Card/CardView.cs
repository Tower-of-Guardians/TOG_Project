using JxModule;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Jongmin
{
    public class CardView : MonoBehaviour
    {
        [BigHeader("Model")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image cardFrameImage;
        [SerializeField] private Image cardItemImage;
        [SerializeField] private TMP_Text cardNameLabel;
        [SerializeField] private TMP_Text cardDescriptionLabel;
        [SerializeField] protected TMP_Text cardAtkLabel;
        [SerializeField] protected TMP_Text cardDefLabel;
        [SerializeField] private Image[] starObjectArray;
        [SerializeField] private Image[] synergyImageArray;
        [SerializeField] private GameObject selectImage;
        [SerializeField] private Image atkLockImage;
        [SerializeField] private Image defLockImage;

        private const int MaxStarCount = 2;

        public RectTransform RectTransform { get; private set; }
        public CanvasGroup CanvasGroup => canvasGroup;

        private void Awake()
        {
            RectTransform = transform as RectTransform;
        }

        /// <summary>
        /// CardData를 이용하여 카드의 비주얼을 업데이트합니다.
        /// </summary>
        public virtual void UpdateModel(CardData cardData)
        {
            if(cardData == null)
            {
                Debug.LogWarning("CardUI: 전달된 cardData가 null입니다.");
                return;
            }

            CanvasGroup.alpha = 1f;
            
            atkLockImage.gameObject.SetActive(false);
            defLockImage.gameObject.SetActive(false);

            InitCardInfo(cardData);
            InitCardStars(cardData);
            InitCardSynergies(cardData);
            UpdateSelect(false);
        }

        public void ToggleLock()
        {
            atkLockImage.gameObject.SetActive(!atkLockImage.gameObject.activeInHierarchy);
            defLockImage.gameObject.SetActive(!defLockImage.gameObject.activeInHierarchy);
        }

        public void LockAtk()
        {
            atkLockImage.gameObject.SetActive(true);
            defLockImage.gameObject.SetActive(false);
        }

        public void LockDef()
        {
            atkLockImage.gameObject.SetActive(false);
            defLockImage.gameObject.SetActive(true);
        }

        public void UpdateSelect(bool isSelect)
        {
            selectImage.gameObject.SetActive(isSelect);
        }

        private void InitCardInfo(CardData cardData)
        {
            cardFrameImage.sprite = cardData.cardimage;
            cardItemImage.sprite = cardData.iconimage;

            cardNameLabel.text = cardData.itemName;
            cardDescriptionLabel.text = cardData.effectDescription;
            cardAtkLabel.text = $"{cardData.ATK}";
            cardDefLabel.text = $"{cardData.DEF}";
        }

        private void InitCardStars(CardData cardData)
        {
            foreach(var starImage in starObjectArray)
            {
                starImage.gameObject.SetActive(false);
            }

            var starCount = Mathf.Min(starObjectArray.Length, Mathf.Min(MaxStarCount, cardData.star));
            for(var i = 0; i < starCount; i++)
            {
                starObjectArray[i].gameObject.SetActive(true);
            }
        }

        private void InitCardSynergies(CardData cardData)
        {
            foreach(var synergyImage in synergyImageArray)
            {
                synergyImage.gameObject.SetActive(false);
            }

            // TODO: 시너지 데이터를 채워야 합니다.
        }
    }
}