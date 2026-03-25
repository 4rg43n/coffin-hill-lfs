using UnityEngine;

public static class BattleFormulas
{
    // Gen 1 damage formula
    public static int CalculateDamageST(PokemonInstance attacker, PokemonInstance defender,
        MoveData move, bool isCrit)
    {
        if (move.category == MoveCategory.Status || move.power == 0)
            return 0;

        int level = isCrit ? attacker.level * 2 : attacker.level;

        float atkStat = move.category == MoveCategory.Physical
            ? PokemonInstance.GetStatST(attacker, PokemonInstance.StatAtk)
            : PokemonInstance.GetStatST(attacker, PokemonInstance.StatSpc);

        float defStat = move.category == MoveCategory.Physical
            ? PokemonInstance.GetStatST(defender, PokemonInstance.StatDef)
            : PokemonInstance.GetStatST(defender, PokemonInstance.StatSpc);

        // Halve Atk/Def for burn on attacker
        if (attacker.status == StatusCondition.Burned && move.category == MoveCategory.Physical)
            atkStat *= 0.5f;

        float damage = ((2f * level / 5f + 2f) * move.power * (atkStat / defStat)) / 50f + 2f;

        // STAB
        if (move.type == attacker.data.primaryType || move.type == attacker.data.secondaryType)
            damage *= 1.5f;

        // Type effectiveness
        TypeChartData chart = TypeChartData.GetInstanceST();
        if (chart != null)
        {
            float eff = chart.GetEffectivenessST(move.type, defender.data.primaryType);
            if (defender.data.secondaryType != PokemonType.None)
                eff *= chart.GetEffectivenessST(move.type, defender.data.secondaryType);
            damage *= eff;
        }

        // Random factor 0.85–1.0
        damage *= Random.Range(0.85f, 1.001f);

        return Mathf.Max(1, Mathf.FloorToInt(damage));
    }

    public static bool IsCritST(PokemonInstance attacker, MoveData move)
    {
        int speed = PokemonInstance.GetStatST(attacker, PokemonInstance.StatSpd);
        float threshold = move.isHighCrit ? speed / 64f : speed / 512f;
        return Random.value < threshold;
    }

    public static int GetExpGainST(PokemonInstance fainted, int partySize, bool isTrainer)
    {
        float exp = (float)(fainted.data.baseExpYield * fainted.level) / (7f * partySize);
        if (isTrainer) exp *= 1.5f;
        return Mathf.Max(1, Mathf.FloorToInt(exp));
    }

    public static float CalculateCatchRateST(PokemonInstance target, float ballModifier)
    {
        int maxHP = PokemonInstance.GetMaxHPST(target);
        float rate = (target.data.catchRate * 255f * ballModifier)
                     / (4f * ((float)maxHP / target.currentHP));
        return Mathf.Clamp01(rate / 255f);
    }
}
