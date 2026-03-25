using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
{
    public static MapManager GetInstanceST() => _instance;
    private static MapManager _instance;

    [SerializeField] private MapView mapView;
    [SerializeField] private int mapLayers = 15;

    private MapRunData _runData;

    public MapRunData RunData             => _runData;
    public Transform  PlayerMarkerTransform { get; private set; }
    public event Action OnMapTravelledST;

    public void SetPlayerMarkerST(Transform t) => PlayerMarkerTransform = t;

    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    public void InitNewRunST()
    {
        _runData = MapGenerator.GenerateST(mapLayers);
        mapView?.BuildST(_runData);
    }

    public void RestoreRunST(MapRunData data)
    {
        _runData = data;
        mapView?.BuildST(_runData);
    }

    public bool IsNodeAvailableST(int nodeId)
    {
        if (_runData == null) return false;
        MapNodeData current = _runData.GetNodeST(_runData.currentNodeId);
        if (current == null) return false;
        return current.childIds.Contains(nodeId);
    }

    public void TravelToNodeST(int nodeId)
    {
        if (_runData == null) return;
        if (!IsNodeAvailableST(nodeId)) return;

        MapNodeData node = _runData.GetNodeST(nodeId);
        if (node == null) return;

        _runData.currentNodeId = nodeId;
        if (!_runData.visitedNodeIds.Contains(nodeId))
            _runData.visitedNodeIds.Add(nodeId);

        mapView?.RefreshST();
        OnMapTravelledST?.Invoke();

        TriggerNodeOutcomeST(node);
    }

    // -------------------------------------------------------------------------
    // Private
    // -------------------------------------------------------------------------

    private void TriggerNodeOutcomeST(MapNodeData node)
    {
        switch (node.type)
        {
            case MapNodeType.Battle:
            case MapNodeType.Elite:
            case MapNodeType.Boss:
                StartBattleST(node);
                break;

            case MapNodeType.Rest:
                HealPartyST();
                break;

            case MapNodeType.NPC:
                TriggerNPCEventST();
                break;

            case MapNodeType.Entrance:
                // No outcome — starting node only
                break;
        }
    }

    private void StartBattleST(MapNodeData node)
    {
        Debug.Log($"[MapManager] Starting battle at node {node.id} ({node.type})");

        PokemonDatabase db = PokemonDatabase.GetInstanceST();
        if (db == null || db.allPokemon == null || db.allPokemon.Length == 0)
        {
            Debug.LogError("[MapManager] PokemonDatabase not found — cannot start battle.");
            return;
        }

        // Pick a random species
        PokemonData species = db.allPokemon[UnityEngine.Random.Range(0, db.allPokemon.Length)];

        // Scale level to layer + node type
        var (minLvl, maxLvl) = LevelRangeForNodeST(node);
        int level = UnityEngine.Random.Range(minLvl, maxLvl + 1);

        PokemonInstance enemy = PokemonInstance.CreateST(species, level);
        BattleManager.QueueWildEncounterST(enemy);
    }

    private static (int min, int max) LevelRangeForNodeST(MapNodeData node)
    {
        int l = node.layerIndex;
        return node.type switch
        {
            MapNodeType.Battle => (Mathf.Max(2, l),     Mathf.Max(5,  l + 3)),
            MapNodeType.Elite  => (Mathf.Max(5, l + 2), Mathf.Max(9,  l + 6)),
            MapNodeType.Boss   => (Mathf.Max(8, l + 4), Mathf.Max(12, l + 8)),
            _                  => (3, 7)
        };
    }

    private void HealPartyST()
    {
        Debug.Log("[MapManager] Rest node — loading HealParty scene.");
        SceneManager.LoadScene("HealParty");
    }

    private void TriggerNPCEventST()
    {
        Debug.Log("[MapManager] NPC node — loading NPCEvent scene.");
        SceneManager.LoadScene("NPCEvent");
    }
}
