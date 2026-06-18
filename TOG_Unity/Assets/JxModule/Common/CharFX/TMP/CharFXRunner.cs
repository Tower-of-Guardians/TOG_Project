using System.Collections.Generic;
using TMPro;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JxModule.CharFX
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TMP_Text))]
    public sealed class CharFxRunner : MonoBehaviour
    {
        [SerializeField, TextArea(2, 10)] private string sourceText; 
        [SerializeField] private bool cfxEnable = true;
        [SerializeField] private CharFXPlayMode playMode = CharFXPlayMode.Loop;
        [SerializeField] private float onceDuration = 1f;
        [SerializeField] private bool restoreWhenFinished = true;
        
        private readonly List<CharFXEffectRange> _effects = new();

        private TMP_Text _text;
        private CharFXEngine _engine;
        private TMPTextLayoutAdapter _layout;

        private bool _isPlaying;
        private bool _dirty;
        private float _startTime;
        
        private void Awake()
        {
            _text = GetComponent<TMP_Text>();

            _engine = new CharFXEngine();
            _layout = new TMPTextLayoutAdapter(_text);

            if (string.IsNullOrEmpty(sourceText) && _text != null)
            {
                sourceText = _text.text;
            }
            
            ParseRichTagsAndApplyText();
            MarkDirtyAndRestart();
        }

        private void OnEnable()
        {
            MarkDirtyAndRestart();
        }
        
        private void OnValidate()
        {
            _text ??= GetComponent<TMP_Text>();

            if (!Application.isPlaying)
            {
                return;
            }

            ParseRichTagsAndApplyText();
            MarkDirtyAndRestart();
        }
        
        private void LateUpdate()
        {
            if (!cfxEnable)
            {
                return;
            }

            if (!_isPlaying)
            {
                return;
            }

            if (_effects.Count == 0)
            {
                return;
            }

            if(_dirty)
            {
                _layout.Rebuild();
                _dirty = false;
            }
            
            var elapsedTime = Time.unscaledTime - _startTime;
            if (playMode == CharFXPlayMode.Once && elapsedTime >= onceDuration)
            {
                _isPlaying = false;

                if (restoreWhenFinished)
                {
                    RestoreBaseMesh();
                }

                return;
            }

            _layout.ResetWorkToBase();

            var context = new CharFXContext(time: Time.unscaledTime,
                                            deltaTime: Time.unscaledDeltaTime,
                                            startTime: _startTime);

            _engine.Apply(_layout, _effects, context);
            _layout.ApplyToText();
        }

        public void Play()
        {
            MarkDirtyAndRestart();
        }

        public void Stop(bool restore = true)
        {
            _isPlaying = false;

            if (restore)
            {
                RestoreBaseMesh();
            }
        }

        public void PlayOnce(float duration)
        {
            playMode = CharFXPlayMode.Once;
            onceDuration = Mathf.Max(0f, duration);
            MarkDirtyAndRestart();
        }

        public void PlayLoop()
        {
            playMode = CharFXPlayMode.Loop;
            MarkDirtyAndRestart();
        }
        
        private void ParseRichTagsAndApplyText()
        {
            _effects.Clear();
            
            var parsed = CharFXTextParser.Parse(sourceText);

            BuildEffects(parsed);
            
            _text.text = parsed.Text;
            _dirty = true;
        }
        
        private void BuildEffects(CharFXParseResult parsed)
        {
            _effects.Clear();

            if (parsed == null)
            {
                return;
            }

            foreach (var range in parsed.Ranges)
            {
                if (!CharFXEffectFactory.TryCreateEffect(range.Tag, out var effect))
                {
                    continue;
                }
                
                _effects.Add(new CharFXEffectRange(effect, range.StartIndex, range.EndIndex));
            }
        }
        
        private void RestoreBaseMesh()
        {
            if (_layout == null)
            {
                return;
            }

            _layout.Rebuild();
            _layout.ResetWorkToBase();
            _layout.ApplyToText();
        }

        public void SetEnabled(bool value)
        {
            cfxEnable = value;

            if(!cfxEnable)
            {
                RestoreBaseMesh();
                return;
            }

            MarkDirtyAndRestart();
        }
        
        private void MarkDirtyAndRestart()
        {
            _dirty = true;
            _startTime = Time.unscaledTime;
            _isPlaying = true;
        }
        
        public void SetText(string text)
        {
            sourceText = text ?? string.Empty;

            if (_text == null)
            {
                return;
            }

            ParseRichTagsAndApplyText();
            MarkDirtyAndRestart();
        }

        public string GetSourceText()
        {
            return sourceText;
        }
    }
    
