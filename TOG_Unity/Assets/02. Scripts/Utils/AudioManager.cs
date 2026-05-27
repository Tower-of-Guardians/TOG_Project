using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AudioManager : Singleton<AudioManager>
{
    [Header("Audio Mixer & Groups")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioMixerGroup bgmGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private AudioMixerGroup uiGroup;

    // --- 지역 변수(필드)로 관리하는 믹서 키 ---
    private const string MasterKey = "MasterVol";
    private const string BGMKey = "BGMVol";
    private const string SFXKey = "SFXVol";
    private const string UIKey = "UIVol";

    [Header("Audio Resources (Auto Scanned)")]
    [SerializeField] private List<AudioData> audioDataList = new List<AudioData>();

    private Dictionary<string, AudioData> audioDictionary = new Dictionary<string, AudioData>();

    private AudioSource bgmSource;
    private AudioSource sfxSource;
    private AudioSource uiSource;

    protected override void Awake()
    {
        base.Awake();
        SetupAudioSources();
        InitializeDictionary();
    }

    private void Start() => LoadVolume();

    private void SetupAudioSources()
    {
        bgmSource = CreateSource("BGMSource", bgmGroup);
        sfxSource = CreateSource("SFXSource", sfxGroup);
        uiSource = CreateSource("UISource", uiGroup);
    }

    private AudioSource CreateSource(string goName, AudioMixerGroup group)
    {
        GameObject go = new GameObject(goName);
        go.transform.SetParent(transform);
        AudioSource source = go.AddComponent<AudioSource>();
        source.outputAudioMixerGroup = group;
        source.playOnAwake = false;
        return source;
    }

    private void InitializeDictionary()
    {
        audioDictionary.Clear();
        foreach (var data in audioDataList)
        {
            if (data != null && !audioDictionary.ContainsKey(data.audioName))
                audioDictionary.Add(data.audioName, data);
        }
    }

    // --- 재생 로직 ---
    public void PlayBGM(string audioName)
    {
        if (audioDictionary.TryGetValue(audioName, out AudioData data))
        {
            bgmSource.clip = data.clip;
            bgmSource.volume = data.volume;
            bgmSource.pitch = data.pitch;
            bgmSource.priority = data.priority; // 우선순위 적용
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    public void PlaySFX(string audioName) => PlayOneShot(sfxSource, audioName);
    public void PlayUI(string audioName) => PlayOneShot(uiSource, audioName);

    private void PlayOneShot(AudioSource source, string audioName)
    {
        if (audioDictionary.TryGetValue(audioName, out AudioData data))
        {
            source.priority = data.priority; // 재생 직전 우선순위 변경
            source.pitch = data.pitch;
            source.PlayOneShot(data.clip, data.volume);
        }
    }

    // --- 볼륨 설정 및 저장 ---
    public void SetMasterVolume(float v) => ApplyAndSave(MasterKey, v);
    public void SetBGMVolume(float v) => ApplyAndSave(BGMKey, v);
    public void SetSFXVolume(float v) => ApplyAndSave(SFXKey, v);
    public void SetUIVolume(float v) => ApplyAndSave(UIKey, v);

    private void ApplyAndSave(string key, float vol)
    {
        float db = Mathf.Log10(Mathf.Clamp(vol, 0.0001f, 1f)) * 20;
        audioMixer.SetFloat(key, db);
        PlayerPrefs.SetFloat(key, vol);
    }

    private void LoadVolume()
    {
        SetMasterVolume(PlayerPrefs.GetFloat(MasterKey, 0.75f));
        SetBGMVolume(PlayerPrefs.GetFloat(BGMKey, 0.75f));
        SetSFXVolume(PlayerPrefs.GetFloat(SFXKey, 0.75f));
        SetUIVolume(PlayerPrefs.GetFloat(UIKey, 0.75f));
    }

#if UNITY_EDITOR
    [ContextMenu("Refresh Audio Data List")]
    private void OnValidate()
    {
        // 실제 AudioData 파일들이 모여있는 폴더 경로로 설정하세요.
        string folderPath = "Assets/Sounds/AudioDatas";
        if (!AssetDatabase.IsValidFolder(folderPath)) return;

        string[] guids = AssetDatabase.FindAssets("t:AudioData", new[] { folderPath });
        audioDataList.Clear();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            audioDataList.Add(AssetDatabase.LoadAssetAtPath<AudioData>(path));
        }
        EditorUtility.SetDirty(this);
    }
#endif
}