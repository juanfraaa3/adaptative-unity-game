using UnityEngine;
using Unity.FPS.Game;

[RequireComponent(typeof(WeaponController))]
public class EnemyShooter : MonoBehaviour
{
    public string playerTag = "Player";
    public float shootRange = 25f;

    WeaponController weapon;
    Transform player;

    void Start()
    {
        weapon = GetComponent<WeaponController>();
        var p = GameObject.FindGameObjectWithTag(playerTag);
        if (p) player = p.transform;
    }

    void Update()
    {
        if (!player) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= shootRange)
        {
            // 👉 DEBUG ANTES de pedir el disparo
            //Debug.Log($"{name} → weapon={weapon.name} | ShootType={weapon.ShootType} | " +
            //$"WeaponRootActive={(weapon.WeaponRoot ? weapon.WeaponRoot.activeInHierarchy : true)} | " +
            //$"IsWeaponActive={weapon.IsWeaponActive} | Ammo={weapon.GetCurrentAmmo()} | " +
            //$"DelayBetweenShots={weapon.DelayBetweenShots} | dist={dist:0.0}");

            // Llamada real al arma (gatillo mantenido)
            bool fired = weapon.HandleShootInputs(false, true, false);

            // 👉 DEBUG DESPUÉS de pedir el disparo (devuelve true si disparó este frame)
            //Debug.Log($"{name} → HandleShootInputs() devolvió {fired}");
        }
        else
        {
            // No dispara
            weapon.HandleShootInputs(false, false, false);
        }
    }
}
