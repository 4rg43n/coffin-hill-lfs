using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public static class OverworldSceneSetup
{
    [MenuItem("CoffinHill/Wire Overworld Scene")]
    public static void WireOverworldSceneST()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name != "Overworld")
        {
            Debug.LogWarning("Please open the Overworld scene first."); return;
        }

        // Add PlayerController to Player
        GameObject player = GameObject.Find("Player");
        if (player != null && player.GetComponent<PlayerController>() == null)
        {
            player.AddComponent<PlayerController>();
            // Set Rigidbody2D to kinematic
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0f;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
            // Remove CameraFollow if mistakenly added to Player
            CameraFollow cf = player.GetComponent<CameraFollow>();
            if (cf != null) Object.DestroyImmediate(cf);
        }

        // Create Main Camera with CameraFollow if not already present
        GameObject cam = GameObject.Find("Main Camera");
        if (cam == null)
        {
            cam = new GameObject("Main Camera");
            cam.tag = "MainCamera";
            cam.AddComponent<Camera>();
            cam.AddComponent<AudioListener>();
        }
        if (cam.GetComponent<CameraFollow>() == null)
        {
            CameraFollow cf = cam.AddComponent<CameraFollow>();
            SerializedObject soCF = new SerializedObject(cf);
            soCF.FindProperty("target").objectReferenceValue = player?.transform;
            soCF.ApplyModifiedProperties();
        }

        // Wire OverworldManager's player reference
        OverworldManager om = Object.FindAnyObjectByType<OverworldManager>();
        if (om != null && player != null)
        {
            SerializedObject soOM = new SerializedObject(om);
            soOM.FindProperty("player").objectReferenceValue = player.GetComponent<PlayerController>();
            soOM.ApplyModifiedProperties();
        }

        // Set Tilemap renderer sort orders
        SetTilemapOrder("Ground", 0);
        SetTilemapOrder("Details", 1);
        SetTilemapOrder("Collision", 2);
        SetTilemapOrder("EncounterZones", 3);
        SetTilemapOrder("AbovePlayer", 10);

        // Make Collision tilemap invisible (it's logic only)
        GameObject colGO = GameObject.Find("Collision");
        if (colGO != null)
        {
            TilemapRenderer tr = colGO.GetComponent<TilemapRenderer>();
            if (tr != null) tr.enabled = false;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("CoffinHill: Overworld scene wired and saved.");
    }

    private static void SetTilemapOrder(string goName, int order)
    {
        GameObject go = GameObject.Find(goName);
        if (go == null) return;
        TilemapRenderer tr = go.GetComponent<TilemapRenderer>();
        if (tr != null)
        {
            SerializedObject so = new SerializedObject(tr);
            so.FindProperty("m_SortingOrder").intValue = order;
            so.ApplyModifiedProperties();
        }
    }

    [MenuItem("CoffinHill/Setup Battle Scene")]
    public static void SetupBattleSceneST()
    {
        // Load Battle scene
        Scene battleScene = EditorSceneManager.OpenScene("Assets/GameContent/Scenes/Battle.unity",
            OpenSceneMode.Single);

        // BattleManager
        GameObject bmGO = new GameObject("BattleManager");
        bmGO.AddComponent<BattleManager>();

        // Battle Canvas
        GameObject canvasGO = new GameObject("BattleCanvas");
        Canvas c = canvasGO.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        var cs = canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        cs.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1080, 1920);
        cs.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        BattleUI bui = canvasGO.AddComponent<BattleUI>();

        // EventSystem
        GameObject esGO = new GameObject("EventSystem");
        esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        EditorSceneManager.MarkSceneDirty(battleScene);
        EditorSceneManager.SaveScene(battleScene);
        Debug.Log("CoffinHill: Battle scene set up and saved.");
    }
}
