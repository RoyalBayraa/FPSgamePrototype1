using UnityEngine;

public class InputHandler : MonoBehaviour
{

    public static InputHandler Instance;

    InputSystem_Actions input;

    private Vector2 _moveInput;
    private Vector2 _look;
    private bool _movePressed;
    private bool _jumpPressed;

    InputHandler _handler;


    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
        GatherInputDataAwake();


    }

    public (Vector2, bool) GetMoveInputData()
    {
        Vector2 input = _moveInput;
        bool jumpPressed = _jumpPressed;
        return (input, jumpPressed);
    }

    public bool GetJumpBool()
    {
        return _jumpPressed;
    }

    public Vector2 GetLook()
    {
        return _look;
    }



    private void GatherInputDataAwake()
    {
        input = new InputSystem_Actions();

        input.Player.Move.performed += ctx =>
        {
            _moveInput = ctx.ReadValue<Vector2>();
            _movePressed = _moveInput.x != 0 || _moveInput.y != 0;
        };

        input.Player.Move.canceled += ctx =>
        {
            _moveInput = Vector2.zero;
            _movePressed = false;
        };

        input.Player.Jump.performed += ctx => _jumpPressed = ctx.ReadValueAsButton();
        input.Player.Jump.canceled += ctx => _jumpPressed = ctx.ReadValueAsButton();

        input.Player.Look.performed += ctx =>
        {
            _look = ctx.ReadValue<Vector2>();
        };
    }

    private void OnEnable()
    {
        input.Player.Enable();
    }

    private void OnDisable()
    {
        input.Player.Disable();
    }
}
