using System;
using UnityEngine;

[Serializable]
public class PokemonInstance
{
    public PokemonData data;
    public string nickname;
    public int level;
    public int currentHP;
    public int[] ivs = new int[6];   // 0=HP,1=Atk,2=Def,3=Spd,4=SpAtk,5=SpDef (Gen1: only 5 stats)
    public int[] evs = new int[6];
    public StatusCondition status;
    public int sleepCounter;
    public MoveData[] moves = new MoveData[4];
    public int[] currentPP = new int[4];
    public int totalExp;

    // Stat indices
    public const int StatHP  = 0;
    public const int StatAtk = 1;
    public const int StatDef = 2;
    public const int StatSpd = 3;
    public const int StatSpc = 4;

    public static PokemonInstance CreateST(PokemonData data, int level)
    {
        var inst = new PokemonInstance();
        inst.data = data;
        inst.nickname = data.speciesName;
        inst.level = level;

        // Random IVs 0–15
        for (int i = 0; i < inst.ivs.Length; i++)
            inst.ivs[i] = UnityEngine.Random.Range(0, 16);

        // Assign moves from learnset up to this level
        int moveSlot = 0;
        foreach (LevelMove lm in data.learnset)
        {
            if (lm.level <= level && lm.move != null)
            {
                inst.moves[moveSlot % 4] = lm.move;
                moveSlot++;
            }
        }
        for (int i = 0; i < 4; i++)
        {
            if (inst.moves[i] != null)
                inst.currentPP[i] = inst.moves[i].maxPP;
        }

        inst.currentHP = GetMaxHPST(inst);
        inst.totalExp = level * level * level; // simplified
        return inst;
    }

    // Gen 1 HP formula: ((Base + IV) * 2 + sqrt(EV) / 4) * Level / 100 + Level + 10
    public static int GetMaxHPST(PokemonInstance inst)
    {
        int baseHP = inst.data.baseHP;
        int iv = inst.ivs[StatHP];
        int ev = inst.evs[StatHP];
        int evBonus = (int)(Math.Sqrt(ev) / 4);
        return ((baseHP + iv) * 2 + evBonus) * inst.level / 100 + inst.level + 10;
    }

    // Gen 1 other stat formula: ((Base + IV) * 2 + sqrt(EV) / 4) * Level / 100 + 5
    public static int GetStatST(PokemonInstance inst, int statIndex)
    {
        int baseStat = GetBaseStat(inst.data, statIndex);
        int iv = inst.ivs[statIndex];
        int ev = inst.evs[statIndex];
        int evBonus = (int)(Math.Sqrt(ev) / 4);
        return ((baseStat + iv) * 2 + evBonus) * inst.level / 100 + 5;
    }

    private static int GetBaseStat(PokemonData data, int statIndex)
    {
        switch (statIndex)
        {
            case StatHP:  return data.baseHP;
            case StatAtk: return data.baseAttack;
            case StatDef: return data.baseDefense;
            case StatSpd: return data.baseSpeed;
            case StatSpc: return data.baseSpecial;
            default:      return data.baseSpecial;
        }
    }

    public bool IsAlive => currentHP > 0;
}
