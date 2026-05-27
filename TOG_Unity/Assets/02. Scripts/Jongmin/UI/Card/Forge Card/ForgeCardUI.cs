using UnityEngine;
using DG.Tweening;

public class ForgeCardUI : CardUI, IForgeCardUI
{
    [Header("Object References")]
    [SerializeField] private GameObject cardObject;
    [SerializeField] private GameObject atkLabelObject;
    [SerializeField] private GameObject defLabelObject;

    private Sequence upgradeSequenceAction;

    public void UpgradeATK(float atk)
    {
       UpdateATKTextLabel(atk);
       PlayATKUpgradeAnimation();
    }

    public void UpgradeBoth(float atk, float def)
    {
        UpdateATKTextLabel(atk);
        UpdateDEFTextLabel(def);
        PlayBothUpgradeAnimation();
    }

    public void UpgradeDEF(float def)
    {
        UpdateDEFTextLabel(def);
        PlayDEFUpgradeAnimation();
    }

    private void PlayATKUpgradeAnimation()
    {
        upgradeSequenceAction.Kill();
        upgradeSequenceAction = DOTween.Sequence();
        
        upgradeSequenceAction.Append(cardObject.transform.DOLocalMoveY(200, 0f));
        upgradeSequenceAction.AppendCallback(() => { _cardATKLabel.gameObject.SetActive(false); });
        upgradeSequenceAction.Append(cardObject.transform.DOLocalMoveY(150, 0.5f)).SetEase(Ease.OutQuad);
        upgradeSequenceAction.AppendInterval(0.3f);
        upgradeSequenceAction.AppendCallback(() => { _cardATKLabel.gameObject.SetActive(true); });
        upgradeSequenceAction.Append(_cardATKLabel.transform.DOScale(new Vector3(4f, 4f, 1f), 0f));
        upgradeSequenceAction.Append(_cardATKLabel.transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f));

        upgradeSequenceAction.Play();
    }

    private void PlayBothUpgradeAnimation()
    {
        upgradeSequenceAction.Kill();
        upgradeSequenceAction = DOTween.Sequence();
        
        upgradeSequenceAction.Append(cardObject.transform.DOLocalMoveY(200, 0f));
        upgradeSequenceAction.AppendCallback(() =>
        {
            _cardATKLabel.gameObject.SetActive(false);
            _cardDEFLabel.gameObject.SetActive(false);
        });
        
        upgradeSequenceAction.Append(cardObject.transform.DOLocalMoveY(150, 0.5f)).SetEase(Ease.OutQuad);
        upgradeSequenceAction.AppendInterval(0.3f);
        
        upgradeSequenceAction.AppendCallback(() => { _cardATKLabel.gameObject.SetActive(true); });
        upgradeSequenceAction.Append(_cardATKLabel.transform.DOScale(new Vector3(4f, 4f, 1f), 0f));
        upgradeSequenceAction.Append(_cardATKLabel.transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f));
        upgradeSequenceAction.AppendInterval(0.2f);
        
        upgradeSequenceAction.AppendCallback(() => { _cardDEFLabel.gameObject.SetActive(true); });
        upgradeSequenceAction.Append(_cardDEFLabel.transform.DOScale(new Vector3(4f, 4f, 1f), 0f));
        upgradeSequenceAction.Append(_cardDEFLabel.transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f));
        
        upgradeSequenceAction.Play();            
    }

    private void PlayDEFUpgradeAnimation()
    {
        upgradeSequenceAction.Kill();
        upgradeSequenceAction = DOTween.Sequence();
        
        upgradeSequenceAction.Append(cardObject.transform.DOLocalMoveY(200, 0f));
        upgradeSequenceAction.AppendCallback(() => { _cardDEFLabel.gameObject.SetActive(false); });
        upgradeSequenceAction.Append(cardObject.transform.DOLocalMoveY(150, 0.5f)).SetEase(Ease.OutQuad);
        upgradeSequenceAction.AppendInterval(0.3f);
        upgradeSequenceAction.AppendCallback(() => { _cardDEFLabel.gameObject.SetActive(true); });
        upgradeSequenceAction.Append(_cardDEFLabel.transform.DOScale(new Vector3(4f, 4f, 1f), 0f));
        upgradeSequenceAction.Append(_cardDEFLabel.transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f));

        upgradeSequenceAction.Play();       
    }

    private void UpdateATKTextLabel(float atk)
        => _cardATKLabel.text = atk.ToString();

    private void UpdateDEFTextLabel(float def)
        => _cardDEFLabel.text = def.ToString();
}
