using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Auto-initializes core singletons when entering Play Mode from any scene other than Boot.
/// Fires after Awake but before Start, so GameState is set before any manager's Start() runs.
/// Requires a "GameManager" prefab placed in Assets/GameContent/Resources/.
/// </summary>
public static class Bootstrapper
{
    // AfterSceneLoad fires after all Awakes but before any Start — perfect for injecting state.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InitST()
    {
        if (GameManager.GetInstanceST() != null) return;

        var prefab = Resources.Load<GameManager>("GameManager");
        if (prefab == null)
        {
            Debug.LogWarning("[Bootstrapper] No 'GameManager' prefab found in Resources/. " +
                             "Drag the GameManager GameObject from Boot scene into " +
                             "Assets/GameContent/Resources/ to enable direct-scene testing.");
            return;
        }

        GameManager gm = Object.Instantiate(prefab);

        // Map scene name to GameState so guards (e.g. MapCameraController) pass correctly.
        GameState state = SceneManager.GetActiveScene().name switch
        {
            "Overworld"  => GameState.Overworld,
            "MainMenu"   => GameState.MainMenu,
            "Battle"     => GameState.Battle,
            "HealParty"  => GameState.Overworld,
            "NPCEvent"   => GameState.Overworld,
            _            => GameState.Boot
        };

        gm.ForceStateST(state);
        Debug.Log($"[Bootstrapper] GameManager bootstrapped. State forced to {state}.");
    }
}
