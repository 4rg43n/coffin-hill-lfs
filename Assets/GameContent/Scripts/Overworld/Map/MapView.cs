using System.Collections.Generic;
using UnityEngine;

public class MapView : MonoBehaviour
{
    [SerializeField] private float layerSpacingUnits = 3f;
    [SerializeField] private float mapWidthUnits      = 8f;

    private MapRunData        _data;
    private List<MapNodeView> _nodeViews    = new List<MapNodeView>();
    private Transform         _playerMarker;
    private Sprite            _circleSprite;

    private void Awake()
    {
        _circleSprite = CreateCircleSpriteST(64);
    }

    public void BuildST(MapRunData data)
    {
        _data = data;
        _nodeViews.Clear();

        foreach (Transform child in transform)
            Destroy(child.gameObject);

        MapManager mm = MapManager.GetInstanceST();

        // Lines behind nodes
        foreach (MapNodeData node in data.nodes)
            foreach (int childId in node.childIds)
            {
                MapNodeData child = data.GetNodeST(childId);
                if (child != null) DrawLineST(WorldPosST(node), WorldPosST(child));
            }

        // Node sprites
        foreach (MapNodeData node in data.nodes)
        {
            GameObject go = new GameObject($"Node_{node.id}_{node.type}");
            go.transform.SetParent(transform, false);
            go.transform.position = WorldPosST(node);

            MapNodeView view = go.AddComponent<MapNodeView>();
            view.InitST(node, mm, _circleSprite);
            _nodeViews.Add(view);
        }

        // Player marker (gold circle, on top of nodes)
        _playerMarker = CreatePlayerMarkerST();
        PlacePlayerMarkerST();

        mm?.SetPlayerMarkerST(_playerMarker);

        // Centre camera after build (only on initial load)
        MapCameraController.GetInstanceST()?.CenterOnPlayerST();
    }

    public void RefreshST()
    {
        foreach (MapNodeView v in _nodeViews)
            v.RefreshStateST();
        PlacePlayerMarkerST();
    }

    // -------------------------------------------------------------------------

    private void PlacePlayerMarkerST()
    {
        if (_playerMarker == null || _data == null) return;
        MapNodeData current = _data.GetNodeST(_data.currentNodeId);
        if (current != null)
            _playerMarker.position = WorldPosST(current);
    }

    private Transform CreatePlayerMarkerST()
    {
        GameObject go = new GameObject("PlayerMarker");
        go.transform.SetParent(transform, false);
        go.transform.localScale = Vector3.one * 0.65f;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = _circleSprite;
        sr.color        = new Color(1f, 0.85f, 0f);   // gold
        sr.sortingOrder = 3;

        return go.transform;
    }

    private void DrawLineST(Vector3 from, Vector3 to)
    {
        GameObject go = new GameObject("Line");
        go.transform.SetParent(transform, false);

        LineRenderer lr    = go.AddComponent<LineRenderer>();
        lr.useWorldSpace   = true;
        lr.positionCount   = 2;
        lr.SetPosition(0, from);
        lr.SetPosition(1, to);
        lr.startWidth      = 0.06f;
        lr.endWidth        = 0.06f;
        lr.sortingOrder    = 0;
        lr.material        = new Material(Shader.Find("Sprites/Default"));
        lr.startColor      = new Color(0.55f, 0.55f, 0.55f, 0.55f);
        lr.endColor        = new Color(0.55f, 0.55f, 0.55f, 0.55f);
    }

    private Vector3 WorldPosST(MapNodeData node)
    {
        float x = node.normalizedX * mapWidthUnits - mapWidthUnits * 0.5f;
        float y = node.layerIndex  * layerSpacingUnits;
        return new Vector3(x, y, 0f);
    }

    private static Sprite CreateCircleSpriteST(int res)
    {
        Texture2D tex  = new Texture2D(res, res, TextureFormat.RGBA32, false);
        float center   = res * 0.5f;
        float radiusSq = (center - 1f) * (center - 1f);
        Color[] pixels = new Color[res * res];
        for (int y = 0; y < res; y++)
            for (int x = 0; x < res; x++)
            {
                float dx = x - center, dy = y - center;
                pixels[y * res + x] = (dx * dx + dy * dy) <= radiusSq ? Color.white : Color.clear;
            }
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), res);
    }
}
