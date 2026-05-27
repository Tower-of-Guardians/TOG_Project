using UnityEngine;

[CreateAssetMenu(fileName = "NewAudioData", menuName = "Audio/Audio Data")]
public class AudioData : ScriptableObject
{
    public string audioName; // 클립 이름이 자동으로 들어갈 변수
    public AudioClip clip;

    [Range(0, 1)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;

    [Tooltip("0에 가까울수록 우선순위가 높습니다 (BGM: 0, UI: 50, SFX: 100 권장)")]
    [Range(0, 256)] public int priority = 128;

    public bool loop = false;

    // 유니티 인스펙터에서 값이 바뀔 때마다 호출되는 함수
    private void OnValidate()
    {
        // clip이 할당되어 있고, audioName이 비어있거나 clip 이름과 다를 때 자동 갱신
        if (clip != null && string.IsNullOrEmpty(audioName))
        {
            audioName = clip.name;
        }
    }
}