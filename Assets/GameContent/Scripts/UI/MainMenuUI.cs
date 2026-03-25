using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button settingsButton;

    private void Start()
    {
        SaveLoadManager slm = SaveLoadManager.GetInstanceST();
        bool hasSave = slm != null && slm.HasAnySaveST();
        if (continueButton != null)
            continueButton.interactable = hasSave;
    }

    public void OnNewGame()
    {
        Debug.Log("New game pressed.");
        SaveLoadManager slm = SaveLoadManager.GetInstanceST();
        slm?.CreateNewSaveST(0);
        GameManager.GetInstanceST()?.ChangeStateST(GameState.Overworld);
    }

    public void OnContinue()
    {
        Debug.Log("Continue pressed.");
        SaveLoadManager slm = SaveLoadManager.GetInstanceST();
        slm?.LoadSlotST(0);
        string scene = slm?.ActiveSaveData?.player?.currentMapScene ?? "Overworld";
        GameManager.GetInstanceST()?.SceneTransitionManager.LoadSceneST(scene);
        GameManager.GetInstanceST()?.ChangeStateST(GameState.Overworld);
    }

    public void OnSettings()
    {
        Debug.Log("Settings pressed.");
        Debug.Log("Settings not yet implemented.");
    }
}
