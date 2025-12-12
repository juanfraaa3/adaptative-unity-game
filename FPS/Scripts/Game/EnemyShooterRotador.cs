using UnityEngine;
using Unity.FPS.Game; // Importa el namespace del WeaponController

public class EnemyShooterRotador : MonoBehaviour
{
    [Header("Target")]
    public string playerTag = "Player";
    public float shootRange = 25f;

    [Header("Weapon")]
    [SerializeField] private WeaponController weapon; // arrástralo en el Inspector (Weapon_EyeLazers)

    private Transform player;

    void Start()
    {
        if (!weapon)
        {
            Debug.LogError("⚠️ EnemyShooterRotador: No se asignó WeaponController en " + gameObject.name);
        }

        var p = GameObject.FindGameObjectWithTag(playerTag);
        if (p) player = p.transform;
    }

    void Update()
    {
        if (!player || weapon == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= shootRange)
        {
            if (weapon.WeaponMuzzle != null)
            {
                // Apuntar al centro del CharacterController (se adapta al crouch)
                Vector3 target = player.position;
                var cc = player.GetComponent<CharacterController>();
                if (cc != null)
                {
                    target = cc.bounds.center;
                }

                weapon.WeaponMuzzle.LookAt(target);
            }

            // Simular mantener presionado el gatillo
            weapon.HandleShootInputs(false, true, false);
        }
        else
        {
            weapon.HandleShootInputs(false, false, false);
        }
    }

}
