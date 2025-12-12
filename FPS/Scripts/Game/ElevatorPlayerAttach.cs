using UnityEngine;

public class ElevatorPlayerAttach : MonoBehaviour
{
    private Transform _player;
    private Vector3 _offset;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            _player = collision.transform;
            _offset = _player.position - transform.position;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            _player = null;
        }
    }

    private void LateUpdate()
    {
        // 🔹 Mueve al jugador sólo si está sobre la plataforma
        if (_player != null)
        {
            var playerPos = _player.position;
            playerPos.y = transform.position.y + _offset.y;
            _player.position = playerPos;
        }
    }
}
