using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private InputActionMap _playerMap;
    private InputAction _moveAction;
    private InputAction _confirmAction;

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
