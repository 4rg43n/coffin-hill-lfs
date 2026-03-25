using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class MapCameraController : MonoBehaviour
{
    public static MapCameraController GetInstanceST() => _instance;
    private static MapCameraController _instance;

    [SerializeField] private float dragThresholdPx = 12f;

    private Camera _cam;

    private bool    _pressing;
    private bool    _isDragging;
    private Vector2 _pressStartScreenPos;
    private Vector2 _lastScreenPos;

    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(this); return; }
        _instance = this;
        _cam = GetComponent<Camera>();
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    public void CenterOnPlayerST()
    {
        Transform marker = MapManager.GetInstanceST()?.PlayerMarkerTransform;
        if (marker == null) return;
        transform.position = new Vector3(marker.position.x, marker.position.y, transform.position.z);
    }

    private void Update()
    {
        if (GameManager.GetInstanceST()?.CurrentState != GameState.Overworld) return;

        Vector2 screenPos = RawScreenPosST();
        bool pressedThisFrame   = PressStartedThisFrameST();
        bool heldThisFrame      = PressHeldST();
        bool releasedThisFrame  = PressReleasedThisFrameST();

        if (pressedThisFrame)
        {
            _pressing            = true;
            _isDragging          = false;
            _pressStartScreenPos = screenPos;
            _lastScreenPos       = screenPos;
        }

        if (_pressing && heldThisFrame)
        {
            if (!_isDragging && (screenPos - _pressStartScreenPos).magnitude > dragThresholdPx)
                _isDragging = true;

            if (_isDragging)
            {
                // Keep the world point under the cursor fixed each frame
                Vector3 prev = ScreenToWorldXYST(_lastScreenPos);
                Vector3 curr = ScreenToWorldXYST(screenPos);
                transform.position += prev - curr;
            }

            _lastScreenPos = screenPos;
        }

        if (_pressing && releasedThisFrame)
        {
            if (!_isDragging)
                HandleTapST(screenPos);

            _pressing   = false;
            _isDragging = false;
        }
    }

    // -------------------------------------------------------------------------

    private void HandleTapST(Vector2 screenPos)
    {
        Vector2 worldPos  = ScreenToWorldXYST(screenPos);
        RaycastHit2D hit  = Physics2D.Raycast(worldPos, Vector2.zero);
        MapNodeView node  = hit.collider?.GetComponent<MapNodeView>();
        node?.OnTappedST();
    }

    private Vector3 ScreenToWorldXYST(Vector2 screenPos)
    {
        // Z = camera depth so ScreenToWorldPoint returns z = 0 in world space
        float depth = Mathf.Abs(_cam.transform.position.z);
        return _cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, depth));
    }

    // ---- Raw input helpers (avoid routing keyboard confirm through drag logic) ----

    private static Vector2 RawScreenPosST()
    {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            return Touchscreen.current.primaryTouch.position.ReadValue();
        if (Mouse.current != null)
            return Mouse.current.position.ReadValue();
        return Vector2.zero;
    }

    private static bool PressStartedThisFrameST()
    {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            return true;
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
    }

    private static bool PressHeldST()
    {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            return true;
        return Mouse.current != null && Mouse.current.leftButton.isPressed;
    }

    private static bool PressReleasedThisFrameST()
    {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame)
            return true;
        return Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame;
    }
}
