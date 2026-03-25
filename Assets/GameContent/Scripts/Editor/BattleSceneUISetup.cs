using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Menu item: CoffinHill/Wire Battle Scene UI
/// Builds the full uGUI hierarchy under BattleCanvas and wires all
/// SerializeField references on BattleUI. Run while the Battle scene is open.
/// </summary>
public static class BattleSceneUISetup
{
    [MenuItem("CoffinHill/Wire Battle Scene UI")]
    public static void WireBattleSceneUIST()
    {
        GameObject canvasGO = GameObject.Find("BattleCanvas");
        if (canvasGO == null) { Debug.LogError("BattleCanvas not found. Open the Battle scene first."); return; }

        BattleUI battleUI = canvasGO.GetComponent<BattleUI>();
        if (battleUI == null) { Debug.LogError("BattleUI component missing on BattleCanvas."); return; }

        // Configure CanvasScaler for portrait 1080×1920
        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode    = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight  = 0f;
        }

        // Clear existing children
        for (int i = canvasGO.transform.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(canvasGO.transform.GetChild(i).gameObject);

        // ── Enemy Status Box ────────────────────────────────────────────
        // Top-left area: x=0..600, y=80..260 from top
        GameObject enemyBox = MakeImage(canvasGO.transform, "EnemyStatusBox", Hex("1E1E2ECC"));
        AnchorTopLeft(enemyBox, 0, 80, 600, 180);

        var enemyName  = MakeTMP(enemyBox.transform,  "EnemyName",  "ENEMY", 48, TextAlignmentOptions.Left);
        var enemyLevel = MakeTMP(enemyBox.transform,  "EnemyLevel", "Lv.--", 42, TextAlignmentOptions.Right);
        AnchorInsideH(enemyName,  enemyBox, 0f,    0.68f, 14, 10, 14, 60);
        AnchorInsideH(enemyLevel, enemyBox, 0.68f, 1f,    4,  10, 14, 60);

        GameObject enemyHPBG   = MakeImage(enemyBox.transform,   "EnemyHPBarBG",   Hex("333333FF"));
        GameObject enemyHPFill = MakeImage(enemyHPBG.transform,  "EnemyHPBarFill", Hex("28D428FF"));
        AnchorBottom(enemyHPBG, enemyBox, 14, 36, 14, 26);
        SetStretch(enemyHPFill, 3, 3, 3, 3);
        MakeFillBar(enemyHPFill);

        // ── Player Status Box ───────────────────────────────────────────
        // Right side: x=480..1080, y=470..710 from top
        GameObject playerBox = MakeImage(canvasGO.transform, "PlayerStatusBox", Hex("1E1E2ECC"));
        AnchorTopLeft(playerBox, 480, 470, 600, 240);

        var playerName  = MakeTMP(playerBox.transform, "PlayerName",  "PLAYER", 48, TextAlignmentOptions.Left);
        var playerLevel = MakeTMP(playerBox.transform, "PlayerLevel", "Lv.--",  42, TextAlignmentOptions.Right);
        AnchorInsideH(playerName,  playerBox, 0f,    0.65f, 14, 10, 14, 58);
        AnchorInsideH(playerLevel, playerBox, 0.65f, 1f,    4,  10, 14, 58);

        GameObject playerHPBG   = MakeImage(playerBox.transform,   "PlayerHPBarBG",   Hex("333333FF"));
        GameObject playerHPFill = MakeImage(playerHPBG.transform,  "PlayerHPBarFill", Hex("28D428FF"));
        AnchorMiddle(playerHPBG, playerBox, 14, 14, 32, 26);
        SetStretch(playerHPFill, 3, 3, 3, 3);
        MakeFillBar(playerHPFill);

        var playerHPText = MakeTMP(playerBox.transform, "PlayerHPText", "25/25", 38, TextAlignmentOptions.Center);
        AnchorBottom(playerHPText, playerBox, 14, 14, 8, 46);

        GameObject playerEXPBG   = MakeImage(playerBox.transform,   "PlayerEXPBarBG",   Hex("111111FF"));
        GameObject playerEXPFill = MakeImage(playerEXPBG.transform, "PlayerEXPBarFill", Hex("3366DDFF"));
        AnchorBottom(playerEXPBG, playerBox, 14, 14, 4, 14);
        SetStretch(playerEXPFill, 2, 2, 2, 2);
        MakeFillBar(playerEXPFill);

        // ── Bottom Panel (log + menus share this area) ───────────────────
        // Full width, 520px tall, pinned to screen bottom
        GameObject bottomPanel = MakeImage(canvasGO.transform, "BottomPanel", Hex("0D0D0DEF"));
        StretchBottom(bottomPanel, 520);

        // Divider line between log and action menu
        GameObject divider = MakeImage(bottomPanel.transform, "Divider", Hex("555555FF"));
        {
            RectTransform rt = divider.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.6f, 0f);
            rt.anchorMax = new Vector2(0.6f, 1f);
            rt.offsetMin = new Vector2(-1, 20);
            rt.offsetMax = new Vector2(1, -20);
        }

