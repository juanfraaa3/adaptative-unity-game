using UnityEngine;

public class PlatformLandingTrigger : MonoBehaviour
{
    PlatformIdentifier id;

    void Awake()
    {
        id = GetComponent<PlatformIdentifier>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlatformManager.Instance.RegisterLanding(id.PlatformIndex);

            // Si quieres depurar:
            // Debug.Log("Landed on platform " + id.PlatformIndex);
        }
    }
}
