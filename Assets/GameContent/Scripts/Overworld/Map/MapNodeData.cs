using System;
using System.Collections.Generic;

[Serializable]
public class MapNodeData
{
    public int id;
    public MapNodeType type;
    public int layerIndex;
    public float normalizedX;       // 0–1 horizontal position within layer
    public List<int> childIds;      // connections to nodes in the next layer

    public MapNodeData(int id, MapNodeType type, int layerIndex, float normalizedX)
    {
        this.id          = id;
        this.type        = type;
        this.layerIndex  = layerIndex;
        this.normalizedX = normalizedX;
        this.childIds    = new List<int>();
    }
}
