using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.UI
{
    public class CompassElement : MonoBehaviour
    {
        [Tooltip("The marker on the compass for this element")]
        public CompassMarker CompassMarkerPrefab;

        [Tooltip("Text override for the marker, if it's a direction")]
        public string TextDirection;

        [Tooltip("Objective that must be completed before this marker is shown")]
        public Objective UnlockAfterObjective;

        Compass m_Compass;
        CompassMarker m_MarkerInstance;

        void Awake()
        {
            m_Compass = FindFirstObjectByType<Compass>();
            DebugUtility.HandleErrorIfNullFindObject<Compass, CompassElement>(m_Compass, this);

            var markerInstance = Instantiate(CompassMarkerPrefab);
            m_MarkerInstance = markerInstance;

            markerInstance.Initialize(this, TextDirection);
            m_Compass.RegisterCompassElement(transform, markerInstance);

            bool shouldShowMarker = UnlockAfterObjective == null || UnlockAfterObjective.IsCompleted;
            markerInstance.gameObject.SetActive(shouldShowMarker);
            if (!shouldShowMarker)
                Objective.OnObjectiveCompleted += OnObjectiveCompleted;
        }

        void OnObjectiveCompleted(Objective objective)
        {
            if (objective != UnlockAfterObjective)
                return;

            if (m_MarkerInstance != null)
                m_MarkerInstance.gameObject.SetActive(true);

            Objective.OnObjectiveCompleted -= OnObjectiveCompleted;
        }

        void OnDestroy()
        {
            Objective.OnObjectiveCompleted -= OnObjectiveCompleted;
            m_Compass.UnregisterCompassElement(transform);
        }
    }
}
