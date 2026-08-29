using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : Singleton<LoadingManager>
{
    [Header("로딩 UI의 캔버스 그룹")]
    [SerializeField] private CanvasGroup m_canvas_group;

    [Header("로딩 상태를 표현 할 라벨")]
    [SerializeField] private TMP_Text m_loading_label;

    private string m_target_scene_name;
    private string m_previous_active_scene_name;
    private LoadSceneMode m_load_scene_mode = LoadSceneMode.Single;

    public string Scene
    {
        get { return m_target_scene_name; }
    }

    public void LoadScene(string scene_name)
    {
        LoadScene(scene_name, LoadSceneMode.Single);
    }

    public void LoadSceneAdditive(string scene_name)
    {
        Scene scene = SceneManager.GetSceneByName(scene_name);

        if (scene.IsValid() && scene.isLoaded)
        {
            SceneManager.SetActiveScene(scene);
            return;
        }

        LoadScene(scene_name, LoadSceneMode.Additive);
    }

    public void UnloadAdditiveScene(string scene_name)
    {
        Scene scene = SceneManager.GetSceneByName(scene_name);

        if (!scene.IsValid() || !scene.isLoaded)
        {
            return;
        }

        StartCoroutine(UnloadAdditiveSceneProcess(scene));
    }

    private void LoadScene(string scene_name, LoadSceneMode load_scene_mode)
    {
        if (load_scene_mode == LoadSceneMode.Single)
        {
            DIContainer.Clear();
            m_previous_active_scene_name = null;
        }
        else
        {
            m_previous_active_scene_name = SceneManager.GetActiveScene().name;
        }

        gameObject.SetActive(true);

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;

        m_target_scene_name = scene_name;
        m_load_scene_mode = load_scene_mode;

        StartCoroutine(LoadSceneProcess());
    }

    private IEnumerator LoadSceneProcess()
    {
        m_canvas_group.interactable = true;
        m_canvas_group.blocksRaycasts = true;

        m_loading_label.text = "0%";

        yield return StartCoroutine(Fade(true));

        var op = SceneManager.LoadSceneAsync(m_target_scene_name, m_load_scene_mode);
        op.allowSceneActivation = false;

        float elapsed_time = 0f;

        while (!op.isDone)
        {
            yield return null;

            if (op.progress < 0.9f)
            {
                m_loading_label.text = (op.progress * 100).ToString("F0") + "%";
            }
            else
            {
                elapsed_time += Time.unscaledDeltaTime;

                m_loading_label.text = (Mathf.Lerp(0.9f, 1f, elapsed_time) * 100).ToString("F0") + "%";

                if (m_loading_label.text == "100%")
                {
                    op.allowSceneActivation = true;
                    yield break;
                }
            }
        }
    }

    private IEnumerator Fade(bool is_fade_in)
    {
        float elapsed_time = 0f;
        float target_time = 1f;


        while (elapsed_time <= target_time)
        {
            elapsed_time += Time.deltaTime;
            yield return null;

            m_canvas_group.alpha = is_fade_in ? Mathf.Lerp(0f, 1f, elapsed_time) : Mathf.Lerp(1f, 0f, elapsed_time);
        }

        if (!is_fade_in)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if (arg0.name == m_target_scene_name)
        {
            if (arg1 == LoadSceneMode.Additive)
            {
                SceneManager.SetActiveScene(arg0);
            }

            m_canvas_group.interactable = false;
            m_canvas_group.blocksRaycasts = false;

            StartCoroutine(Fade(false));
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private IEnumerator UnloadAdditiveSceneProcess(Scene scene)
    {
        Scene previous_scene = SceneManager.GetSceneByName(m_previous_active_scene_name);

        if (previous_scene.IsValid() && previous_scene.isLoaded)
        {
            SceneManager.SetActiveScene(previous_scene);
        }

        yield return SceneManager.UnloadSceneAsync(scene);
        m_previous_active_scene_name = null;
    }
}
