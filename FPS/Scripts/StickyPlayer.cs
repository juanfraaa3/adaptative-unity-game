using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class StickToPlatform : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 platformVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.GetComponent<MovingPlatform>() != null)
        {
            MovingPlatform platform = hit.collider.GetComponent<MovingPlatform>();
            Vector3 move = platform.GetDeltaMovement();
            if (move != Vector3.zero)
            {
                controller.Move(move);
            }
        }
    }
}
