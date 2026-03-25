using UnityEditor;
using UnityEngine;

public static class DataSetup
{
    [MenuItem("CoffinHill/Create Data Assets")]
    public static void CreateDataAssetsST()
    {
        // TypeChart
        TypeChartData chart = ScriptableObject.CreateInstance<TypeChartData>();
        chart.InitializeDefaultChartST();
        AssetDatabase.CreateAsset(chart, "Assets/GameContent/Data/Database/TypeChart.asset");

        // PokemonDatabase
        PokemonDatabase db = ScriptableObject.CreateInstance<PokemonDatabase>();
        db.allPokemon = new PokemonData[0];
        AssetDatabase.CreateAsset(db, "Assets/GameContent/Data/Database/PokemonDatabase.asset");

        // AudioClipReferences
        AudioClipReferences acr = ScriptableObject.CreateInstance<AudioClipReferences>();
        AssetDatabase.CreateAsset(acr, "Assets/GameContent/Data/Database/AudioClipReferences.asset");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("CoffinHill: Data assets created.");
    }

    [MenuItem("CoffinHill/Create Placeholder Pokemon")]
    public static void CreatePlaceholderPokemonST()
    {
        string folder = "Assets/GameContent/Data/Pokemon";

        // Data: (name, num, primary type, secondary type, HP, Atk, Def, Spd, Spc, expYield, catchRate)
        var defs = new (string n, int num, PokemonType t1, PokemonType t2, int hp, int atk, int def, int spd, int spc, int exp, float cr)[]
        {
            ("Embercub",   1, PokemonType.Fire,  PokemonType.None,  45, 49, 49, 45, 65,  45, 0.45f),
            ("Flamepaw",   2, PokemonType.Fire,  PokemonType.None,  60, 62, 63, 60, 80,  65, 0.25f),
            ("Puddle",     3, PokemonType.Water, PokemonType.None,  45, 49, 49, 45, 65,  45, 0.45f),
            ("Tidalfin",   4, PokemonType.Water, PokemonType.None,  60, 62, 63, 60, 80,  65, 0.25f),
            ("Sproutling", 5, PokemonType.Grass, PokemonType.None,  45, 49, 49, 45, 65,  45, 0.45f),
            ("Thornweed",  6, PokemonType.Grass, PokemonType.None,  60, 62, 63, 60, 80,  65, 0.25f),
            ("Pebble",     7, PokemonType.Rock,  PokemonType.None,  50, 52, 60, 40, 55,  45, 0.45f),
            ("Boulderback",8, PokemonType.Rock,  PokemonType.Ground,70, 68, 80, 50, 65,  80, 0.25f),
        };

        foreach (var d in defs)
        {
            PokemonData p = ScriptableObject.CreateInstance<PokemonData>();
            p.pokedexNumber    = d.num;
            p.speciesName      = d.n;
            p.primaryType      = d.t1;
            p.secondaryType    = d.t2;
            p.baseHP           = d.hp;
            p.baseAttack       = d.atk;
            p.baseDefense      = d.def;
            p.baseSpeed        = d.spd;
            p.baseSpecial      = d.spc;
            p.baseExpYield     = d.exp;
            p.catchRate        = d.cr;
            p.pokedexDescription = $"A placeholder {d.t1} type creature.";
            p.learnset         = new LevelMove[0];
            AssetDatabase.CreateAsset(p, $"{folder}/{d.n}.asset");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("CoffinHill: Placeholder Pokemon assets created.");
    }

    [MenuItem("CoffinHill/Create Placeholder Moves")]
    public static void CreatePlaceholderMovesST()
    {
        string folder = "Assets/GameContent/Data/Moves";

        var defs = new (string n, PokemonType t, MoveCategory cat, int pwr, int acc, int pp)[]
        {
            ("Tackle",      PokemonType.Normal,   MoveCategory.Physical, 40, 100, 35),
            ("Scratch",     PokemonType.Normal,   MoveCategory.Physical, 40, 100, 35),
            ("Ember",       PokemonType.Fire,     MoveCategory.Special,  40, 100, 25),
            ("Water Gun",   PokemonType.Water,    MoveCategory.Special,  40, 100, 25),
            ("Vine Whip",   PokemonType.Grass,    MoveCategory.Physical, 45, 100, 25),
            ("Rock Throw",  PokemonType.Rock,     MoveCategory.Physical, 50,  90, 15),
            ("Growl",       PokemonType.Normal,   MoveCategory.Status,    0, 100, 40),
            ("Tail Whip",   PokemonType.Normal,   MoveCategory.Status,    0, 100, 30),
            ("Poison Sting",PokemonType.Poison,   MoveCategory.Physical, 15, 100, 35),
            ("Thunder Wave",PokemonType.Electric, MoveCategory.Status,    0, 100, 20),
            ("Slash",       PokemonType.Normal,   MoveCategory.Physical, 70, 100, 20),
            ("Hyper Fang",  PokemonType.Normal,   MoveCategory.Physical, 80,  90, 15),
        };

        var statusDefs = new System.Collections.Generic.Dictionary<string, StatusCondition>
        {
            { "Poison Sting",  StatusCondition.Poisoned },
            { "Thunder Wave",  StatusCondition.Paralyzed },
        };

        var critMoves = new System.Collections.Generic.HashSet<string> { "Slash" };

        foreach (var d in defs)
        {
            MoveData m = ScriptableObject.CreateInstance<MoveData>();
            m.moveName    = d.n;
            m.type        = d.t;
            m.category    = d.cat;
            m.power       = d.pwr;
            m.accuracy    = d.acc;
            m.maxPP       = d.pp;
            m.description = $"A placeholder {d.t} move.";
            m.statusEffect  = statusDefs.TryGetValue(d.n, out var sc) ? sc : StatusCondition.None;
            m.statusChance  = statusDefs.ContainsKey(d.n) ? 0.3f : 0f;
            m.isHighCrit    = critMoves.Contains(d.n);
            AssetDatabase.CreateAsset(m, $"{folder}/{d.n.Replace(" ","_")}.asset");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("CoffinHill: Placeholder Move assets created.");
    }
}
