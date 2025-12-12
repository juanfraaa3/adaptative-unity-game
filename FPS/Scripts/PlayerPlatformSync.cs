using UnityEngine;

public class PlayerPlatformSync : MonoBehaviour
{
    public Transform platform;         // Asigna tu plataforma
    public Transform platformGhost;    // Asigna el PlatformGhost

    private CharacterController controller;
    private Vector3 lastPlatformPosition;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (platform != null)
        {
            lastPlatformPosition = platform.position;
            if (platformGhost != null)
            {
                platformGhost.position = platform.position;
            }
        }
    }

    void Update()
    {
        if (platform == null || platformGhost == null) return;

        Vector3 platformMovement = platform.position - lastPlatformPosition;

        if (IsGroundedOnPlatform())
        {
            controller.Move(platformMovement);
        }

        lastPlatformPosition = platform.position;
        platformGhost.position = platform.position;
    }

    bool IsGroundedOnPlatform()
    {
        RaycastHit hit;
        // Cast a ray down to see if player is standing on the platform
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f))
        {
            return hit.transform == platform;
        }
        return false;
    }
}
