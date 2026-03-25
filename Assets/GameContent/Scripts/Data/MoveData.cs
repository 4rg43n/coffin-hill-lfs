using UnityEngine;

[CreateAssetMenu(fileName = "NewMove", menuName = "CoffinHill/Move Data")]
public class MoveData : ScriptableObject
{
    [Header("Identity")]
    public string moveName;
    [TextArea] public string description;

    [Header("Type & Category")]
    public PokemonType type;
    public MoveCategory category;

    [Header("Stats")]
    public int power;
    [Range(0, 100)] public int accuracy;
    public int maxPP;

    [Header("Status Effect")]
    public StatusCondition statusEffect;
    [Range(0f, 1f)] public float statusChance;

    [Header("Flags")]
    public bool isHighCrit;
}
