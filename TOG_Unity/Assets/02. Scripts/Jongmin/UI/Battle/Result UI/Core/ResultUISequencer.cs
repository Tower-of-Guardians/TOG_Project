using System.Collections;
using DG.Tweening;
using UnityEngine;
using VContainer;

public class ResultUISequencer : MonoBehaviour
{
    private ResultPresenter _resultPresenter;
    private ResultRewardPresenter _resultRewardPresenter;
    private ResultShopPresenter _resultShopPresenter;
    private ResultDeckInvenPresenter _resultDeckInvenPresenter;

    [Inject]
    private void Construct(ResultPresenter resultPresenter,
                           ResultRewardPresenter resultRewardPresenter,
                           ResultShopPresenter resultShopPresenter,
                           ResultDeckInvenPresenter resultDeckInvenPresenter)
    {
        _resultPresenter = resultPresenter;
        _resultRewardPresenter = resultRewardPresenter;
        _resultShopPresenter = resultShopPresenter;
        _resultDeckInvenPresenter = resultDeckInvenPresenter;
    }
    
    /// <summary>
    /// 테스트용 메서드
    /// </summary>
    public void PlaySequence()
        => PlaySequence(new ResultData(86, 46));

    /// <summary>
    /// ResultData에 따라 Result UI 애니메이션 시퀀스를 실행합니다.
    /// </summary>
    public void PlaySequence(ResultData resultData)
        => StartCoroutine(PlayRoutine(resultData));

    private IEnumerator PlayRoutine(ResultData resultData)
    {
        _resultPresenter.InitUI();
        _resultRewardPresenter.InitUI();
        _resultShopPresenter.InitUI();

        _resultPresenter.OpenUI(resultData);
        yield return new WaitForSeconds(1.5f);

        _resultRewardPresenter.OpenUI(resultData.Gold, resultData.EXP);
        yield return new WaitForSeconds(2.5f);

        _resultDeckInvenPresenter.OpenUI();
        _resultShopPresenter.OpenUI();
        yield return new WaitForSeconds(1f);
        
        _resultPresenter.ShowCloseButton();
    }
}
