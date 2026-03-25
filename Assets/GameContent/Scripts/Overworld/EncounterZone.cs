using UnityEngine;

public class EncounterZone : MonoBehaviour
{
    [SerializeField] private PokemonData[] possiblePokemon;
    [SerializeField] private Vector2Int levelRange = new Vector2Int(3, 7);
    [SerializeField] [Range(0f, 1f)] private float encounterRate = 0.1f;

    public void OnPlayerStepST()
    {
        if (possiblePokemon == null || possiblePokemon.Length == 0) return;
        if (Random.value > encounterRate) return;

        Debug.Log("Encounter triggered!");

        PokemonData picked = possiblePokemon[Random.Range(0, possiblePokemon.Length)];
        int lvl = Random.Range(levelRange.x, levelRange.y + 1);
        PokemonInstance enemy = PokemonData.CreateInstanceST(picked, lvl);

        BattleManager bm = FindAnyObjectByType<BattleManager>();
        bm?.StartWildEncounterST(enemy);
    }
}
