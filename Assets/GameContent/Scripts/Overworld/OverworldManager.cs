using UnityEngine;

public class OverworldManager : MonoBehaviour
{
    private static OverworldManager _instance;
    public static OverworldManager GetInstanceST() => _instance;

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        GameManager.OnStateChanged += OnGameStateChangedST;

        GameManager gm = GameManager.GetInstanceST();
        if (gm != null && gm.CurrentState == GameState.Overworld)
        {
            // Normal flow: arrived here via GameManager state transition.
            OnGameStateChangedST(GameState.Overworld);
        }
        else
        {
            // Direct-scene play (editor testing): init without GameManager state.
            InitMapST();
        }
    }

    private void OnDestroy()
    {
        GameManager.OnStateChanged -= OnGameStateChangedST;
        if (_instance == this) _instance = null;
    }

    private void OnGameStateChangedST(GameState state)
    {
        if (state != GameState.Overworld) return;
        InitMapST();
    }

    private void InitMapST()
    {
        MapManager mm = MapManager.GetInstanceST();
        if (mm == null) return;

        SaveLoadManager slm = SaveLoadManager.GetInstanceST();
        MapRunData saved = slm?.ActiveSaveData?.player?.activeMapRun;

        if (saved != null)
            mm.RestoreRunST(saved);
        else
            mm.InitNewRunST();
    }

    public void SaveMapStateST()
    {
        SaveLoadManager slm = SaveLoadManager.GetInstanceST();
        if (slm?.ActiveSaveData?.player == null) return;

        MapManager mm = MapManager.GetInstanceST();
        if (mm?.RunData != null)
            slm.ActiveSaveData.player.activeMapRun = mm.RunData;
    }
}
