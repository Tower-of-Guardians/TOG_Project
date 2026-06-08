using System;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JxModule.DataTable
{
#if UNITY_EDITOR
    public class DataTableRowWindow : EditorWindow
    {
        private Action<MonoScript> _onSelected;
        private Vector2 _scrollPosition;
        private string _searchText = "";

        public static void Open(Action<MonoScript> onSelected)
        {
            var window = CreateInstance<DataTableRowWindow>();

            window.titleContent = new GUIContent("Select DataTable Row");
            window._onSelected = onSelected;
            window.minSize = new Vector2(300, 300);

            window.ShowUtility();
        }

        private void OnGUI()
        {
            DrawSearchField();
            DrawRowScriptList();
        }

        private void DrawSearchField()
        {
            EditorGUILayout.Space(8);

            _searchText = EditorGUILayout.TextField("Search", _searchText);

            EditorGUILayout.Space(4);
        }

        private void DrawRowScriptList()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            var types = TypeCache.GetTypesDerivedFrom<DataTableRowBase>()
                .Where(t => !t.IsAbstract && !t.IsInterface);

            if (!string.IsNullOrWhiteSpace(_searchText))
            {
                types = types.Where(t =>
                    t.Name.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            var hasAny = false;

            foreach (var type in types)
            {
                var script = FindMonoScript(type);
                if (script == null)
                {
                    continue;
                }

                hasAny = true;

                if (GUILayout.Button(type.Name, GUILayout.Height(24)))
                {
                    _onSelected?.Invoke(script);
                    Close();
                }
            }

            if (!hasAny)
            {
                EditorGUILayout.LabelField("Nothing");
            }

            EditorGUILayout.EndScrollView();
        }

        private static MonoScript FindMonoScript(Type type)
        {
            var assetGuids = AssetDatabase.FindAssets($"{type.Name} t:MonoScript");

            foreach (var guid in assetGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

                if (script != null && script.GetClass() == type)
                {
                    return script;
                }
            }

            return null;
        }
    }
#endif
}