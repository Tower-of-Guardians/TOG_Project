using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI Image에 스프라이트 프레임 배열을 무한 루프로 재생합니다.
/// Time.time 기준으로 프레임을 계산해, 재설정 시에도 애니메이션이 처음부터 다시 시작하지 않습니다.
/// </summary>
[DisallowMultipleComponent]
public class UIImageFrameAnimator : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    [SerializeField] private float playDuration = 1f;

    private Sprite[] frames;
    private Coroutine playCoroutine;
    private bool isPlayRequested;
    private int lastAppliedFrameIndex = -1;

    public void Configure(Image image, Sprite[] spriteFrames, float playSeconds)
    {
        targetImage = image;
        frames = spriteFrames;
        playDuration = Mathf.Max(0.01f, playSeconds);

        if (isPlayRequested)
        {
            ApplyCurrentFrame(force: true);
        }
    }

    public void Play()
    {
        if (targetImage == null || frames == null || frames.Length == 0)
        {
            return;
        }

        isPlayRequested = true;
        ApplyCurrentFrame(force: true);
        TryStartPlay();
    }

    public void Stop()
    {
        isPlayRequested = false;
        lastAppliedFrameIndex = -1;
        StopInternal();
    }

    private void StopInternal()
    {
        if (playCoroutine != null)
        {
            StopCoroutine(playCoroutine);
            playCoroutine = null;
        }
    }

    private void TryStartPlay()
    {
        if (!isPlayRequested || playCoroutine != null || !isActiveAndEnabled)
        {
            return;
        }

        playCoroutine = StartCoroutine(PlayLoop());
    }

    private IEnumerator PlayLoop()
    {
        while (isPlayRequested)
        {
            ApplyCurrentFrame(force: false);
            yield return null;
        }
    }

    private int GetCurrentFrameIndex()
    {
        if (frames == null || frames.Length == 0)
        {
            return 0;
        }

        float elapsed = Time.time % playDuration;
        int frameIndex = Mathf.FloorToInt(elapsed / playDuration * frames.Length);
        return Mathf.Clamp(frameIndex, 0, frames.Length - 1);
    }

    private void ApplyCurrentFrame(bool force)
    {
        if (targetImage == null || frames == null || frames.Length == 0)
        {
            return;
        }

        int frameIndex = GetCurrentFrameIndex();
        if (!force && frameIndex == lastAppliedFrameIndex)
        {
            return;
        }

        Sprite frame = frames[frameIndex];
        if (frame != null)
        {
            targetImage.sprite = frame;
        }

        lastAppliedFrameIndex = frameIndex;
    }

    private void OnEnable()
    {
        if (isPlayRequested)
        {
            ApplyCurrentFrame(force: true);
        }

        TryStartPlay();
    }

    private void OnDisable()
    {
        StopInternal();
    }
}
