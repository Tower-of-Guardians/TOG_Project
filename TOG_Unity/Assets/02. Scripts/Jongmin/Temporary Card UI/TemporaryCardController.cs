using System;
using System.Collections;
using UnityEngine;

public class TemporaryCardController : MonoBehaviour
{
    [SerializeField] private TemporaryCardAnimator m_animator;

    public event Action<BattleCardData> OnCardAnimationBegin; 
    public event Action<BattleCardData> OnCardAnimationEnd;
    public event Action OnFinalAnimationEnd;

    private Coroutine m_running;

    public void Play(TemporaryCardAnimeRequest req)
    {
        if (m_running != null) StopCoroutine(m_running);
        m_running = StartCoroutine(Co_Play(req));
    }

    private IEnumerator Co_Play(TemporaryCardAnimeRequest req)
    {
        int startedCount = 0;
        int completedCount = 0;

        for (int i = 0; i < req.CardDatas.Length; i++)
        {
            if (req.Interval > 0f)
                yield return new WaitForSeconds(req.Interval);

            var s = req.GetSettings(i);

            m_animator.AnimateOne(
                req.TargetRoot,
                req.CardDatas[i],
                req.GetStartPosition(i),
                req.EndPosition,
                req.GetStartRotation(i),
                s,
                d =>
                {
                    startedCount++;
                    OnCardAnimationBegin?.Invoke(d);
                },
                d =>
                {
                    completedCount++;
                    OnCardAnimationEnd?.Invoke(d);
                }
            );
        }

        if (startedCount > 0)
        {
            yield return new WaitUntil(() => completedCount >= startedCount);
        }

        OnFinalAnimationEnd?.Invoke();

        m_running = null;
    }
}
