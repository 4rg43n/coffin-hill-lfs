using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private InputActionMap _playerMap;
    private InputAction _moveAction;
    private InputAction _confirmAction;

    private float _prevPinchDistance = -1f;

    private void Awake()
    {
        BuildProgrammaticActions();
    }

    private void BuildProgrammaticActions()
    {
        var asset = ScriptableObject.CreateInstance<InputActionAsset>();
        _playerMap = asset.AddActionMap("Player");

        _moveAction = _playerMap.AddAction("Move", InputActionType.Value);
        _moveAction.AddCompositeBinding("2DVector")
            .With("Up",    "<Keyboard>/w")
            .With("Up",    "<Keyboard>/upArrow")
            .With("Down",  "<Keyboard>/s")
            .With("Down",  "<Keyboard>/downArrow")
            .With("Left",  "<Keyboard>/a")
            .With("Left",  "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/d")
            .With("Right", "<Keyboard>/rightArrow");

        _confirmAction = _playerMap.AddAction("Confirm", InputActionType.Button);
        _confirmAction.AddBinding("<Mouse>/leftButton");
        _confirmAction.AddBinding("<Touchscreen>/primaryTouch/tap");
        _confirmAction.AddBinding("<Keyboard>/z");
        _confirmAction.AddBinding("<Keyboard>/space");
        _confirmAction.AddBinding("<Keyboard>/return");

        EnablePlayerMapST();
    }

    public void EnablePlayerMapST()  => _playerMap?.Enable();
    public void DisablePlayerMapST() => _playerMap?.Disable();
    public InputAction OnConfirmST() => _confirmAction;
    public InputAction OnMoveST()    => _moveAction;

    /// <summary>
    /// Returns a zoom delta this frame. Positive = zoom in, negative = zoom out.
    /// Scale: ~1.0 per mouse-wheel notch, or proportional to pinch distance / screen height.
    /// Caller applies their own zoom speed multiplier.
    /// </summary>
    public float GetZoomDeltaST()
    {
        // Pinch gesture (Android / multi-touch) — takes priority over mouse wheel
        if (Touchscreen.current != null)
        {
            var touches = Touchscreen.current.touches;
            bool t0Active = touches.Count > 0 && touches[0].press.isPressed;
            bool t1Active = touches.Count > 1 && touches[1].press.isPressed;

            if (t0Active && t1Active)
            {
                float dist = Vector2.Distance(
                    touches[0].position.ReadValue(),
                    touches[1].position.ReadValue());

                float delta = _prevPinchDistance > 0f
                    ? (dist - _prevPinchDistance) / Screen.height
                    : 0f;

                _prevPinchDistance = dist;
                return delta;
            }
        }

        _prevPinchDistance = -1f;

        // Mouse wheel (PC)
        if (Mouse.current != null)
        {
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (scroll != 0f)
                return scroll / 120f;   // normalise: 1.0 per wheel notch
        }

        return 0f;
    }

    /// <summary>Returns true when two fingers are actively pinching (zoom gesture on Android).</summary>
    public bool IsPinchingZoomST()
    {
        if (Touchscreen.current == null) return false;
        var touches = Touchscreen.current.touches;
        return touches.Count > 1
            && touches[0].press.isPressed
            && touches[1].press.isPressed;
    }

    public Vector2 GetPointerPositionST()
    {
        Vector2 screenPos = Vector2.zero;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
        else if (Mouse.current != null)
            screenPos = Mouse.current.position.ReadValue();

        Camera cam = Camera.main;
        if (cam != null)
            return cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, cam.nearClipPlane));

        return screenPos;
    }
}
