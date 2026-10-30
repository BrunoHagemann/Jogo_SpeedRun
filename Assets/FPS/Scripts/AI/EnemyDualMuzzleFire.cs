using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.AI
{
    /// <summary>
    /// Adds a second projectile from a second muzzle whenever the base weapon fires.
    /// The WeaponController remains responsible for ammo, cadence, audio and the first muzzle.
    /// </summary>
    [RequireComponent(typeof(WeaponController))]
    public class EnemyDualMuzzleFire : MonoBehaviour
    {
        public Transform SecondaryMuzzle;

        WeaponController m_Weapon;
        Vector3 m_AimTarget;
        bool m_HasAimTarget;

        void Awake()
        {
            m_Weapon = GetComponent<WeaponController>();
        }

        void OnEnable()
        {
            if (m_Weapon == null)
                m_Weapon = GetComponent<WeaponController>();

            if (m_Weapon != null)
                m_Weapon.OnShootProcessed += FireSecondaryProjectile;
        }

        void OnDisable()
        {
            if (m_Weapon != null)
                m_Weapon.OnShootProcessed -= FireSecondaryProjectile;
        }

        public void AimAt(Vector3 targetPosition)
        {
            m_AimTarget = targetPosition;
            m_HasAimTarget = true;

            if (m_Weapon != null && m_Weapon.WeaponMuzzle != null)
                AimMuzzleAt(m_Weapon.WeaponMuzzle, targetPosition);

            if (SecondaryMuzzle != null)
                AimMuzzleAt(SecondaryMuzzle, targetPosition);
        }

        static void AimMuzzleAt(Transform muzzle, Vector3 targetPosition)
        {
            Vector3 direction = targetPosition - muzzle.position;
            if (direction.sqrMagnitude > Mathf.Epsilon)
                muzzle.rotation = Quaternion.LookRotation(direction.normalized);
        }

        void FireSecondaryProjectile()
        {
            if (SecondaryMuzzle == null || m_Weapon == null || m_Weapon.ProjectilePrefab == null)
                return;

            Vector3 direction = m_HasAimTarget
                ? (m_AimTarget - SecondaryMuzzle.position).normalized
                : SecondaryMuzzle.forward;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
                direction = SecondaryMuzzle.forward;
            ProjectileBase projectile = Instantiate(m_Weapon.ProjectilePrefab, SecondaryMuzzle.position,
                Quaternion.LookRotation(direction));
            projectile.Shoot(m_Weapon);
        }
    }
}
