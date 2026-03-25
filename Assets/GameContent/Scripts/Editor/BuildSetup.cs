using UnityEditor;

public static class BuildSetup
{
    [MenuItem("CoffinHill/Setup Build Settings")]
    public static void SetupBuildSettingsST()
    {
        var scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/GameContent/Scenes/Boot.unity",    true),
            new EditorBuildSettingsScene("Assets/GameContent/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene("Assets/GameContent/Scenes/Overworld.unity",true),
            new EditorBuildSettingsScene("Assets/GameContent/Scenes/Battle.unity",   true),
        };
        EditorBuildSettings.scenes = scenes;

        PlayerSettings.companyName = "CoffinHill";
        PlayerSettings.productName = "Coffin Hill";

        // Android portrait lock
        PlayerSettings.defaultInterfaceOrientation       = UIOrientation.Portrait;
        PlayerSettings.allowedAutorotateToPortrait        = true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft   = false;
        PlayerSettings.allowedAutorotateToLandscapeRight  = false;

        UnityEngine.Debug.Log("CoffinHill: Build settings configured.");
    }
}
