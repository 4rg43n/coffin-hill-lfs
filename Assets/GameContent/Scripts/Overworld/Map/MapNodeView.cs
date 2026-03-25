using UnityEngine;

public class MapNodeView : MonoBehaviour
{
    private MapNodeData    _data;
    private MapManager     _manager;
    private SpriteRenderer _sprite;

    private static readonly Color ColourEntrance = new Color(0.4f, 0.9f, 0.4f);
    private static readonly Color ColourBattle   = new Color(0.9f, 0.3f, 0.3f);
    private static readonly Color ColourElite    = new Color(0.9f, 0.5f, 0.1f);
    private static readonly Color ColourNPC      = new Color(0.3f, 0.6f, 0.9f);
    private static readonly Color ColourRest     = new Color(0.4f, 0.8f, 0.6f);
    private static readonly Color ColourBoss     = new Color(0.7f, 0.1f, 0.9f);

    public void InitST(MapNodeData data, MapManager manager, Sprite circleSprite)
    {
        _data    = data;
        _manager = manager;

        _sprite              = gameObject.AddComponent<SpriteRenderer>();
        _sprite.sprite       = circleSprite;
        _sprite.color        = TypeColourST(data.type);
        _sprite.sortingOrder = 1;

        CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
        col.radius = 0.5f;

        // World-space text label (child GO)
        GameObject labelGo = new GameObject("Label");
        labelGo.transform.SetParent(transform, false);
        TextMesh tm  = labelGo.AddComponent<TextMesh>();
        tm.text      = TypeLabelST(data.type);
        tm.fontSize  = 28;
        tm.fontStyle = FontStyle.Bold;
        tm.anchor    = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color     = Color.white;
        labelGo.GetComponent<MeshRenderer>().sortingOrder = 2;
        labelGo.transform.localPosition = Vector3.zero;
        labelGo.transform.localScale    = new Vector3(0.08f, 0.08f, 1f);

        RefreshStateST();
    }

    public void RefreshStateST()
    {
        if (_data == null || _manager == null || _sprite == null) return;

        MapRunData run     = _manager.RunData;
        bool isCurrent     = run.currentNodeId == _data.id;
        bool isVisited     = run.visitedNodeIds.Contains(_data.id);
        bool isAvailable   = _manager.IsNodeAvailableST(_data.id);

        MapNodeData currentNode = run.GetNodeST(run.currentNodeId);
        int layersAhead = currentNode != null ? _data.layerIndex - currentNode.layerIndex : 0;

        Color c = TypeColourST(_data.type);

        if (isCurrent)
        {
            c.a = 1f;
            transform.localScale = Vector3.one * 1.3f;
        }
        else if (isVisited)
        {
            c.a = 0.3f;
            transform.localScale = Vector3.one;
        }
        else if (isAvailable)
        {
            c.a = 1f;
            transform.localScale = Vector3.one;
        }
        else if (layersAhead > 1)
        {
            c.a = 0.08f;
            transform.localScale = Vector3.one;
        }
        else
        {
            c.a = 0.4f;
            transform.localScale = Vector3.one;
        }

        _sprite.color = c;
    }

    public void OnTappedST()
    {
        if (_manager == null) return;
        if (!_manager.IsNodeAvailableST(_data.id)) return;
        _manager.TravelToNodeST(_data.id);
    }

    private static Color TypeColourST(MapNodeType t) => t switch
    {
        MapNodeType.Entrance => ColourEntrance,
        MapNodeType.Battle   => ColourBattle,
        MapNodeType.Elite    => ColourElite,
        MapNodeType.NPC      => ColourNPC,
        MapNodeType.Rest     => ColourRest,
        MapNodeType.Boss     => ColourBoss,
        _                    => Color.white
    };

    private static string TypeLabelST(MapNodeType t) => t switch
    {
        MapNodeType.Entrance => "EN",
        MapNodeType.Battle   => "BA",
        MapNodeType.Elite    => "EL",
        MapNodeType.NPC      => "NP",
        MapNodeType.Rest     => "RE",
        MapNodeType.Boss     => "BO",
        _                    => "?"
    };
}
