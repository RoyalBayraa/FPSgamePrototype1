using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Movement : MonoBehaviour
{
    [SerializeField] private float _maxSpeed = 2f; // actually i set it 120 in properties :)
    [SerializeField] private float _groundAccelration = 30f;
    [SerializeField] private float _groundingForce = -.05f;
    [SerializeField] private float _fallAcceleration = 110f;
    [SerializeField] private float _fall_MaxSpeed = 40f;
    [SerializeField] private float _grounderDistance = 0.05f;
    [SerializeField] private float _groundDeceleration = 60;
    [SerializeField] private float _airDeceleration = 30;
    [SerializeField] private float _jumpPower = 36f;
    [SerializeField] private float _jumpEndEarlyGravityModifier = 3f;
    [SerializeField] private float _coyoteTime = .15f;
    [SerializeField] private float _Jumpbuffer = .2f;
    [SerializeField] private float _maxJumpTime = 0.5f;


    [SerializeField] private LayerMask _playerLayer;

    [SerializeField] private Transform _cameraTransform;

    [SerializeField] private float _sensitivity = 2f;

    private Rigidbody _rb;
    private CapsuleCollider _col;
    private Vector2 _moveInput;
    private Vector3 _frameVelocity;

    private Vector3 _move;
    private Vector3 _wishDir;

    private float _time;
    private float _timeJumpWasPressed;

    private float _frameLeftGrounded = float.MinValue;

    private bool _grounded;
    private bool _movePressed;// should useful animation check
    private bool _jumpPressed;
    private bool _jumpPressStarted;
    private bool _jumpToConsume;
    private bool _isJumping;

    InputSystem_Actions input;

    private void Awake()
    {
        GatherInputDataAwake();
    }

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<CapsuleCollider>();
    }

    private void Update()
    {
        _time += Time.deltaTime;

        CheckInputData();
    }



    void FixedUpdate()
    {
        MovebyCam();

        HandleJump();
        CheckCollisions();
        HandleHorizontalDirection();
        HandleGravity();

        Applymovement();
    }

    private void MovebyCam()
    {
        Vector3 camForward = _cameraTransform.forward;
        Vector3 camRight = _cameraTransform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        _move = (camRight * _moveInput.x + camForward * _moveInput.y).normalized;

    }




    private void HandleHorizontalDirection()
    {
        //OldTempHandleHorizontal();
        NewTempHandleHorizontal();
        float velocityMagnitude = _rb.linearVelocity.magnitude;// velocity check.

    }

    private void NewTempHandleHorizontal()
    {
        Vector3 flatVelocity = new Vector3(_rb.linearVelocity.x, _frameVelocity.y, _rb.linearVelocity.z); //ignore y vel for ground move but will need to check TODO: check for jump
        float wishSpeed = _maxSpeed;



        _wishDir = new Vector3(_move.x, 0, _move.z).normalized;

        if (_moveInput.magnitude > 0)
        {
            _frameVelocity = Accelrate(flatVelocity, _wishDir, wishSpeed, _groundAccelration, Time.fixedDeltaTime);

            if (_frameVelocity.magnitude > _maxSpeed)
            {
                _frameVelocity = _frameVelocity.normalized * _maxSpeed;
            }
        }
        else
        {
            var decelration = _grounded ? _groundDeceleration : _airDeceleration;
            _frameVelocity *= 1f - decelration * Time.fixedDeltaTime;
        }
    }

    private Vector3 Accelrate(Vector3 velocity, Vector3 wishDir, float wishSpeed, float accel, float deltaTime)
    {
        float currentSpeed = Vector3.Dot(velocity, wishDir);
        float addSpeed = wishSpeed - currentSpeed;

        if (addSpeed <= 0f)
        {
            return velocity;
        }

        float accelSpeed = accel * deltaTime * wishSpeed;
        if (accelSpeed > addSpeed)
            accelSpeed = addSpeed;

        return velocity + wishDir * accelSpeed;
    }

    private void OldTempHandleHorizontal()
    {
        Vector3 targetVel = _wishDir * _maxSpeed;

        float accel = _groundAccelration;
        float dot = Vector3.Dot(_frameVelocity.normalized, _wishDir);

        if (dot < 0f)
        {
            //_frameVelocity = targetVel;
            accel *= 4f;
        }

        if (_wishDir == Vector3.zero)
        {
            var decelration = _grounded ? _groundDeceleration : _airDeceleration; //handle decelration 
            _frameVelocity = Vector3.MoveTowards(_frameVelocity, _wishDir * 0f, decelration * Time.fixedDeltaTime);

        }
        else
        {
            _frameVelocity = Vector3.MoveTowards(_frameVelocity, targetVel, _groundAccelration * Time.fixedDeltaTime);

            //TODO: i need to change movetoward it using ease the move so. i need instant change of velocity? maybe ah....
        }
    }

    private void HandleGravity()
    {
        if (_grounded)
        {
            _frameVelocity.y = _groundingForce;
        }
        else if (_isJumping && (_time - _frameLeftGrounded) < JumpTimeApex_Get())
        {
            _frameVelocity.y = _jumpPower;
        }
        else
        {
            //idk
            _frameVelocity.y = -_fall_MaxSpeed;
        }
    }
    private bool _bufferedJumpUsable;
    private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + _Jumpbuffer;
    private void HandleJump()
    {
        //if (!_jumpToConsume && !HasBufferedJump) return;
        if (!_jumpToConsume && !HasBufferedJump) return;

        //if (_grounded || CanUseCoyote) ExecuteJump();

        //_jumpToConsume = false;
        if (_grounded) ExecuteJump();

        _jumpToConsume = false;
    }

    private void ExecuteJump()
    {
        _timeJumpWasPressed = 0f;
        _bufferedJumpUsable = false;
        _isJumping = true;
    }


    private void CheckCollisions()
    {
        Vector3 point1 = _col.bounds.center + Vector3.up * (_col.height / 2f - _col.radius);

        Vector3 origin = _col.bounds.center + Vector3.down * (_col.height / 2f - _col.radius);

        Vector3 point2 = _col.bounds.center + Vector3.down * (_col.height / 2f - _col.radius);
        RaycastHit hit;
        bool groundHit = Physics.SphereCast(_col.bounds.center, _col.radius, Vector3.down, out hit, _grounderDistance, ~_playerLayer);
        bool ceilingHit = Physics.SphereCast(_col.bounds.center, _col.radius, Vector3.down, out hit, _grounderDistance, ~_playerLayer);

        //landed on ground    
        if (!_grounded && groundHit)
        {
            _grounded = true;
            //_coyoteUseable = true;
            _bufferedJumpUsable = true;
        }
        //Left the ground
        else if (_grounded && !groundHit)
        {
            _grounded = false;
            _frameLeftGrounded = _time;
        }
    }


    private void Applymovement()
    {
        _rb.linearVelocity = _frameVelocity;
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

        input.Player.Jump.started += ctx => _jumpPressStarted = ctx.ReadValueAsButton();
        input.Player.Jump.performed += ctx => _jumpPressed = ctx.ReadValueAsButton();
        input.Player.Jump.canceled += ctx => _jumpPressed = ctx.ReadValueAsButton();
    }

    private void CheckInputData()
    {
        if (_jumpPressStarted)
        {
            _timeJumpWasPressed = _time;
            _jumpPressStarted = false;
        }

        if (_jumpPressed)
        {
            _jumpToConsume = true;
        }

    }

    private float JumpTimeApex_Get()
    {
        float timeToApex = _maxJumpTime / 2;
        return timeToApex;
    }


    private void OnEnable()
    {
        input.Player.Enable();
    }

    private void OnDisable()
    {
        input.Player.Disable();
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        if (_col == null) return;

        float radius = _col.radius;
        float castDistance = _grounderDistance;

        Gizmos.DrawWireSphere(_col.bounds.center - new Vector3(0, castDistance, 0), radius);


    }
}




