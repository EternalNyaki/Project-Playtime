using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerController : NetworkBehaviour
{
    public enum FacingDirection
    {
        Left = -1, Right = 1
    }

    [System.Flags]
    public enum InputFlags : byte
    {
        None = 0,
        Up = 0b_1000_0000,
        Down = 0b_0100_0000,
        Left = 0b_0010_0000,
        Right = 0b_0001_0000,
        Jump = 0b_0000_1000,
        Misc1 = 0b_0000_0100,
        Misc2 = 0b_0000_0010,
        Misc3 = 0b_0000_0001
    }

    public PlatformerControllerParams movementParams;

    private InputFlags m_inputFlags;
    private InputFlags m_prevInputs;

    private FacingDirection m_facingDirection = FacingDirection.Right;

    //Variables for storing player input
    private Vector2 m_playerInput;
    private bool m_jumpTrigger, m_jumpReleaseTrigger;

    private float m_timeSinceLastGrounded;

    private Rigidbody2D m_rigidbody;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();

        if (!NetworkObject.IsSpawned && NetworkManager.Singleton.IsHost)
        {
            NetworkObject.Spawn();
            NetworkObject.ChangeOwnership(OwnerClientId);
        }
    }

    // Update is called once per frame
    void Update()
    {
        GetInputs();
    }

    public void SetInputs(InputFlags inputFlags)
    {
        m_inputFlags = inputFlags;
    }

    private void GetInputs()
    {
        if (!IsOwner) { return; }

        //Get directional input
        m_playerInput = Vector2.zero;
        if ((m_inputFlags & InputFlags.Left) != InputFlags.None)
        {
            m_playerInput.x += -1;
        }
        if ((m_inputFlags & InputFlags.Right) != InputFlags.None)
        {
            m_playerInput.x += 1;
        }

        if ((m_inputFlags & InputFlags.Down) != InputFlags.None)
        {
            m_playerInput.y += -1;
        }
        if ((m_inputFlags & InputFlags.Up) != InputFlags.None)
        {
            m_playerInput.y += 1;
        }

        if ((m_inputFlags & InputFlags.Jump) != InputFlags.None &&
            (m_prevInputs & InputFlags.Jump) == InputFlags.None)
        {
            m_jumpTrigger = true;
        }
        else if ((m_inputFlags & InputFlags.Jump) == InputFlags.None &&
                 (m_prevInputs & InputFlags.Jump) != InputFlags.None)
        {
            m_jumpReleaseTrigger = true;
        }

        m_prevInputs = m_inputFlags;
    }

    void FixedUpdate()
    {
        if (!IsOwner) { return; }

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
        Vector2 groundCheckCenter = (Vector2)transform.position + new Vector2(movementParams.groundCheckRect.position.x * (int)m_facingDirection, movementParams.groundCheckRect.position.y);
        return Physics2D.OverlapBox(groundCheckCenter, movementParams.groundCheckRect.size, 0f, movementParams.groundMask);
    }

    //Returns the direction the player is facing
    public FacingDirection GetFacingDirection()
    {
        return m_facingDirection;
    }

    public Vector2 GetVelocity()
    {
        return m_rigidbody.linearVelocity;
    }

#if UNITY_EDITOR
    //EDITOR LOGIC
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Rect groundCheckRect = new Rect(movementParams.groundCheckRect);
        Vector2 groundCheckCenter = (Vector2)transform.position + new Vector2(groundCheckRect.position.x * (int)m_facingDirection, groundCheckRect.position.y);
        Gizmos.DrawWireCube(groundCheckCenter, groundCheckRect.size);
    }
#endif
}
