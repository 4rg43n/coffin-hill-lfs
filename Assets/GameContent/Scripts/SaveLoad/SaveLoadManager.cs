using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveLoadManager : MonoBehaviour
{
    private static SaveLoadManager _instance;
    public static SaveLoadManager GetInstanceST() => _instance;

    private ISaveLoadFactory _factory;

    public int ActiveSlot { get; private set; } = -1;
    public GameSaveData ActiveSaveData { get; private set; }

    private void Awake()
    {
        _instance = this;
        _factory  = new ES3SaveLoadFactory();
        GameManager.RegisterSaveLoadManagerST(this);
    }

    private void Start()
    {
        Application.quitting += OnApplicationQuitting;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        Application.quitting -= OnApplicationQuitting;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Auto-save on scene load (except Boot)
        if (ActiveSaveData != null && scene.name != "Boot")
            SaveActiveSlateST();
    }

    private void OnApplicationQuitting()
    {
        if (ActiveSaveData != null)
            SaveActiveSlateST();
    }

    public bool HasAnySaveST()
    {
        for (int i = 0; i < 3; i++)
            if (_factory.SlotExistsST(i)) return true;
        return false;
    }

    public void CreateNewSaveST(int slotIndex)
    {
        ActiveSlot = slotIndex;
        ActiveSaveData = _factory.CreateNewSaveDataST(slotIndex, "Player");
        SaveActiveSlateST();
    }

    public void LoadSlotST(int slotIndex)
    {
        GameSaveData data = _factory.LoadFromSlotST(slotIndex);
        if (data == null)
        {
            Debug.LogWarning($"SaveLoadManager: No save found at slot {slotIndex}");
            return;
        }
        ActiveSlot     = slotIndex;
        ActiveSaveData = data;

        // Apply settings
        AudioManager am = AudioManager.GetInstanceST();
        am?.ApplySettingsSaveDataST(ActiveSaveData.settings);
    }

    public void SaveActiveSlateST()
    {
        if (ActiveSaveData == null || ActiveSlot < 0) return;

        // Collect settings
        AudioManager am = AudioManager.GetInstanceST();
        if (am != null)
            ActiveSaveData.settings = am.GetSettingsSaveDataST();

        // Collect player position from Overworld if loaded
        OverworldManager om = FindAnyObjectByType<OverworldManager>();
        om?.SaveMapStateST();

        _factory.SaveToSlotST(ActiveSlot, ActiveSaveData);
    }
}
