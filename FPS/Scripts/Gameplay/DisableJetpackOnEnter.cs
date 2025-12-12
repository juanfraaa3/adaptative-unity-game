using UnityEngine;
using Unity.FPS.Gameplay;

public class DisableJetpackOnEnter : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var jp = other.GetComponent<Jetpack>();
            if (jp != null)
            {
                jp.LockJetpack(); // 👈 usamos el método público
                Debug.Log("Jetpack bloqueado al entrar en el área.");
            }
        }
    }
}
