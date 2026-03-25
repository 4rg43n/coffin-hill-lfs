using UnityEngine;

public static class CatchSystem
{
    public static CatchResult AttemptCatchST(PokemonInstance target, float pokeballModifier)
    {
        float catchProb = BattleFormulas.CalculateCatchRateST(target, pokeballModifier);

        // Shakiness check — 4 shakes required in Gen 1 simplified
        int shakes = 0;
        for (int i = 0; i < 4; i++)
        {
            if (Random.value < catchProb)
                shakes++;
            else
                break;
        }

        switch (shakes)
        {
            case 4: return CatchResult.Caught;
            case 3: return CatchResult.Shake3;
            case 2: return CatchResult.Shake2;
            case 1: return CatchResult.Shake1;
            default: return CatchResult.Failed;
        }
    }
}
