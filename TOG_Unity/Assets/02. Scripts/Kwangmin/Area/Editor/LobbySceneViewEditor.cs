using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(LobbySceneView))]
public sealed class LobbySceneViewEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        var toggle = new Toggle("로비씬 보기") { name = "showLobby" };
        toggle.SetValueWithoutNotify(serializedObject.FindProperty("showLobby").boolValue);
        toggle.RegisterValueChangedCallback(evt =>
        {
            serializedObject.Update();
            var objects = new List<Object> { target };
            var lobby = serializedObject.FindProperty("lobbyRoot").objectReferenceValue;
            if (lobby != null) objects.Add(lobby);
            var gameRoots = serializedObject.FindProperty("gameRoots");
            for (int i = 0; i < gameRoots.arraySize; i++)
            {
                var item = gameRoots.GetArrayElementAtIndex(i).objectReferenceValue;
                if (item != null) objects.Add(item);
            }
            Undo.RecordObjects(objects.ToArray(), "Toggle Lobby View");
            var view = (LobbySceneView)target;
            view.SetLobbyVisible(evt.newValue);
            foreach (var item in objects) EditorUtility.SetDirty(item);
            if (!Application.isPlaying) EditorSceneManager.MarkSceneDirty(view.gameObject.scene);
            serializedObject.Update();
            SceneView.RepaintAll();
        });
        root.Add(toggle);
        root.TrackPropertyValue(serializedObject.FindProperty("showLobby"),
            property => toggle.SetValueWithoutNotify(property.boolValue));
        root.Add(new PropertyField(serializedObject.FindProperty("lobbyRoot")));
        root.Add(new PropertyField(serializedObject.FindProperty("gameRoots")));
        return root;
    }
}
