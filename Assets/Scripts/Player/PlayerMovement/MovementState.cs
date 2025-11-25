using UnityEngine;

public class MovementState : PlayerMovementState
{
    GameObject Entity;
    private Rigidbody _rigidbody;
    private ParticleSystem _particleSystem;
    private Animator _animator;
    private Transform _entityTransform;  // Cache transform reference
    private bool _isGrounded;
    private bool _wasGrounded;
    private bool _canJump;
    
    private float _CurrentVelocity;
    private float _MoveSpeed;
    private float _JumpForce;
    private Joystick _Input = Constant.JOY_LEFT;

    private bool _isSneaking;
    
    private float _groundCheckTimer;
    private const float GROUND_CHECK_INTERVAL = 0.08f;
    private const float GROUND_CHECK_DISTANCE = 0.7f;
    
    private const float AIRBORNE_MULTIPLIER = 0.7f;
    private const float SPEED_LERP_RATE = 5f;
    private const float SNEAK_MULTIPLIER = 0.3f;

    private const float FALL_MULTIPLIER = 2.5f;
    private const float LOW_JUMP_MULTIPLIER = 2f;

    private Vector3? FollowVector = null;

    /*
     * ========================================================================
     * Initialization
     * ========================================================================
     */
    public override void EnterState(PlayerMovementManager manager, MovementConfig config)
    {
        Entity = config.Entity;
        _entityTransform = Entity.transform;
        _MoveSpeed = config.MoveSpeed;
        _JumpForce = config.JumpForce;
        _canJump = config.CanJump;
        
        _rigidbody = Entity.GetComponent<Rigidbody>();
        _particleSystem = Entity.GetComponent<ParticleSystem>();

        _animator = Entity.GetComponentInChildren<Animator>();

        _groundCheckTimer = 0f;

    }

    /* 
     * ========================================================================
     * Movement Logic
     * ========================================================================
     */
    public override void UpdateState(PlayerMovementManager manager, bool isActive, Vector3 direction)
    {
        UpdateGroundCheck();

        if (!isActive)
        {
            if (_rigidbody.linearVelocity.sqrMagnitude > 0.1f)
            {
                Vector3 currentVel = _rigidbody.linearVelocity;
                _rigidbody.linearVelocity = new Vector3(
                    Mathf.Lerp(currentVel.x, 0, Time.deltaTime * SPEED_LERP_RATE),
                    currentVel.y,
                    Mathf.Lerp(currentVel.z, 0, Time.deltaTime * SPEED_LERP_RATE)
                );
            }
            return;
        }
        // Only run when entity is active
        Vector3 moveDirection;
        float horizontalInput = Input.GetAxis(_Input.Horizontal);
        float verticalInput = Input.GetAxis(_Input.Vertical);
        
        // Lock movement to FollowVector if mouse is active and FollowVector is set
        if (manager.IsMouseActive && FollowVector.HasValue)
        {
            Vector3 followVec = FollowVector.Value.normalized;
            float magnitude;

            if (Mathf.Abs(horizontalInput) > Mathf.Abs(verticalInput)) magnitude = horizontalInput;
            else magnitude = verticalInput * Mathf.Sign(followVec.y);

            moveDirection = magnitude * new Vector3(followVec.x, 0f, followVec.z).normalized;
        }
        else
        {
            moveDirection = direction;
        }

        ProcessJumpInput();
        ProcessSneakInput();
        ApplyMovement(moveDirection, Config.SMOOTH_TIME);
    }
    
