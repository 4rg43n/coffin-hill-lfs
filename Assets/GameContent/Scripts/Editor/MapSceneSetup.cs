#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class MapSceneSetup
{
    [MenuItem("CoffinHill/Setup Map Scene")]
    public static void SetupMapScene()
    {
        // Remove old Canvas-based map (if re-running)
        GameObject oldCanvas = GameObject.Find("MapCanvas");
        if (oldCanvas != null) Object.DestroyImmediate(oldCanvas);

        // Remove old MapView GO (if re-running)
        GameObject oldMV = GameObject.Find("MapView");
        if (oldMV != null) Object.DestroyImmediate(oldMV);

        // ---- MapView world-space GO ----
        GameObject mapViewGo = new GameObject("MapView");
        MapView mapView = mapViewGo.AddComponent<MapView>();

        // ---- Wire MapManager ↔ MapView ----
        MapManager mm = Object.FindAnyObjectByType<MapManager>();
        if (mm != null)
        {
            SerializedObject soMM = new SerializedObject(mm);
            soMM.FindProperty("mapView").objectReferenceValue = mapView;
            soMM.ApplyModifiedProperties();
        }
        else
        {
            Debug.LogWarning("[MapSceneSetup] MapManager not found — wire 'mapView' reference manually.");
        }

        // ---- Add MapCameraController to Main Camera ----
        GameObject camGo = GameObject.FindWithTag("MainCamera");
        if (camGo != null)
        {
            if (camGo.GetComponent<MapCameraController>() == null)
                camGo.AddComponent<MapCameraController>();
        }
        else
        {
            Debug.LogWarning("[MapSceneSetup] Main Camera not found — add MapCameraController manually.");
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[MapSceneSetup] World-space map scene configured successfully.");
    }
}
#endif
