using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum FacingDirection
    {
        Left = -1, Right = 1
    }

    public PlatformerControllerParams movementParams;

    private FacingDirection m_facingDirection = FacingDirection.Right;

    //Variables for storing player input
    private Vector2 m_playerInput;
    private bool m_jumpTrigger, m_jumpReleaseTrigger;

    private float m_timeSinceLastGrounded;

    private SpriteRenderer m_spriteRenderer;
    private Animator m_animator;
    private Rigidbody2D m_rigidbody;

    private int m_velocityXHash, m_velocityYHash, m_groundedHash;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_animator = GetComponent<Animator>();
        m_rigidbody = GetComponent<Rigidbody2D>();

        m_velocityXHash = Animator.StringToHash("velocityX");
        m_velocityYHash = Animator.StringToHash("velocityY");
        m_groundedHash = Animator.StringToHash("grounded");
    }

    // Update is called once per frame
    void Update()
    {
        GetInputs();

        AnimUpdate();
    }

    private void GetInputs()
    {
        //Get directional input
        m_playerInput.x = Input.GetAxisRaw("Horizontal");
        m_playerInput.y = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.C))
        {
            m_jumpTrigger = true;
        }
        else if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.C))
        {
            m_jumpReleaseTrigger = true;
        }
    }

    private void AnimUpdate()
    {
        m_animator.SetFloat(m_velocityXHash, Mathf.Abs(m_rigidbody.linearVelocityX));
        m_animator.SetFloat(m_velocityYHash, m_rigidbody.linearVelocityY);
        m_animator.SetBool(m_groundedHash, IsGrounded());

        //Flip the player sprite based on facing direction
        switch (m_facingDirection)
        {
            case FacingDirection.Left:
                m_spriteRenderer.flipX = true;
                break;

            case FacingDirection.Right:
            default:
                m_spriteRenderer.flipX = false;
                break;
        }
    }

    void FixedUpdate()
    {
        m_timeSinceLastGrounded += Time.deltaTime;

        MovementUpdate(m_playerInput);

        //Reset input triggers
        m_jumpTrigger = false;
        m_jumpReleaseTrigger = false;
    }

    //Update movement and apply physics (called in FixedUpdate())
    private void MovementUpdate(Vector2 playerInput)
    {
        Vector2 velocity = m_rigidbody.linearVelocity;

        //Calculate movement
        HorizontalMovement(playerInput.x, ref velocity.x);
        VerticalMovement(playerInput.y, ref velocity.y);

        //Apply movement
        m_rigidbody.linearVelocity = velocity;

        ApplyGravity();
    }

    //Calculate horizontal movement (horizontal input)
    private void HorizontalMovement(float horizontalInput, ref float xVelocity)
    {
        if (horizontalInput == 0)
        {
            if (xVelocity < movementParams.minMovementTolerance && xVelocity > -movementParams.minMovementTolerance)
            {
                //Do nothing
                xVelocity = 0f;
            }
            else
            {
                //Decelerate
                xVelocity = Mathf.Clamp(-Mathf.Sign(xVelocity) * movementParams.acceleration * Time.deltaTime, -movementParams.maxSpeed, movementParams.maxSpeed);
            }
        }
        else
        {
            //Accelerate
            xVelocity = Mathf.Clamp(xVelocity + horizontalInput * movementParams.acceleration * Time.deltaTime, -movementParams.maxSpeed, movementParams.maxSpeed);

            //Change facing direction
            if (horizontalInput > 0)
            {
                m_facingDirection = FacingDirection.Right;
            }
            else if (horizontalInput < 0)
            {
                m_facingDirection = FacingDirection.Left;
            }
        }
    }

    //Calculate vertical movement (gravity and jumping)
    private void VerticalMovement(float verticalInput, ref float yVelocity)
    {
        if (m_jumpTrigger && (IsGrounded() || m_timeSinceLastGrounded < movementParams.coyoteTime))
        {
            //Jump
            Jump(ref yVelocity);
        }
        if (m_jumpReleaseTrigger && yVelocity > 0f)
        {
            //Shorten jump (for dynamic jump height)
            yVelocity /= 2;
        }
    }

    private void Jump(ref float yVelocity)
    {
        yVelocity = movementParams.jumpVelocity;
    }

    private void ApplyGravity()
    {
        //Calculate gravity
        m_rigidbody.linearVelocity = new Vector2(m_rigidbody.linearVelocityX, Mathf.Clamp(m_rigidbody.linearVelocityY + movementParams.gravity * Time.deltaTime, -movementParams.terminalVelocity, float.PositiveInfinity));
    }

    //Returns true if the player is moving horizontally
    public bool IsWalking()
    {
        return Mathf.Abs(m_rigidbody.linearVelocityX) > 0f;
    }

    //Returns true if the player is standing on ground
    public bool IsGrounded()
    {
        if (!IsOnGround())
        {
            return false;
        }
        else if (m_rigidbody.linearVelocityY > 0.01f) //This case catches the scenario where the player is jumping through a one-way platform
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    //Returns true if the player is slightly above a piece of terrain
    //Different from IsGrounded() as it doesn't check the player's velocity,
    //meaning it can return a false positive when jumping through one-way platforms
    private bool IsOnGround()
    {
        return Physics2D.OverlapBox((Vector2)transform.position + movementParams.groundCheckRect.position, movementParams.groundCheckRect.size, 0f, movementParams.groundMask);
    }

    //Returns the direction the player is facing
    public FacingDirection GetFacingDirection()
    {
        return m_facingDirection;
    }
}
