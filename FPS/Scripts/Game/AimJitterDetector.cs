using UnityEngine;

public class AimJitterDetector : MonoBehaviour
{
    [Header("References")]
    public Transform CameraTransform;

    [Header("Jitter Settings")]
    public float SmoothingFactor = 0.2f;
    public float JitterThreshold = 1.5f;

    [Header("Debug Values (Read Only)")]
    public float CurrentJitter;
    public float SmoothedJitter;
    public bool IsJitteringHard;

    private Vector3 _previousForward;

    void Start()
    {
        if (CameraTransform == null)
            CameraTransform = Camera.main.transform;

        _previousForward = CameraTransform.forward;
    }

    void Update()
    {
        Vector3 currentForward = CameraTransform.forward;

        CurrentJitter = Vector3.Angle(_previousForward, currentForward);
        SmoothedJitter = Mathf.Lerp(SmoothedJitter, CurrentJitter, SmoothingFactor);
        IsJitteringHard = SmoothedJitter > JitterThreshold;

        _previousForward = currentForward;
    }
}
