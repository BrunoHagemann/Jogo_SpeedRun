using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class Pickup : MonoBehaviour
    {
        [Tooltip("Frequency at which the item will move up and down")]
        public float VerticalBobFrequency = 1f;

        [Tooltip("Distance the item will move up and down")]
        public float BobbingAmount = 1f;

        [Tooltip("Rotation angle per second")] public float RotatingSpeed = 360f;

        [Tooltip("Sound played on pickup")] public AudioClip PickupSfx;
        [Tooltip("VFX spawned on pickup")] public GameObject PickupVfxPrefab;

        [Tooltip("Objective that must be completed before this pickup can be collected")]
        public Objective UnlockAfterObjective;

        public Rigidbody PickupRigidbody { get; private set; }

        Collider m_Collider;
        Vector3 m_StartPosition;
        bool m_HasPlayedFeedback;
        Renderer[] m_VisualRenderers;
        bool[] m_VisualRenderersInitiallyEnabled;
        Light[] m_VisualLights;
        bool[] m_VisualLightsInitiallyEnabled;

        protected virtual void Start()
        {
            PickupRigidbody = GetComponent<Rigidbody>();
            DebugUtility.HandleErrorIfNullGetComponent<Rigidbody, Pickup>(PickupRigidbody, this, gameObject);
            m_Collider = GetComponent<Collider>();
            DebugUtility.HandleErrorIfNullGetComponent<Collider, Pickup>(m_Collider, this, gameObject);

            // ensure the physics setup is a kinematic rigidbody trigger
            PickupRigidbody.isKinematic = true;
            m_Collider.isTrigger = true;

            // Remember start position for animation
            m_StartPosition = transform.position;

            m_VisualRenderers = GetComponentsInChildren<Renderer>(true);
            m_VisualRenderersInitiallyEnabled = new bool[m_VisualRenderers.Length];
            for (int i = 0; i < m_VisualRenderers.Length; i++)
                m_VisualRenderersInitiallyEnabled[i] = m_VisualRenderers[i].enabled;

            m_VisualLights = GetComponentsInChildren<Light>(true);
            m_VisualLightsInitiallyEnabled = new bool[m_VisualLights.Length];
            for (int i = 0; i < m_VisualLights.Length; i++)
                m_VisualLightsInitiallyEnabled[i] = m_VisualLights[i].enabled;

            if (UnlockAfterObjective != null && !UnlockAfterObjective.IsCompleted)
            {
                SetVisualsVisible(false);
                Objective.OnObjectiveCompleted += OnUnlockObjectiveCompleted;
            }
        }

        void OnUnlockObjectiveCompleted(Objective objective)
        {
            if (objective != UnlockAfterObjective)
                return;

            SetVisualsVisible(true);
            Objective.OnObjectiveCompleted -= OnUnlockObjectiveCompleted;
        }

        void SetVisualsVisible(bool visible)
        {
            for (int i = 0; i < m_VisualRenderers.Length; i++)
                m_VisualRenderers[i].enabled = visible && m_VisualRenderersInitiallyEnabled[i];

            for (int i = 0; i < m_VisualLights.Length; i++)
                m_VisualLights[i].enabled = visible && m_VisualLightsInitiallyEnabled[i];
        }

        void Update()
        {
            // Handle bobbing
            float bobbingAnimationPhase = ((Mathf.Sin(Time.time * VerticalBobFrequency) * 0.5f) + 0.5f) * BobbingAmount;
            transform.position = m_StartPosition + Vector3.up * bobbingAnimationPhase;

            // Handle rotating
            transform.Rotate(Vector3.up, RotatingSpeed * Time.deltaTime, Space.Self);
        }

        void OnTriggerEnter(Collider other)
        {
            if (UnlockAfterObjective != null && !UnlockAfterObjective.IsCompleted)
                return;

            PlayerCharacterController pickingPlayer = other.GetComponent<PlayerCharacterController>();

            if (pickingPlayer != null)
            {
                OnPicked(pickingPlayer);

                PickupEvent evt = Events.PickupEvent;
                evt.Pickup = gameObject;
                EventManager.Broadcast(evt);
            }
        }

        void OnDestroy()
        {
            Objective.OnObjectiveCompleted -= OnUnlockObjectiveCompleted;
        }

        protected virtual void OnPicked(PlayerCharacterController playerController)
        {
            PlayPickupFeedback();
        }

        public void PlayPickupFeedback()
        {
            if (m_HasPlayedFeedback)
                return;

            if (PickupSfx)
            {
                AudioUtility.CreateSFX(PickupSfx, transform.position, AudioUtility.AudioGroups.Pickup, 0f);
            }

            if (PickupVfxPrefab)
            {
                var pickupVfxInstance = Instantiate(PickupVfxPrefab, transform.position, Quaternion.identity);
            }

            m_HasPlayedFeedback = true;
        }
    }
}