    /*
     * Check if entity is on ground.
     */
    private void UpdateGroundCheck()
    {
        _wasGrounded = _isGrounded;

        if (_groundCheckTimer <= 0f)
        {
            _isGrounded = Physics.Raycast(_entityTransform.position, Vector3.down, GROUND_CHECK_DISTANCE) ||
                          Physics.Raycast(_entityTransform.position + Vector3.forward * 0.2f, Vector3.down, GROUND_CHECK_DISTANCE) ||
                          Physics.Raycast(_entityTransform.position + Vector3.back * 0.2f, Vector3.down, GROUND_CHECK_DISTANCE) ||
                          Physics.Raycast(_entityTransform.position + Vector3.right * 0.2f, Vector3.down, GROUND_CHECK_DISTANCE) ||
                          Physics.Raycast(_entityTransform.position + Vector3.left * 0.2f, Vector3.down, GROUND_CHECK_DISTANCE);        }
        _groundCheckTimer -= Time.deltaTime;

        if (_animator != null)
        {
            if (_isGrounded)
            {
                _animator.SetBool("isFalling", false);
            }
            else
            {
                bool isFalling = !_isGrounded && _rigidbody.linearVelocity.y < -2f;
                _animator.SetBool("isFalling", isFalling);
            }
        }

        if (_isGrounded && !_wasGrounded)
        {
            OnLanded();
        }
    }

    /*
     * Jump Input Handling
     */
    private void ProcessJumpInput()
    {
        if (!_canJump || !_isGrounded || _rigidbody.linearVelocity.y > 2f)
            return;

        if (Input.GetButtonDown("MouseJump"))
        {
            if (_animator != null)
            {
                _animator.SetTrigger("Jump");
                _animator.SetBool("isFalling", false);
                _rigidbody.AddForce(Vector3.up * _JumpForce, ForceMode.Impulse);
            }
        }
    }
    
    /*
     * Called when entity lands on ground.
     */
    private void OnLanded()
    {
        if (_animator != null)
        {
            _animator.SetTrigger("Land");
            _animator.SetBool("isFalling", false);
        }
    }
    
    /*
     * Sneak Input Handling
     */
    private void ProcessSneakInput()
    {
        if (_canJump)
        {
            _isSneaking = Input.GetButton("MouseDash");
        }
    }
    
    /*
     *  Movement based on input direction.
     */
    private void ApplyMovement(Vector3 moveDirection, float smoothTime)
    {
        float currentMoveSpeed = _MoveSpeed;

        if (_isSneaking && _canJump)
            currentMoveSpeed *= SNEAK_MULTIPLIER;
            
        if (_canJump && !_isGrounded) 
            currentMoveSpeed *= AIRBORNE_MULTIPLIER;
        
        Vector3 currentVelocity = _rigidbody.linearVelocity;
        Vector3 horizontalVelocity = Vector3.zero;

        // increase gravity for better jump feel
        if (_rigidbody.linearVelocity.y < 0)
        {
            _rigidbody.linearVelocity += Vector3.up * Physics.gravity.y * (FALL_MULTIPLIER - 1f) * Time.deltaTime;
        }
        else if (_rigidbody.linearVelocity.y > 0 && !Input.GetButton("Jump"))
        {
            // short hop if player releases jump
            _rigidbody.linearVelocity += Vector3.up * Physics.gravity.y * (LOW_JUMP_MULTIPLIER - 1f) * Time.deltaTime;
        }
        
        if (moveDirection.sqrMagnitude > 0)
        {
            horizontalVelocity = moveDirection * currentMoveSpeed;
            
            var targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            var angle = Mathf.SmoothDampAngle(_entityTransform.eulerAngles.y, targetAngle, ref _CurrentVelocity, smoothTime);
            _entityTransform.rotation = Quaternion.Euler(0.0f, angle, 0.0f);
        }

        _rigidbody.linearVelocity = new Vector3(horizontalVelocity.x, currentVelocity.y, horizontalVelocity.z);
        
        if (_animator != null)
        {
            bool isMoving = moveDirection.sqrMagnitude > 0.1f;
            _animator.SetBool("isRunning", isMoving);
        }
    }
    
    /*
     * ========================================================================
     * Joystick
     * ========================================================================
     */
    public override void UpdateJoyStick(Joystick Input) => _Input = Input;

    /* 
     * ========================================================================
     * Locked Movement
     * ========================================================================
     */
    public override void setFollowVector(Vector3? vec) => FollowVector = vec;

    public override void Reset()
    {
        _rigidbody.linearVelocity = Vector3.zero;
        if (_animator != null) _animator.SetBool("isRunning", false);
    }
}