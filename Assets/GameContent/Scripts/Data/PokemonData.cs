using System;
using UnityEngine;

[Serializable]
public struct LevelMove
{
    public int level;
    public MoveData move;
}

[CreateAssetMenu(fileName = "NewPokemon", menuName = "CoffinHill/Pokemon Data")]
public class PokemonData : ScriptableObject
{
    [Header("Identity")]
    public int pokedexNumber;
    public string speciesName;

    [Header("Types")]
    public PokemonType primaryType;
    public PokemonType secondaryType;

    [Header("Sprites")]
    public Sprite frontSprite;
    public Sprite backSprite;

    [Header("Base Stats (Gen 1)")]
    public int baseHP;
    public int baseAttack;
    public int baseDefense;
    public int baseSpeed;
    public int baseSpecial;  // Gen 1 unified Special

    [Header("Learnset")]
    public LevelMove[] learnset;

    [Header("Misc")]
    public int baseExpYield;
    [Range(0f, 1f)] public float catchRate;
    [TextArea] public string pokedexDescription;

    public static PokemonInstance CreateInstanceST(PokemonData data, int level)
    {
        return PokemonInstance.CreateST(data, level);
    }
}
