using UnityEngine;

using InputFlags = PlayerController.InputFlags;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerController m_controller;

    void Start()
    {
        m_controller = GetComponent<PlayerController>();
    }

    void Update()
    {
        InputFlags inputFlags = InputFlags.None;

        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) { inputFlags |= InputFlags.Up; }
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) { inputFlags |= InputFlags.Down; }
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) { inputFlags |= InputFlags.Left; }
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) { inputFlags |= InputFlags.Right; }
        if (Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.Space)) { inputFlags |= InputFlags.Jump; }

        m_controller.SetInputs(inputFlags);
    }
}
