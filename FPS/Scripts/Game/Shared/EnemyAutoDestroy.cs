using UnityEngine;
using Unity.FPS.Game; // para Health

// Destruye el enemigo (o lo desactiva) cuando su Health dispare OnDie.
public class EnemyAutoDestroy : MonoBehaviour
{
    public bool deactivateInsteadOfDestroy = false;
    public float destroyDelay = 0f; // por si quieres una animación antes

    Health _health;

    void Awake()
    {
        _health = GetComponent<Health>();
        if (_health != null)
            _health.OnDie += HandleDie;
        else
            Debug.LogWarning($"{name}: EnemyAutoDestroy no encontró Health.");
    }

    void OnDestroy()
    {
        if (_health != null)
            _health.OnDie -= HandleDie;
    }

    void HandleDie()
    {
        if (deactivateInsteadOfDestroy)
            gameObject.SetActive(false);
        else
            Destroy(gameObject, destroyDelay);
    }
}
