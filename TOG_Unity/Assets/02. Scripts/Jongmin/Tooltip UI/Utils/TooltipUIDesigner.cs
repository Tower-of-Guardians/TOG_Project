using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "Tooltip UI Designer", menuName = "SO/Design/Tooltip UI Designer")]
public class TooltipUIDesigner : ScriptableObject
{
    [Header("기획 옵션")]
    [Header("툴팁의 페이드 시간")]
    [SerializeField] private float m_fade_duration;
    public float FadeDuration => m_fade_duration;

    [Header("툴팁 이동 여부")]
    [SerializeField] private bool m_can_moveable;
    public bool Moveable => m_can_moveable;

    [Header("X축 피벗 변경 퍼센트")]
    [SerializeField, Range(0f, 1f)] private float m_pivot_x;
    public float PivotX => m_pivot_x;

    [Header("Y축 피벗 변경 퍼센트")]
    [SerializeField, Range(0f, 1f)] private float m_pivot_y;
    public float PivotY => m_pivot_y;
}

#if UNITY_EDITOR
[CustomEditor(typeof(TooltipUIDesigner))]
public class TooltipUIDesignerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty fadeDuration = serializedObject.FindProperty("m_fade_duration");
        SerializedProperty canMoveable  = serializedObject.FindProperty("m_can_moveable");
        SerializedProperty pivotX       = serializedObject.FindProperty("m_pivot_x");
        SerializedProperty pivotY       = serializedObject.FindProperty("m_pivot_y");

        EditorGUILayout.PropertyField(fadeDuration);
        EditorGUILayout.PropertyField(canMoveable);

        EditorGUI.BeginDisabledGroup(!canMoveable.boolValue);
        {
            EditorGUILayout.PropertyField(pivotX);
            EditorGUILayout.PropertyField(pivotY);
        }
        EditorGUI.EndDisabledGroup();

        serializedObject.ApplyModifiedProperties();
    }
}
#endif