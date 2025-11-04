using UnityEngine;

[System.Serializable]
public struct PlatformerControllerParams
{
    //Maximum walking speed (in units per second)
    public float maxSpeed;
    //Time to reach maximum walking speed (in seconds)
    public float accelerationTime;
    //Maximum apex jump height (in units)
    public float apexHeight;
    //Time to reach maximum apex jump height (in seconds)
    public float apexTime;

    //Maximum vertical speed of the character while falling (in units/s)
    public float terminalVelocity;
    //The duration of time during which the character can still jump after leaving the ground (in seconds)
    public float coyoteTime;

    //Rect within which to check for terrain below the character, used to check if the character is grounded
    public Rect groundCheckRect;
    //Layer Mask for terrain to check if the character is grounded
    public LayerMask groundMask;

    //The character's horizontal acceleration (in units/s^2)
    //Derived from the character's maximum walking speed and time to reach maximum walking speed
    public readonly float acceleration => maxSpeed / accelerationTime;

    //The character's vertical acceleration due to gravity (in units/s^2)
    //Derived from the character's maximum apex height and time to reach maximum apex height
    public readonly float gravity => -2 * apexHeight / Mathf.Pow(apexTime, 2);

    //The initial vertical speed of the character's jump (in units/s)
    //Derived from the character's maximum apex height and time to reach maximum apex height
    public readonly float jumpVelocity => 2 * apexHeight / apexTime;

    //The minimum amount of movement for the character to be considered moving
    //Used to stop the character from vibrating endlessly instead of stopping
    public readonly float minMovementTolerance => acceleration * 0.05f;
}
