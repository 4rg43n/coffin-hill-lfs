using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private LayerMask collisionLayer;
    [SerializeField] private LayerMask npcLayer;
    private const float TileSize = 1f;

    private Rigidbody2D _rb;
    private InputAction _moveAction;
    private InputAction _confirmAction;
    private Vector2 _moveDir;

    public Vector2Int FacingDirection { get; private set; } = Vector2Int.down;
    public Vector2Int GridPosition => Vector2Int.RoundToInt((Vector2)transform.position);

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.gravityScale = 0f;
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Start()
    {
        InputManager im = GameManager.GetInstanceST()?.InputManager;
        if (im == null) return;
        _moveAction    = im.OnMoveST();
        _confirmAction = im.OnConfirmST();
        im.EnablePlayerMapST();
    }

    private void Update()
    {
        if (DialogueManager.GetInstanceST() != null && DialogueManager.GetInstanceST().IsShowing)
        {
            _moveDir = Vector2.zero;
            return;
        }

        // Read input and resolve 4-directional facing each frame
        Vector2 input = _moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
        if (input != Vector2.zero)
        {
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
                _moveDir = input.x > 0 ? Vector2.right : Vector2.left;
            else
                _moveDir = input.y > 0 ? Vector2.up : Vector2.down;

            FacingDirection = Vector2Int.RoundToInt(_moveDir);
        }
        else
        {
            _moveDir = Vector2.zero;
        }

        HandleInteract();
    }

    private void FixedUpdate()
    {
        if (_moveDir == Vector2.zero) return;

        Vector2 newPos = _rb.position + _moveDir * moveSpeed * Time.fixedDeltaTime;
        _rb.MovePosition(newPos);
    }

    private void HandleInteract()
    {
        if (_confirmAction == null || !_confirmAction.WasPressedThisFrame()) return;

        Vector2 interactPos = (Vector2)transform.position + (Vector2)FacingDirection * TileSize;
        Collider2D hit = Physics2D.OverlapBox(interactPos, Vector2.one * 0.5f, 0f, npcLayer);
        if (hit != null)
        {
            INPCInteractable npc = hit.GetComponent<INPCInteractable>();
            npc?.InteractST();
        }
    }
}
