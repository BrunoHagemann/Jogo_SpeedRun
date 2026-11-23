using UnityEngine;

namespace Unity.FPS.AI
{
    /// <summary>Rotates a flat enemy visual to face the active MainCamera.</summary>
    public class EnemyBillboard : MonoBehaviour
    {
        Camera m_TargetCamera;

        void OnEnable()
        {
            ResolveTargetCamera();
        }

        void LateUpdate()
        {
            if (m_TargetCamera == null || !m_TargetCamera.isActiveAndEnabled)
                ResolveTargetCamera();

            if (m_TargetCamera == null)
                return;

            // Face the player's camera on both horizontal and vertical axes.
            Vector3 direction = m_TargetCamera.transform.position - transform.position;
            if (direction.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(direction.normalized, m_TargetCamera.transform.up);
        }

        void ResolveTargetCamera()
        {
            m_TargetCamera = Camera.main;
        }
    }
}
