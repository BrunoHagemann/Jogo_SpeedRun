using UnityEngine;

namespace Unity.FPS.AI
{
    /// <summary>Rotates a flat enemy visual around its vertical axis to face the active camera.</summary>
    public class EnemyBillboard : MonoBehaviour
    {
        void LateUpdate()
        {
            Camera targetCamera = Camera.main;
            if (targetCamera == null)
                return;

            Vector3 direction = targetCamera.transform.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }
    }
}