        // Battle Log (left 60% of bottom panel)
        GameObject logArea = new GameObject("BattleLogArea");
        logArea.transform.SetParent(bottomPanel.transform, false);
        {
            RectTransform rt = logArea.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0.6f, 1f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        // ScrollRect inside log area
        GameObject scrollGO = new GameObject("BattleLogScroll");
        scrollGO.transform.SetParent(logArea.transform, false);
        ScrollRect scrollRect = scrollGO.AddComponent<ScrollRect>();
        SetStretch(scrollGO, 12, 12, 12, 12);
        scrollRect.horizontal    = false;
        scrollRect.vertical      = true;
        scrollRect.movementType  = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 30f;

        // Viewport
        GameObject viewport = MakeImage(scrollGO.transform, "Viewport", Color.clear);
        SetStretch(viewport, 0, 0, 0, 0);
        viewport.AddComponent<RectMask2D>();
        scrollRect.viewport = viewport.GetComponent<RectTransform>();

        // Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRT = content.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot     = new Vector2(0.5f, 1f);
        contentRT.offsetMin = Vector2.zero;
        contentRT.offsetMax = Vector2.zero;
        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scrollRect.content = contentRT;

        // Log text
        GameObject logTextGO = new GameObject("BattleLogText");
        logTextGO.transform.SetParent(content.transform, false);
        TextMeshProUGUI logTMP = logTextGO.AddComponent<TextMeshProUGUI>();
        logTMP.fontSize  = 44;
        logTMP.color     = Color.white;
        logTMP.alignment = TextAlignmentOptions.TopLeft;
        logTMP.text      = "";
        RectTransform logRT = logTextGO.GetComponent<RectTransform>();
        logRT.anchorMin = new Vector2(0, 0);
        logRT.anchorMax = new Vector2(1, 1);
        logRT.offsetMin = new Vector2(16, 12);
        logRT.offsetMax = new Vector2(-12, -12);

        // ── Action Menu (right 40% of bottom panel) ──────────────────────
        // Shows when player is choosing an action
        GameObject actionMenu = new GameObject("ActionMenu");
        actionMenu.transform.SetParent(bottomPanel.transform, false);
        {
            RectTransform rt = actionMenu.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.6f, 0f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
        actionMenu.SetActive(false);

        // 2×2 grid: FIGHT (top-left), BAG (top-right), PARTY (bot-left), RUN (bot-right)
        string[] actionLabels = { "FIGHT", "BAG", "PARTY", "RUN" };
        Button[] actionButtons = new Button[4];
        for (int i = 0; i < 4; i++)
        {
            int col = i % 2;
            int row = 1 - i / 2;   // row 1 = top, row 0 = bottom
            GameObject btn = MakeButton(actionMenu.transform, actionLabels[i] + "Button", actionLabels[i], 44);
            RectTransform rt = btn.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(col * 0.5f,        row * 0.5f);
            rt.anchorMax = new Vector2(col * 0.5f + 0.5f, row * 0.5f + 0.5f);
            rt.offsetMin = new Vector2(8, 8);
            rt.offsetMax = new Vector2(-8, -8);
            actionButtons[i] = btn.GetComponent<Button>();
        }

        // ── Move Menu (full bottom panel) ────────────────────────────────
        // Overlays the bottom panel when player selects FIGHT
        GameObject moveMenu = new GameObject("MoveMenu");
        moveMenu.transform.SetParent(canvasGO.transform, false);
        {
            RectTransform rt = moveMenu.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = new Vector2(0, 520);
        }
        MakeImage(moveMenu.transform, "MoveMenuBG", Hex("0D0D0DEF")).GetComponent<RectTransform>().Let(SetStretchFull);
        moveMenu.SetActive(false);

        Button[]           moveButtons   = new Button[4];
        TextMeshProUGUI[]  moveNameTexts = new TextMeshProUGUI[4];
        TextMeshProUGUI[]  movePPTexts   = new TextMeshProUGUI[4];

        for (int i = 0; i < 4; i++)
        {
            int col = i % 2;
            int row = 1 - i / 2;

            GameObject moveBtnGO = new GameObject($"MoveButton{i + 1}");
            moveBtnGO.transform.SetParent(moveMenu.transform, false);
            Image img = moveBtnGO.AddComponent<Image>();
            img.color = Hex("2A2A3AFF");
            Button btn = moveBtnGO.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = Hex("3A3A5AFF");
            colors.pressedColor     = Hex("1A1A2AFF");
            btn.colors = colors;
            btn.targetGraphic = img;
            moveButtons[i] = btn;

            RectTransform rt = moveBtnGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(col * 0.5f,        row * 0.5f);
            rt.anchorMax = new Vector2(col * 0.5f + 0.5f, row * 0.5f + 0.5f);
            rt.offsetMin = new Vector2(8, 8);
            rt.offsetMax = new Vector2(-8, -8);

            // Move name (upper portion of button)
            GameObject nameGO = new GameObject("MoveName");
            nameGO.transform.SetParent(moveBtnGO.transform, false);
            TextMeshProUGUI nameTMP = nameGO.AddComponent<TextMeshProUGUI>();
            nameTMP.fontSize  = 44;
            nameTMP.color     = Color.white;
            nameTMP.alignment = TextAlignmentOptions.Left;
            nameTMP.text      = "---";
            moveNameTexts[i]  = nameTMP;
            RectTransform nameRT = nameGO.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0f,   0.45f);
            nameRT.anchorMax = new Vector2(0.72f, 1f);
            nameRT.offsetMin = new Vector2(18, 0);
            nameRT.offsetMax = new Vector2(-4, -8);

            // PP text (lower-right of button)
            GameObject ppGO = new GameObject("MovePP");
            ppGO.transform.SetParent(moveBtnGO.transform, false);
            TextMeshProUGUI ppTMP = ppGO.AddComponent<TextMeshProUGUI>();
            ppTMP.fontSize  = 34;
            ppTMP.color     = new Color(0.75f, 0.75f, 0.75f);
            ppTMP.alignment = TextAlignmentOptions.Right;
            ppTMP.text      = "--/--";
            movePPTexts[i]  = ppTMP;
            RectTransform ppRT = ppGO.GetComponent<RectTransform>();
            ppRT.anchorMin = new Vector2(0.72f, 0f);
            ppRT.anchorMax = new Vector2(1f,    0.55f);
            ppRT.offsetMin = new Vector2(4,  8);
            ppRT.offsetMax = new Vector2(-16, 0);
        }

        // ── Debug Panel (W / L buttons — shown only when debugMode=true) ─
        // Two small buttons pinned to bottom-right above the bottom panel
        GameObject debugPanel = new GameObject("DebugPanel");
        debugPanel.transform.SetParent(canvasGO.transform, false);
        {
            RectTransform rt = debugPanel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot     = new Vector2(1f, 0f);
            rt.anchoredPosition = new Vector2(-10f, 530f);   // just above bottom panel
            rt.sizeDelta        = new Vector2(200f, 80f);
        }
        debugPanel.SetActive(false);   // hidden by default; BattleUI.Start() shows if debugMode

        Button debugWinBtn  = MakeButton(debugPanel.transform, "DebugWinButton",  "W", 52).GetComponent<Button>();
        Button debugLoseBtn = MakeButton(debugPanel.transform, "DebugLoseButton", "L", 52).GetComponent<Button>();

        // Win button — left half
        {
            var rt = debugWinBtn.gameObject.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = new Vector2(-4, 0);
            debugWinBtn.gameObject.GetComponent<Image>().color = Hex("1A6B1AFF");
        }
        // Lose button — right half
        {
            var rt = debugLoseBtn.gameObject.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.offsetMin = new Vector2(4, 0);
            rt.offsetMax = new Vector2(0, 0);
            debugLoseBtn.gameObject.GetComponent<Image>().color = Hex("6B1A1AFF");
        }

        // ── Wire all SerializeField references on BattleUI ──────────────
        SerializedObject so = new SerializedObject(battleUI);

        so.FindProperty("enemyNameText").objectReferenceValue  = enemyName.GetComponent<TextMeshProUGUI>();
        so.FindProperty("enemyLevelText").objectReferenceValue = enemyLevel.GetComponent<TextMeshProUGUI>();
        so.FindProperty("enemyHPBar").objectReferenceValue     = enemyHPFill.GetComponent<Image>();

        so.FindProperty("playerNameText").objectReferenceValue  = playerName.GetComponent<TextMeshProUGUI>();
        so.FindProperty("playerLevelText").objectReferenceValue = playerLevel.GetComponent<TextMeshProUGUI>();
        so.FindProperty("playerHPBar").objectReferenceValue     = playerHPFill.GetComponent<Image>();
        so.FindProperty("playerHPText").objectReferenceValue    = playerHPText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("playerEXPBar").objectReferenceValue    = playerEXPFill.GetComponent<Image>();

        so.FindProperty("actionMenu").objectReferenceValue = actionMenu;
        so.FindProperty("fightButton").objectReferenceValue = actionButtons[0];
        so.FindProperty("bagButton").objectReferenceValue   = actionButtons[1];
        so.FindProperty("partyButton").objectReferenceValue = actionButtons[2];
        so.FindProperty("runButton").objectReferenceValue   = actionButtons[3];

        so.FindProperty("moveMenu").objectReferenceValue = moveMenu;

        var moveBtnsProp = so.FindProperty("moveButtons");
        moveBtnsProp.arraySize = 4;
        for (int i = 0; i < 4; i++) moveBtnsProp.GetArrayElementAtIndex(i).objectReferenceValue = moveButtons[i];

        var moveNamesProp = so.FindProperty("moveNameTexts");
        moveNamesProp.arraySize = 4;
        for (int i = 0; i < 4; i++) moveNamesProp.GetArrayElementAtIndex(i).objectReferenceValue = moveNameTexts[i];

        var movePPProp = so.FindProperty("movePPTexts");
        movePPProp.arraySize = 4;
        for (int i = 0; i < 4; i++) movePPProp.GetArrayElementAtIndex(i).objectReferenceValue = movePPTexts[i];

        so.FindProperty("battleLogText").objectReferenceValue   = logTMP;
        so.FindProperty("battleLogScroll").objectReferenceValue = scrollRect;

        so.FindProperty("debugPanel").objectReferenceValue     = debugPanel;
        so.FindProperty("debugWinButton").objectReferenceValue  = debugWinBtn;
        so.FindProperty("debugLoseButton").objectReferenceValue = debugLoseBtn;

        so.ApplyModifiedProperties();

        // ── Configure Camera ──────────────────────────────────────────────
        Camera cam = Object.FindAnyObjectByType<Camera>();
        if (cam != null)
        {
            cam.orthographic     = true;
            cam.orthographicSize = 4f;
            cam.backgroundColor  = new Color(0.08f, 0.07f, 0.12f);
            cam.clearFlags       = CameraClearFlags.SolidColor;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            EditorUtility.SetDirty(cam);
        }

        // ── BattleVisuals GameObject ──────────────────────────────────────
        // Remove stale instance if the script was re-run
        GameObject existingVis = GameObject.Find("BattleVisuals");
        if (existingVis != null) Object.DestroyImmediate(existingVis);

        GameObject visualsGO = new GameObject("BattleVisuals");
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(
            visualsGO, canvasGO.scene);
        visualsGO.AddComponent<BattleVisuals>();

        // ── Results Screen Panel ──────────────────────────────────────────
        // Full-screen overlay on top of everything; starts inactive.
        GameObject resultsPanel = new GameObject("ResultsPanel");
        resultsPanel.transform.SetParent(canvasGO.transform, false);
        Image resultsBG = resultsPanel.AddComponent<Image>();
        resultsBG.color = new Color(0f, 0f, 0f, 0.88f);
        SetStretch(resultsPanel, 0, 0, 0, 0);

        // Title
        var resultsTitle = MakeTMP(resultsPanel.transform, "ResultsTitle", "VICTORY!", 96,
            TextAlignmentOptions.Center);
        {
            var rt = resultsTitle.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0.6f); rt.anchorMax = new Vector2(1f, 0.8f);
            rt.offsetMin = new Vector2(20, 0); rt.offsetMax = new Vector2(-20, 0);
        }

