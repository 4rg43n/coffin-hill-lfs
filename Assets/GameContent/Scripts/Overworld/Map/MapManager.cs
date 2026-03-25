using System;
using UnityEngine;

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
        // Placeholder: wire up to BattleManager when encounter data is set up per node
        Debug.Log($"[MapManager] Starting battle at node {node.id} ({node.type})");
        //GameManager.GetInstanceST()?.ChangeStateST(GameState.Battle);
    }

    private void HealPartyST()
    {
        // TODO: heal party Pokemon to full HP
        Debug.Log("[MapManager] Rest node — healing party.");
        //DialogueManager dm = DialogueManager.GetInstanceST();
        //dm?.ShowDialogueST(new[] { "You rest and recover your strength." }, null);
    }

    private void TriggerNPCEventST()
    {
        // Placeholder: show generic dialogue until NPC event data is per-node
        Debug.Log("[MapManager] NPC node.");
        //DialogueManager dm = DialogueManager.GetInstanceST();
        //dm?.ShowDialogueST(new[] { "A stranger speaks to you from the shadows..." }, null);
    }
}
