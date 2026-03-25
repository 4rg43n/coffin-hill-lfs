using System.Collections.Generic;
using UnityEngine;

public static class MapGenerator
{
    // Tier thresholds (layer indices, exclusive of layer 0 Start and last Boss)
    private const int EarlyMax = 5;
    private const int MidMax   = 10;

    public static MapRunData GenerateST(int layers = 15, int seed = -1)
    {
        if (seed == -1) seed = Random.Range(0, int.MaxValue);
        Random.InitState(seed);

        MapRunData run   = new MapRunData();
        run.seed         = seed;
        int nextId       = 0;
        int lastLayer    = layers - 1;

        // --- Build nodes layer by layer ---
        for (int layer = 0; layer < layers; layer++)
        {
            if (layer == 0)
            {
                run.nodes.Add(new MapNodeData(nextId++, MapNodeType.Entrance, 0, 0.5f));
            }
            else if (layer == lastLayer)
            {
                run.nodes.Add(new MapNodeData(nextId++, MapNodeType.Boss, lastLayer, 0.5f));
            }
            else
            {
                int count = Random.Range(1, 4); // 1–3 nodes
                for (int i = 0; i < count; i++)
                {
                    float nx = count == 1 ? 0.5f : Mathf.Lerp(0.15f, 0.85f, (float)i / (count - 1));
                    MapNodeType type = PickTypeST(layer);
                    run.nodes.Add(new MapNodeData(nextId++, type, layer, nx));
                }
            }
        }

        // --- Connect layers ---
        for (int layer = 0; layer < lastLayer; layer++)
        {
            List<MapNodeData> current = run.GetLayerST(layer);
            List<MapNodeData> next    = run.GetLayerST(layer + 1);

            // Sort both by normalizedX to prefer non-crossing connections
            current.Sort((a, b) => a.normalizedX.CompareTo(b.normalizedX));
            next.Sort((a, b) => a.normalizedX.CompareTo(b.normalizedX));

            // Ensure every node in 'next' has at least one parent
            HashSet<int> coveredNext = new HashSet<int>();

            foreach (MapNodeData node in current)
            {
                // Connect to 1–2 nearest nodes in next layer
                int connections = Random.Range(1, next.Count > 1 ? 3 : 2);
                List<MapNodeData> candidates = NearestST(node, next);
                for (int c = 0; c < Mathf.Min(connections, candidates.Count); c++)
                {
                    if (!node.childIds.Contains(candidates[c].id))
                        node.childIds.Add(candidates[c].id);
                    coveredNext.Add(candidates[c].id);
                }
            }

            // Patch any orphaned next-layer nodes
            foreach (MapNodeData nextNode in next)
            {
                if (coveredNext.Contains(nextNode.id)) continue;
                // Connect the nearest current-layer node to this orphan
                MapNodeData nearest = NearestST(nextNode, current)[0];
                if (!nearest.childIds.Contains(nextNode.id))
                    nearest.childIds.Add(nextNode.id);
            }
        }

        // Set currentNodeId to the Start node so the player can immediately tap adjacent nodes
        run.currentNodeId = run.GetLayerST(0)[0].id;
        run.visitedNodeIds.Add(run.currentNodeId);

        return run;
    }

    // -------------------------------------------------------------------------

    private static MapNodeType PickTypeST(int layer)
    {
        float roll = Random.value;

        if (layer <= EarlyMax)
        {
            // Early: Battle 60%, Rest 20%, NPC 20%
            if (roll < 0.60f) return MapNodeType.Battle;
            if (roll < 0.80f) return MapNodeType.Rest;
            return MapNodeType.NPC;
        }
        else if (layer <= MidMax)
        {
            // Mid: Battle 40%, Elite 20%, Rest 20%, NPC 20%
            if (roll < 0.40f) return MapNodeType.Battle;
            if (roll < 0.60f) return MapNodeType.Elite;
            if (roll < 0.80f) return MapNodeType.Rest;
            return MapNodeType.NPC;
        }
        else
        {
            // Late: Elite 50%, Battle 30%, NPC 20%
            if (roll < 0.50f) return MapNodeType.Elite;
            if (roll < 0.80f) return MapNodeType.Battle;
            return MapNodeType.NPC;
        }
    }

    private static List<MapNodeData> NearestST(MapNodeData from, List<MapNodeData> candidates)
    {
        List<MapNodeData> sorted = new List<MapNodeData>(candidates);
        sorted.Sort((a, b) =>
            Mathf.Abs(a.normalizedX - from.normalizedX)
                .CompareTo(Mathf.Abs(b.normalizedX - from.normalizedX)));
        return sorted;
    }
}
