using UnityEngine;

[CreateAssetMenu(fileName = "TypeChart", menuName = "CoffinHill/Type Chart")]
public class TypeChartData : ScriptableObject
{
    // 18x18 flat array, row = attacking type, col = defending type
    // Values: 0=immune, 0.5=not very effective, 1=normal, 2=super effective
    // Indices match PokemonType enum (Normal=0 ... Fairy=17, None=18 excluded)
    [SerializeField] private float[] chart = new float[18 * 18];

    private static TypeChartData _instance;

    public static TypeChartData GetInstanceST()
    {
        if (_instance == null)
            _instance = Resources.Load<TypeChartData>("TypeChart");
        return _instance;
    }

    public float GetEffectivenessST(PokemonType attacking, PokemonType defending)
    {
        if (attacking == PokemonType.None || defending == PokemonType.None)
            return 1f;
        int atk = (int)attacking;
        int def = (int)defending;
        if (atk >= 18 || def >= 18) return 1f;
        return chart[atk * 18 + def];
    }

    // Call this in OnEnable or from an Editor tool to populate the chart
    public void InitializeDefaultChartST()
    {
        // Fill with 1.0
        for (int i = 0; i < chart.Length; i++)
            chart[i] = 1f;

        // Shorthand helper
        void Set(PokemonType atk, PokemonType def, float val)
        {
            chart[(int)atk * 18 + (int)def] = val;
        }

        // Normal attacking
        Set(PokemonType.Normal, PokemonType.Rock, 0.5f);
        Set(PokemonType.Normal, PokemonType.Steel, 0.5f);
        Set(PokemonType.Normal, PokemonType.Ghost, 0f);

        // Fire attacking
        Set(PokemonType.Fire, PokemonType.Fire, 0.5f);
        Set(PokemonType.Fire, PokemonType.Water, 0.5f);
        Set(PokemonType.Fire, PokemonType.Rock, 0.5f);
        Set(PokemonType.Fire, PokemonType.Dragon, 0.5f);
        Set(PokemonType.Fire, PokemonType.Grass, 2f);
        Set(PokemonType.Fire, PokemonType.Ice, 2f);
        Set(PokemonType.Fire, PokemonType.Bug, 2f);
        Set(PokemonType.Fire, PokemonType.Steel, 2f);

        // Water attacking
        Set(PokemonType.Water, PokemonType.Water, 0.5f);
        Set(PokemonType.Water, PokemonType.Grass, 0.5f);
        Set(PokemonType.Water, PokemonType.Dragon, 0.5f);
        Set(PokemonType.Water, PokemonType.Fire, 2f);
        Set(PokemonType.Water, PokemonType.Ground, 2f);
        Set(PokemonType.Water, PokemonType.Rock, 2f);

        // Grass attacking
        Set(PokemonType.Grass, PokemonType.Fire, 0.5f);
        Set(PokemonType.Grass, PokemonType.Grass, 0.5f);
        Set(PokemonType.Grass, PokemonType.Poison, 0.5f);
        Set(PokemonType.Grass, PokemonType.Flying, 0.5f);
        Set(PokemonType.Grass, PokemonType.Bug, 0.5f);
        Set(PokemonType.Grass, PokemonType.Dragon, 0.5f);
        Set(PokemonType.Grass, PokemonType.Steel, 0.5f);
        Set(PokemonType.Grass, PokemonType.Water, 2f);
        Set(PokemonType.Grass, PokemonType.Ground, 2f);
        Set(PokemonType.Grass, PokemonType.Rock, 2f);

        // Electric attacking
        Set(PokemonType.Electric, PokemonType.Grass, 0.5f);
        Set(PokemonType.Electric, PokemonType.Electric, 0.5f);
        Set(PokemonType.Electric, PokemonType.Dragon, 0.5f);
        Set(PokemonType.Electric, PokemonType.Ground, 0f);
        Set(PokemonType.Electric, PokemonType.Water, 2f);
        Set(PokemonType.Electric, PokemonType.Flying, 2f);

        // Ice attacking
        Set(PokemonType.Ice, PokemonType.Water, 0.5f);
        Set(PokemonType.Ice, PokemonType.Ice, 0.5f);
        Set(PokemonType.Ice, PokemonType.Steel, 0.5f);
        Set(PokemonType.Ice, PokemonType.Fire, 0.5f);
        Set(PokemonType.Ice, PokemonType.Grass, 2f);
        Set(PokemonType.Ice, PokemonType.Ground, 2f);
        Set(PokemonType.Ice, PokemonType.Flying, 2f);
        Set(PokemonType.Ice, PokemonType.Dragon, 2f);

        // Fighting attacking
        Set(PokemonType.Fighting, PokemonType.Poison, 0.5f);
        Set(PokemonType.Fighting, PokemonType.Bug, 0.5f);
        Set(PokemonType.Fighting, PokemonType.Psychic, 0.5f);
        Set(PokemonType.Fighting, PokemonType.Flying, 0.5f);
        Set(PokemonType.Fighting, PokemonType.Fairy, 0.5f);
        Set(PokemonType.Fighting, PokemonType.Ghost, 0f);
        Set(PokemonType.Fighting, PokemonType.Normal, 2f);
        Set(PokemonType.Fighting, PokemonType.Ice, 2f);
        Set(PokemonType.Fighting, PokemonType.Rock, 2f);
        Set(PokemonType.Fighting, PokemonType.Dark, 2f);
        Set(PokemonType.Fighting, PokemonType.Steel, 2f);

        // Poison attacking
        Set(PokemonType.Poison, PokemonType.Poison, 0.5f);
        Set(PokemonType.Poison, PokemonType.Ground, 0.5f);
        Set(PokemonType.Poison, PokemonType.Rock, 0.5f);
        Set(PokemonType.Poison, PokemonType.Ghost, 0.5f);
        Set(PokemonType.Poison, PokemonType.Steel, 0f);
        Set(PokemonType.Poison, PokemonType.Grass, 2f);
        Set(PokemonType.Poison, PokemonType.Fairy, 2f);

        // Ground attacking
        Set(PokemonType.Ground, PokemonType.Grass, 0.5f);
        Set(PokemonType.Ground, PokemonType.Bug, 0.5f);
        Set(PokemonType.Ground, PokemonType.Flying, 0f);
        Set(PokemonType.Ground, PokemonType.Fire, 2f);
        Set(PokemonType.Ground, PokemonType.Electric, 2f);
        Set(PokemonType.Ground, PokemonType.Poison, 2f);
        Set(PokemonType.Ground, PokemonType.Rock, 2f);
        Set(PokemonType.Ground, PokemonType.Steel, 2f);

        // Flying attacking
        Set(PokemonType.Flying, PokemonType.Electric, 0.5f);
        Set(PokemonType.Flying, PokemonType.Rock, 0.5f);
        Set(PokemonType.Flying, PokemonType.Steel, 0.5f);
        Set(PokemonType.Flying, PokemonType.Grass, 2f);
        Set(PokemonType.Flying, PokemonType.Fighting, 2f);
        Set(PokemonType.Flying, PokemonType.Bug, 2f);

        // Psychic attacking — Gen 1 quirk: Ghost immune to Psychic, Psychic immune to Psychic(dark not in gen1 but kept here)
        Set(PokemonType.Psychic, PokemonType.Steel, 0.5f);
        Set(PokemonType.Psychic, PokemonType.Psychic, 0.5f);
        Set(PokemonType.Psychic, PokemonType.Dark, 0f);
        Set(PokemonType.Psychic, PokemonType.Ghost, 0f); // Gen 1 quirk
        Set(PokemonType.Psychic, PokemonType.Fighting, 2f);
        Set(PokemonType.Psychic, PokemonType.Poison, 2f);

        // Bug attacking
        Set(PokemonType.Bug, PokemonType.Fire, 0.5f);
        Set(PokemonType.Bug, PokemonType.Fighting, 0.5f);
        Set(PokemonType.Bug, PokemonType.Flying, 0.5f);
        Set(PokemonType.Bug, PokemonType.Ghost, 0.5f);
        Set(PokemonType.Bug, PokemonType.Steel, 0.5f);
        Set(PokemonType.Bug, PokemonType.Fairy, 0.5f);
        Set(PokemonType.Bug, PokemonType.Grass, 2f);
        Set(PokemonType.Bug, PokemonType.Psychic, 2f);
        Set(PokemonType.Bug, PokemonType.Dark, 2f);

        // Rock attacking
        Set(PokemonType.Rock, PokemonType.Fighting, 0.5f);
        Set(PokemonType.Rock, PokemonType.Ground, 0.5f);
        Set(PokemonType.Rock, PokemonType.Steel, 0.5f);
        Set(PokemonType.Rock, PokemonType.Fire, 2f);
        Set(PokemonType.Rock, PokemonType.Ice, 2f);
        Set(PokemonType.Rock, PokemonType.Flying, 2f);
        Set(PokemonType.Rock, PokemonType.Bug, 2f);

        // Ghost attacking — Gen 1 quirk: Ghost does not affect Normal or Psychic
        Set(PokemonType.Ghost, PokemonType.Normal, 0f); // Gen 1 quirk
        Set(PokemonType.Ghost, PokemonType.Psychic, 0f); // Gen 1 quirk (should be 2x but Gen1 bug)
        Set(PokemonType.Ghost, PokemonType.Dark, 0.5f);
        Set(PokemonType.Ghost, PokemonType.Steel, 0.5f);
        Set(PokemonType.Ghost, PokemonType.Ghost, 2f);

        // Dragon attacking
        Set(PokemonType.Dragon, PokemonType.Steel, 0.5f);
        Set(PokemonType.Dragon, PokemonType.Fairy, 0f);
        Set(PokemonType.Dragon, PokemonType.Dragon, 2f);

        // Dark attacking
        Set(PokemonType.Dark, PokemonType.Fighting, 0.5f);
        Set(PokemonType.Dark, PokemonType.Dark, 0.5f);
        Set(PokemonType.Dark, PokemonType.Fairy, 0.5f);
        Set(PokemonType.Dark, PokemonType.Ghost, 2f);
        Set(PokemonType.Dark, PokemonType.Psychic, 2f);

        // Steel attacking
        Set(PokemonType.Steel, PokemonType.Steel, 0.5f);
        Set(PokemonType.Steel, PokemonType.Fire, 0.5f);
        Set(PokemonType.Steel, PokemonType.Water, 0.5f);
        Set(PokemonType.Steel, PokemonType.Electric, 0.5f);
        Set(PokemonType.Steel, PokemonType.Ice, 2f);
        Set(PokemonType.Steel, PokemonType.Rock, 2f);
        Set(PokemonType.Steel, PokemonType.Fairy, 2f);

        // Fairy attacking
        Set(PokemonType.Fairy, PokemonType.Fire, 0.5f);
        Set(PokemonType.Fairy, PokemonType.Poison, 0.5f);
        Set(PokemonType.Fairy, PokemonType.Steel, 0.5f);
        Set(PokemonType.Fairy, PokemonType.Fighting, 2f);
        Set(PokemonType.Fairy, PokemonType.Dragon, 2f);
        Set(PokemonType.Fairy, PokemonType.Dark, 2f);

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}
