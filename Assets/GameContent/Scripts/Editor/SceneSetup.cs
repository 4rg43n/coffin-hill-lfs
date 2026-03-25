using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneSetup
{
    [MenuItem("CoffinHill/Wire Boot Scene")]
    public static void WireBootSceneST()
    {
        // Make sure Boot scene is loaded
        Scene bootScene = SceneManager.GetActiveScene();
        if (bootScene.name != "Boot")
        {
            Debug.LogWarning("Please open the Boot scene first.");
            return;
        }

        GameObject gmGO = GameObject.Find("GameManager");
        if (gmGO == null) { Debug.LogError("GameManager GameObject not found in Boot scene."); return; }

        GameManager gm   = gmGO.GetComponent<GameManager>();
        InputManager im  = gmGO.GetComponent<InputManager>();
        SceneTransitionManager stm = gmGO.GetComponent<SceneTransitionManager>();
        AudioManager am  = gmGO.GetComponent<AudioManager>();

        // Wire GameManager SerializeFields via SerializedObject
        SerializedObject soGM = new SerializedObject(gm);
        soGM.FindProperty("inputManager").objectReferenceValue          = im;
        soGM.FindProperty("sceneTransitionManager").objectReferenceValue = stm;
        soGM.ApplyModifiedProperties();

        // Wire AudioManager's two AudioSources
        AudioSource[] sources = gmGO.GetComponents<AudioSource>();
        if (sources.Length >= 2)
        {
            SerializedObject soAM = new SerializedObject(am);
            soAM.FindProperty("musicSource").objectReferenceValue = sources[0];
            soAM.FindProperty("sfxSource").objectReferenceValue   = sources[1];

            // Configure music source for looping
            SerializedObject soSrc0 = new SerializedObject(sources[0]);
            soSrc0.FindProperty("Loop").boolValue = true;
            soSrc0.ApplyModifiedProperties();

            soAM.ApplyModifiedProperties();
        }

        // Wire AudioClipReferences if it exists
        AudioClipReferences acr = AssetDatabase.LoadAssetAtPath<AudioClipReferences>(
            "Assets/GameContent/Data/Database/AudioClipReferences.asset");
        if (acr != null)
        {
            SerializedObject soAM = new SerializedObject(am);
            soAM.FindProperty("clips").objectReferenceValue = acr;
            soAM.ApplyModifiedProperties();
        }

        EditorSceneManager.MarkSceneDirty(bootScene);
        EditorSceneManager.SaveScene(bootScene);
        Debug.Log("CoffinHill: Boot scene wired and saved.");
    }

    [MenuItem("CoffinHill/Save All GameContent Scenes")]
    public static void SaveAllScenesST()
    {
        string[] scenePaths = new[]
        {
            "Assets/GameContent/Scenes/Boot.unity",
            "Assets/GameContent/Scenes/MainMenu.unity",
            "Assets/GameContent/Scenes/Overworld.unity",
            "Assets/GameContent/Scenes/Battle.unity",
        };
        foreach (string path in scenePaths)
        {
            Scene s = SceneManager.GetSceneByPath(path);
            if (s.isLoaded)
                EditorSceneManager.SaveScene(s);
        }
        Debug.Log("CoffinHill: All loaded scenes saved.");
    }
}
