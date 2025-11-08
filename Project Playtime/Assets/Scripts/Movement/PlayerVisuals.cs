using UnityEngine;
using Unity.Netcode;

using FacingDirection = PlayerController.FacingDirection;

[RequireComponent(typeof(PlayerController))]
public class PlayerVisuals : MonoBehaviour
{
    private PlayerController m_playerController;
    private SpriteRenderer m_spriteRenderer;
    private Animator m_animator;

    private int m_velocityXHash, m_velocityYHash, m_groundedHash;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_playerController = GetComponent<PlayerController>();
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_animator = GetComponent<Animator>();

        m_velocityXHash = Animator.StringToHash("velocityX");
        m_velocityYHash = Animator.StringToHash("velocityY");
        m_groundedHash = Animator.StringToHash("grounded");
    }

    // Update is called once per frame
    void Update()
    {
        AnimUpdate();
    }

    private void AnimUpdate()
    {
        AnimUpdateRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void AnimUpdateRpc()
    {
        m_animator.SetFloat(m_velocityXHash, Mathf.Abs(m_playerController.GetVelocity().x));
        m_animator.SetFloat(m_velocityYHash, m_playerController.GetVelocity().y);
        m_animator.SetBool(m_groundedHash, m_playerController.IsGrounded());

        //Flip the player sprite based on facing direction
        switch (GetFacingDirectionRpc())
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

    [Rpc(SendTo.Owner)]
    private FacingDirection GetFacingDirectionRpc()
    {
        return m_playerController.GetFacingDirection();
    }
}
