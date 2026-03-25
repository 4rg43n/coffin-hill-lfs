using UnityEngine;

/// <summary>
/// Attach this to any GameObject in the Battle scene.
/// When the Battle scene is played directly (no GameManager present), it creates
/// two test pokemon and kicks off a battle — no save data or game flow required.
///
/// Does nothing when the scene is loaded normally from the game.
/// </summary>
public class BattleTestBootstrap : MonoBehaviour
{
    [Header("Test Configuration")]
    [Tooltip("Pokedex number of the player's test pokemon.")]
    [SerializeField] private int playerPokedexNumber = 1;

    [Tooltip("Pokedex number of the enemy test pokemon.")]
    [SerializeField] private int enemyPokedexNumber = 3;

    [Tooltip("Level for both test pokemon.")]
    [SerializeField] private int testLevel = 5;

    [Tooltip("Enable to simulate a trainer battle (shows trainer intro/outro). " +
             "Disable for a wild encounter (monsters appear immediately).")]
    [SerializeField] private bool simulateTrainerBattle = false;

    private void Start()
    {
        // Skip if a real encounter was already queued (normal game flow).
        // The Bootstrapper always creates a GameManager, so we can't use that as a sentinel.
        BattleManager bm = BattleManager.GetInstanceST();

        if (bm == null)
        {
            Debug.LogError("[BattleTestBootstrap] BattleManager not found.");
            return;
        }

        // EnemyPokemon is populated in Awake when a real encounter was queued.
        // If it's still null here, we're in standalone/direct-play test mode.
        if (bm.EnemyPokemon != null) return;

        PokemonDatabase db = PokemonDatabase.GetInstanceST();
        if (db == null)
        {
            Debug.LogError("[BattleTestBootstrap] PokemonDatabase not found in Resources.");
            return;
        }

        PokemonData playerData = db.GetByNumberST(playerPokedexNumber);
        PokemonData enemyData  = db.GetByNumberST(enemyPokedexNumber);

        // Fallback to any available pokemon if configured numbers aren't found.
        if (playerData == null && db.allPokemon.Length > 0) playerData = db.allPokemon[0];
        if (enemyData  == null && db.allPokemon.Length > 1) enemyData  = db.allPokemon[1];

        if (playerData == null || enemyData == null)
        {
            Debug.LogError("[BattleTestBootstrap] Not enough pokemon in database to run a test battle.");
            return;
        }

        PokemonInstance player = PokemonInstance.CreateST(playerData, testLevel);
        PokemonInstance enemy  = PokemonInstance.CreateST(enemyData,  testLevel);

        bool isWild = !simulateTrainerBattle;
        Debug.Log($"[BattleTestBootstrap] Starting test battle ({(isWild ? "wild" : "trainer")}): " +
                  $"{player.nickname} (Lv.{testLevel}) vs {enemy.nickname} (Lv.{testLevel})");
        bm.InitBattleST(player, enemy, isWild: isWild);
    }
}