#if UNITY_EDITOR
    // [CanEditMultipleObjects]
    // [CustomEditor(typeof(CharFxRunner))]
    // public class CharFxRunnerEditor : Editor
    // {
    //     private SerializedProperty _sourceText;
    //
    //     private void OnEnable()
    //     {
    //         _sourceText = serializedObject.FindProperty("sourceText");
    //     }
    //
    //     public override void OnInspectorGUI()
    //     {
    //         serializedObject.Update();
    //
    //         DrawTitle();
    //         DrawSourceText();
    //
    //         serializedObject.ApplyModifiedProperties();
    //     }
    //
    //     private void DrawTitle()
    //     {
    //         var headerStyle = new GUIStyle(EditorStyles.boldLabel)
    //         {
    //             fontSize = 20,
    //             fontStyle = FontStyle.Bold,
    //             alignment = TextAnchor.MiddleCenter
    //         };
    //
    //         ColorUtility.TryParseHtmlString("#36FD96", out var headerColor);
    //         headerStyle.normal.textColor = headerColor;
    //
    //         EditorGUILayout.Space(10);
    //         EditorGUILayout.LabelField("CharFX Runner", headerStyle, GUILayout.Height(28));
    //         EditorGUILayout.Space(12);
    //     }
    //
    //     private void DrawSourceText()
    //     {
    //         EditorGUILayout.LabelField("Source Text", EditorStyles.boldLabel);
    //         
    //         _sourceText.stringValue = EditorGUILayout.TextArea(_sourceText.stringValue,
    //                                                            EditorStyles.textArea,
    //                                                            GUILayout.MinHeight(60));
    //         
    //         EditorGUILayout.Space(12);
    //         DrawGuideBox();
    //     }
    //     
    //     private void DrawGuideBox()
    //     {
    //         var boxStyle = new GUIStyle(EditorStyles.helpBox)
    //         {
    //             padding = new RectOffset(12, 12, 10, 10)
    //         };
    //
    //         var titleStyle = new GUIStyle(EditorStyles.boldLabel)
    //         {
    //             fontSize = 13
    //         };
    //
    //         var bodyStyle = new GUIStyle(EditorStyles.label)
    //         {
    //             wordWrap = true,
    //             richText = true
    //         };
    //
    //         var codeStyle = new GUIStyle(EditorStyles.miniLabel)
    //         {
    //             richText = true,
    //             wordWrap = true,
    //             padding = new RectOffset(8, 8, 4, 4)
    //         };
    //
    //         EditorGUILayout.BeginVertical(boxStyle);
    //         EditorGUILayout.LabelField(
    //             "<b>TextMeshPro</b> 대신 <b>CharFXRunner</b>의 <b>TextArea</b>를 사용해야 합니다.\n" +
    //             "태그 매개변수는 <b>JxModule</b>의 <b>[CharFX] - [Effects]</b>를 참고해주세요.",
    //             bodyStyle
    //         );
    //
    //         EditorGUILayout.Space(8);
    //
    //         EditorGUILayout.LabelField("태그 목록", titleStyle);
    //         EditorGUILayout.LabelField("<b>- wave</b>\n" + "<b>- shake</b>\n" + "<b>- pop</b>\n", bodyStyle);
    //         
    //         EditorGUILayout.EndVertical();
    //     }
    // }
#endif
}