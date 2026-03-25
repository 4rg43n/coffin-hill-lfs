using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "PokemonDatabase", menuName = "CoffinHill/Pokemon Database")]
public class PokemonDatabase : ScriptableObject
{
    public PokemonData[] allPokemon;

    private static PokemonDatabase _instance;

    public static PokemonDatabase GetInstanceST()
    {
        if (_instance == null)
            _instance = Resources.Load<PokemonDatabase>("PokemonDatabase");
        return _instance;
    }

    public PokemonData GetByNumberST(int number)
    {
        foreach (var p in allPokemon)
            if (p != null && p.pokedexNumber == number)
                return p;
        return null;
    }

    public PokemonData GetByNameST(string speciesName)
    {
        foreach (var p in allPokemon)
            if (p != null && p.speciesName == speciesName)
                return p;
        return null;
    }
}
