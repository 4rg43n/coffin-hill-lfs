/// <summary>
/// Converts serialized PokemonInstanceSaveData back into a runtime PokemonInstance.
/// Lives in the default assembly so it can see both CoffinHill.Data and SaveLoad types.
/// </summary>
public static class PokemonHydrator
{
    public static PokemonInstance HydrateST(PokemonInstanceSaveData save)
    {
        if (save == null) return null;
        PokemonData data = PokemonDatabase.GetInstanceST()?.GetByNumberST(save.pokedexNumber);
        if (data == null) return null;

        var inst = new PokemonInstance
        {
            data         = data,
            nickname     = save.nickname,
            level        = save.level,
            currentHP    = save.currentHP,
            totalExp     = save.totalExp,
            ivs          = save.ivs,
            evs          = save.evs,
            status       = save.status,
            sleepCounter = save.sleepCounter,
        };

        // Rebuild moves from learnset (move-index database not yet implemented)
        int slot = 0;
        foreach (LevelMove lm in data.learnset)
        {
            if (lm.level <= save.level && lm.move != null)
            {
                inst.moves[slot % 4] = lm.move;
                slot++;
            }
        }
        for (int i = 0; i < 4; i++)
        {
            if (inst.moves[i] == null) continue;
            inst.currentPP[i] = save.movePPCurrent[i] > 0
                ? save.movePPCurrent[i]
                : inst.moves[i].maxPP;
        }
        return inst;
    }
}
