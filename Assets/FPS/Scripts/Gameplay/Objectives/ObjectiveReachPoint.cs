using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public class ObjectiveReachPoint : Objective
    {
        [Tooltip("Visible transform that will be destroyed once the objective is completed")]
        public Transform DestroyRoot;

        Renderer m_ObjectiveIndicator;
        bool m_IsUnlocked;

        void Awake()
        {
            if (DestroyRoot == null)
                DestroyRoot = transform;

            m_ObjectiveIndicator = GetComponent<Renderer>();
        }

        protected override void Start()
        {
            base.Start();

            if (UnlockAfterObjective == null)
                return;

            m_IsUnlocked = UnlockAfterObjective.IsCompleted;
            SetObjectiveIndicatorVisible(m_IsUnlocked);
            if (m_IsUnlocked)
                return;

            Objective.OnObjectiveCompleted += OnObjectiveCompleted;
        }

        void OnObjectiveCompleted(Objective objective)
        {
            if (objective != UnlockAfterObjective)
                return;

            m_IsUnlocked = true;
            SetObjectiveIndicatorVisible(true);
            Objective.OnObjectiveCompleted -= OnObjectiveCompleted;
        }

        void SetObjectiveIndicatorVisible(bool visible)
        {
            if (m_ObjectiveIndicator != null)
                m_ObjectiveIndicator.enabled = visible;
        }

        void OnTriggerEnter(Collider other)
        {
            if (IsCompleted || (UnlockAfterObjective != null && !m_IsUnlocked))
                return;

            var player = other.GetComponent<PlayerCharacterController>();
            // test if the other collider contains a PlayerCharacterController, then complete
            if (player != null)
            {
                CompleteObjective(string.Empty, string.Empty, "Objective complete : " + Title);

                // destroy the transform, will remove the compass marker if it has one
                Destroy(DestroyRoot.gameObject);
            }
        }

        protected override void OnDestroy()
        {
            Objective.OnObjectiveCompleted -= OnObjectiveCompleted;
            base.OnDestroy();
        }
    }
}
