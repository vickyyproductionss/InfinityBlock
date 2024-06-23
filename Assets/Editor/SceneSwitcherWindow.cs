using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;

public class SceneSwitcherWindow : EditorWindow
{
    private Vector2 scrollPos;

    [MenuItem("Window/Scene Switcher")]
    public static void ShowWindow()
    {
        GetWindow<SceneSwitcherWindow>("Scene Switcher");
    }

    private void OnGUI()
    {
        GUILayout.Label("Available Scenes", EditorStyles.boldLabel);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        foreach (string scenePath in scenes)
        {
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(sceneName);

            if (GUILayout.Button("Open"))
            {
                OpenScene(scenePath);
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    private void OpenScene(string scenePath)
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene(scenePath);
        }
    }
}