        // Body
        var resultsBody = MakeTMP(resultsPanel.transform, "ResultsBody",
            "You won!\n\n(Rewards coming soon)", 48, TextAlignmentOptions.Center);
        {
            var rt = resultsBody.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0.35f); rt.anchorMax = new Vector2(1f, 0.6f);
            rt.offsetMin = new Vector2(40, 0); rt.offsetMax = new Vector2(-40, 0);
            resultsBody.GetComponent<TextMeshProUGUI>().color = new Color(0.85f, 0.85f, 0.85f);
        }

        // Continue button
        var continueBtn = MakeButton(resultsPanel.transform, "ContinueButton", "CONTINUE", 56);
        {
            var rt = continueBtn.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.2f, 0.12f); rt.anchorMax = new Vector2(0.8f, 0.28f);
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        resultsPanel.SetActive(false);

        // Add and wire BattleResultsScreen
        BattleResultsScreen rsc = resultsPanel.AddComponent<BattleResultsScreen>();
        SerializedObject rscSO = new SerializedObject(rsc);
        rscSO.FindProperty("panel").objectReferenceValue         = resultsPanel;
        rscSO.FindProperty("titleText").objectReferenceValue     = resultsTitle.GetComponent<TextMeshProUGUI>();
        rscSO.FindProperty("bodyText").objectReferenceValue      = resultsBody.GetComponent<TextMeshProUGUI>();
        rscSO.FindProperty("continueButton").objectReferenceValue= continueBtn.GetComponent<Button>();
        rscSO.ApplyModifiedProperties();

        var scene = canvasGO.scene;
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("CoffinHill: Battle scene UI built and saved.");
    }

    // ── Layout helpers ────────────────────────────────────────────────

    /// Pin to top-left corner, offset by (x, yFromTop), with given size.
    static void AnchorTopLeft(GameObject go, float x, float yFromTop, float w, float h)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0, 1);
        rt.anchorMax        = new Vector2(0, 1);
        rt.pivot            = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(x, -yFromTop);
        rt.sizeDelta        = new Vector2(w, h);
    }

    /// Horizontal strip inside a parent panel (uses normalised x bounds).
    static void AnchorInsideH(GameObject go, GameObject parent,
        float xMinN, float xMaxN, float lPad, float tPad, float rPad, float h)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(xMinN, 1);
        rt.anchorMax = new Vector2(xMaxN, 1);
        rt.pivot     = new Vector2(0, 1);
        rt.offsetMin = new Vector2(lPad,  -(tPad + h));
        rt.offsetMax = new Vector2(-rPad, -tPad);
    }

    /// HP bar-style strip pinned to bottom of parent.
    static void AnchorBottom(GameObject go, GameObject parent,
        float lPad, float rPad, float bPad, float h)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.offsetMin = new Vector2(lPad,         bPad);
        rt.offsetMax = new Vector2(-rPad,        bPad + h);
    }

    /// Strip anchored to vertical middle of parent.
    static void AnchorMiddle(GameObject go, GameObject parent,
        float lPad, float rPad, float yFromMid, float h)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0.5f);
        rt.anchorMax = new Vector2(1, 0.5f);
        rt.offsetMin = new Vector2(lPad,   yFromMid);
        rt.offsetMax = new Vector2(-rPad,  yFromMid + h);
    }

    /// Full-width panel pinned to screen bottom.
    static void StretchBottom(GameObject go, float height)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.offsetMin = new Vector2(0, 0);
        rt.offsetMax = new Vector2(0, height);
    }

    static void SetStretch(GameObject go, float l, float b, float r, float t)
    {
        var rt = go.GetComponent<RectTransform>();
        if (rt == null) rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(l, b);
        rt.offsetMax = new Vector2(-r, -t);
    }

    static void SetStretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static void MakeFillBar(GameObject go)
    {
        var img = go.GetComponent<Image>();
        img.type       = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Horizontal;
        img.fillAmount = 1f;
    }

    // ── Factory helpers ───────────────────────────────────────────────

    static GameObject MakeImage(Transform parent, string name, Color color)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<Image>().color = color;
        return go;
    }

    static GameObject MakeTMP(Transform parent, string name, string text,
        float fontSize, TextAlignmentOptions align)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        var tmp       = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.color     = Color.white;
        tmp.alignment = align;
        return go;
    }

    static GameObject MakeButton(Transform parent, string name, string label, float fontSize)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img   = go.AddComponent<Image>();
        img.color = Hex("2A2A3AFF");
        var btn   = go.AddComponent<Button>();
        btn.targetGraphic = img;

        var textGO = MakeTMP(go.transform, "Text", label, fontSize, TextAlignmentOptions.Center);
        SetStretch(textGO, 0, 0, 0, 0);
        return go;
    }

    /// Parse a hex colour string like "1E1E2ECC".
    static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out Color c);
        return c;
    }
}

/// Extension to allow inline lambda on RectTransform.
internal static class RectTransformExt
{
    internal static T Let<T>(this T self, System.Action<T> action) { action(self); return self; }
}
