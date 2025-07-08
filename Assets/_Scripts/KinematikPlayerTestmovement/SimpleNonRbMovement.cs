using UnityEngine;

public class SimpleNonRbMovement : MonoBehaviour
{
    public float wishSpeed = 7.62f; //300 units/sec (Source) = 300 inches/sec = 300 × 0.0254 meters/sec 7.62 meters/sec(Unity)
    public float acceleration = 0.254f;
    public float jumpForce = 6f;
    public float gravity = -20f;
    public float maxSpeed = 36f;


    public float groundCheckDistance = 0.1f;
    public float groundedDistance = 0.1f;
    public LayerMask groundLayer;

    private Vector3 _movePositionTarget;
    private Vector3 _transientPosition;
    [SerializeField] private Vector3 _velocity;
    private bool _movePosisionRawOrDirty;

    private bool _isGrounded;
    private float _verticalVelocity;


    //input section
    private InputSystem_Actions input;
    private Vector2 _moveInput;
    private Vector3 wishDir;
    private bool _movePressed;
    private bool _jumpPressed;

    private CapsuleCollider _capsuleCollider;

    private void Awake()
    {
        GatherInputDataAwake();
        _capsuleCollider = GetComponent<CapsuleCollider>();
    }

    private void Start()
    {
        _transientPosition = transform.position;
        _movePositionTarget = _transientPosition;
    }




    private void Update()
    {
        CastChecks();

        GroundCheck();


        // GravityCheck();

        //combine horizontal and vertical motion;
        Combine_Vert_N_HoriMovement();


        Move();


    }


    private void CastChecks()
    {
        float offsetpos = (_capsuleCollider.height / 2) - _capsuleCollider.radius;
        // _isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance + 0.1f, groundLayer);

        Vector3 origin = transform.position + new Vector3(0, offsetpos, 0);
        bool hasHit = Physics.SphereCast(origin, 0.4f, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayer);
        _isGrounded = hasHit && hit.distance <= groundedDistance;


        if (_isGrounded)
        {
            Debug.DrawLine(origin, hit.point, Color.green);
        }
        else
        {
            Debug.DrawRay(origin, Vector3.down * 2f, Color.red);
        }

        Vector3 hitNormal = hit.normal;
    }




    private void Combine_Vert_N_HoriMovement()
    {


        wishDir = new Vector3(_moveInput.x, 0f, _moveInput.y).normalized;

        Vector3 wishVelocity;

        wishVelocity = wishDir;

        //Vector3 wishDirNorm = wishDir.normalized;

        //wishSpeed = wishDirNorm.magnitude;

        //if (wishSpeed > maxSpeed)
        //{
        //    ClampVectorMagnitude(wishVelocity, maxSpeed);
        //}

        if (_isGrounded)
        {
            if (_jumpPressed)
            {
                _verticalVelocity = jumpForce;
            }

            Accelerate();
        }
        else
        {
            _verticalVelocity += gravity * Time.deltaTime;


            AirAccelerate(_velocity);
        }

        //i dont know how to implement the gravity so 
        _velocity = new Vector3(_velocity.x, _verticalVelocity, _velocity.z);
    }

    private void Accelerate()
    {
        float addSpeed, accelSpeed, currentSpeed;

        currentSpeed = Vector3.Dot(_velocity, wishDir);

        addSpeed = wishSpeed - currentSpeed;

        if (addSpeed <= 0)
        {
            return;
        }

        accelSpeed = acceleration * Time.deltaTime * wishSpeed;

        if (accelSpeed > addSpeed)
            accelSpeed = addSpeed;



        _velocity = new Vector3(wishDir.x, 0f, wishDir.z) * accelSpeed;
    }


    private void AirAccelerate(Vector3 wishVelocity)
    {
        float addSpeed, wishSpeed2, accelSpeed, currentSpeed;

        wishVelocity.Normalize();
        wishSpeed2 = wishVelocity.magnitude;

        if (wishSpeed2 > 30)
            wishSpeed2 = 30;


        currentSpeed = Vector3.Dot(_velocity, wishVelocity);

        addSpeed = wishSpeed2 - currentSpeed;

        if (addSpeed <= 0)
        {
            return;
        }

        accelSpeed = acceleration * Time.deltaTime * wishSpeed;

        if (accelSpeed > addSpeed)
            accelSpeed = addSpeed;



        _velocity = new Vector3(wishDir.x, 0f, wishDir.z) * accelSpeed;
    }

    private void Move()
    {
        //apply movement target
        _movePositionTarget = _transientPosition + _velocity * Time.deltaTime;
        _movePosisionRawOrDirty = true;

        if (_movePosisionRawOrDirty)
        {
            Vector3 tmpVelocity = (_movePositionTarget - _transientPosition) / Time.deltaTime;

            //update position manually
            _transientPosition += tmpVelocity * Time.deltaTime;
            transform.position = _transientPosition;

            _movePosisionRawOrDirty = false;
        }
    }

    private void GravityCheck()
    {

    }

    private void GroundCheck()
    {
        //Stop falling if grounded
        if (_isGrounded && _verticalVelocity <= 0f)
        {
            _verticalVelocity = 0f;
        }
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
    }


    public static Vector3 ClampVectorMagnitude(Vector3 vector, float maxMagnitude)
    {
        float mag = vector.magnitude;
        if (mag > maxMagnitude)
        {
            return vector * (maxMagnitude / mag);
        }
        return vector;
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
        if (_capsuleCollider != null)
        {
            Gizmos.color = Color.yellow;
            float offset = (_capsuleCollider.height / 2) - _capsuleCollider.radius;
            Vector3 pos = transform.position + new Vector3(0, offset, 0);
            Vector3 endpos = pos - new Vector3(0, groundedDistance, 0);

            Gizmos.DrawLine(pos, endpos);
            Gizmos.DrawWireSphere(endpos, 0.4f);
        }




    }
}
