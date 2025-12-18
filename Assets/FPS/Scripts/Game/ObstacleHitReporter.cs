using UnityEngine;

public class ObstacleHitReporter : MonoBehaviour
{
    private bool _alreadyHit = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_alreadyHit)
            return;

        var logger = other.GetComponent<MultitaskMetricsLogger>();
        if (logger == null)
            return;

        _alreadyHit = true;
        logger.NotifyObstacleHit();
    }

    public void ResetForNewAttempt()
    {
        _alreadyHit = false;
    }
}
