using UnityEngine;

public static class EnemyAI
{
    public static MoveData ChooseMoveST(PokemonInstance enemy, PokemonInstance player)
    {
        // Collect usable moves (have PP)
        var usable = new System.Collections.Generic.List<(MoveData move, float weight)>();

        for (int i = 0; i < enemy.moves.Length; i++)
        {
            if (enemy.moves[i] == null || enemy.currentPP[i] <= 0) continue;
            float weight = enemy.moves[i].category != MoveCategory.Status ? 2f : 1f;
            usable.Add((enemy.moves[i], weight));
        }

        if (usable.Count == 0) return null;

        float totalWeight = 0f;
        foreach (var (_, w) in usable) totalWeight += w;

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;
        foreach (var (move, w) in usable)
        {
            cumulative += w;
            if (roll <= cumulative) return move;
        }

        return usable[usable.Count - 1].move;
    }
}
