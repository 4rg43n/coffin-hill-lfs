using System;
using System.Collections.Generic;

[Serializable]
public class MapRunData
{
    public List<MapNodeData> nodes;
    public int currentNodeId;       // -1 = run not yet started
    public List<int> visitedNodeIds;
    public int seed;

    public MapRunData()
    {
        nodes          = new List<MapNodeData>();
        currentNodeId  = -1;
        visitedNodeIds = new List<int>();
        seed           = -1;
    }

    public MapNodeData GetNodeST(int id)
    {
        return nodes.Find(n => n.id == id);
    }

    public List<MapNodeData> GetLayerST(int layerIndex)
    {
        return nodes.FindAll(n => n.layerIndex == layerIndex);
    }
}
