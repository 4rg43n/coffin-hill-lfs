using System;
using UnityEngine;

public enum GameState
{
    Boot,
    MainMenu,
    Overworld,
    Battle
}

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    [SerializeField] private InputManager inputManager;
    [SerializeField] private SceneTransitionManager sceneTransitionManager;

    // AudioManager and SaveLoadManager live in separate assemblies.
    // They self-register via RegisterAudioManagerST / RegisterSaveLoadManagerST.
    private static MonoBehaviour _audioManager;
    private static MonoBehaviour _saveLoadManager;

    public GameState CurrentState { get; private set; } = GameState.Boot;
    public static event Action<GameState> OnStateChanged;

    public InputManager InputManager => inputManager;
    public SceneTransitionManager SceneTransitionManager => sceneTransitionManager;

    // Typed accessors — cast in the calling code that knows the concrete type.
    // (AudioManager and SaveLoadManager live outside the Core assembly.)
    public MonoBehaviour AudioManagerBehaviour    => _audioManager;
    public MonoBehaviour SaveLoadManagerBehaviour => _saveLoadManager;

    public static GameManager GetInstanceST() => _instance;

    public static void RegisterAudioManagerST(MonoBehaviour am)   => _audioManager   = am;
    public static void RegisterSaveLoadManagerST(MonoBehaviour sm) => _saveLoadManager = sm;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        GetComponent<PortraitLock>()?.EnforcePortrait();
    }

    public void ChangeStateST(GameState newState)
    {
        if (CurrentState == newState) return;
        CurrentState = newState;
        OnStateChanged?.Invoke(newState);

        switch (newState)
        {
            case GameState.MainMenu:
                sceneTransitionManager?.LoadSceneST("MainMenu");
                break;
            case GameState.Overworld:
                sceneTransitionManager?.LoadSceneST("Overworld");
                break;
            case GameState.Battle:
                // Battle is loaded additively by BattleManager
                break;
        }
    }
}
