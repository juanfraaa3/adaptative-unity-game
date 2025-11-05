using UnityEngine;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using System.Collections;

public class PlayerRespawn : MonoBehaviour
{
    Health m_Health;
    CharacterController m_Controller;

    void Start()
    {
        m_Health = GetComponent<Health>();
        m_Controller = GetComponent<CharacterController>();

        // Guardar posición inicial como primer checkpoint
        CheckpointManager.Instance.SetCheckpoint(transform.position, transform.rotation);


        if (m_Health != null)
            m_Health.OnDie += RespawnAtCheckpoint;
    }

    void Update()
    {
        // Matar al jugador si cae fuera del mapa
        if (transform.position.y < -100f && m_Health.CurrentHealth > 0)
        {
            m_Health.Kill();
        }
    }

    void RespawnAtCheckpoint()
    {
        // 🔹 Limpiar solo los pickups sueltos por enemigos (prefab Loot_Health)
        foreach (var pickup in FindObjectsOfType<HealthPickup>())
        {
            if (pickup.name.Contains("Loot_Health"))
            {
                Destroy(pickup.gameObject);
            }
        }

        if (m_Controller != null)
            m_Controller.enabled = false;

        // 🔹 Reposicionar y restaurar orientación del jugador
        transform.position = CheckpointManager.Instance.GetCheckpoint() + Vector3.up * 1f;
        transform.rotation = CheckpointManager.Instance.GetCheckpointRotation();

        // 🔹 Forzar orientación de la cámara y del controlador del jugador
        var controller = GetComponent<Unity.FPS.Gameplay.PlayerCharacterController>();
        if (controller != null)
        {
            controller.SetLookRotation(CheckpointManager.Instance.GetCheckpointRotation());
        }

        if (m_Controller != null)
            m_Controller.enabled = true;

        // 🔹 Reactivar el arma
        var weaponsManager = GetComponent<PlayerWeaponsManager>();
        if (weaponsManager != null)
        {
            // CAMBIO ÚNICO: forzar el arma del slot 0
            weaponsManager.SwitchToWeaponIndex(0, true);

            // Desactivar el apuntado (IsAiming) después de revivir
            weaponsManager.SetAiming(false);
        }

        // 🔹 Reactivar HUD si está desactivado
        GameObject hud = GameObject.Find("PlayerHUD");
        if (hud != null)
        {
            hud.SetActive(true);
        }

        // 🔹 Resetear el estado de muerte
        typeof(Health)
            .GetField("m_IsDead", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(m_Health, false);

        // 🔹 Reactivar el arma si está desactivada
        Transform weaponParent = transform.Find("Main Camera/FirstPersonSocket/WeaponParentSocket");
        if (weaponParent != null && weaponParent.childCount > 0)
        {
            Transform weapon = weaponParent.GetChild(0);
            if (weapon != null && !weapon.gameObject.activeSelf)
            {
                weapon.gameObject.SetActive(true);
            }
        }

        // 🔹 Volver a suscribirse a OnDie (por seguridad)
        m_Health.OnDie -= RespawnAtCheckpoint;
        m_Health.OnDie += RespawnAtCheckpoint;

        // 🔹 Restaurar salud
        m_Health.Heal(m_Health.MaxHealth);

        // 🔹 Resetear las animaciones de la cámara y el arma (si está en ADS)
        ResetWeaponAndCamera();

        // 🔥 Reiniciar las waves al reaparecer
        var waveManager = FindObjectOfType<WaveManager>();
        if (waveManager != null)
        {
            waveManager.ResetWaves();
        }
    }

    void ResetWeaponAndCamera()
    {
        // Si existe el 'PlayerWeaponsManager' y el 'WeaponController', reseteamos el FOV
        var weaponsManager = GetComponent<PlayerWeaponsManager>();
        if (weaponsManager != null)
        {
            weaponsManager.SetAiming(false); // Desactivamos el estado de apuntado
        }

        // Aquí puedes añadir código para resetear cualquier animación, FOV o estado visual del arma
        var playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera != null)
        {
            playerCamera.fieldOfView = 60f;  // Restaurar el FOV al valor por defecto
        }
    }
}
