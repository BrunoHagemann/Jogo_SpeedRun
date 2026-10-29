using System.Collections.Generic;
using Unity.FPS.AI;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;

namespace Unity.FPS.UI
{
    public class Compass : MonoBehaviour
    {
        public RectTransform CompasRect;
        public float VisibilityAngle = 180f;
        public float HeightDifferenceMultiplier = 2f;
        public float MinScale = 0.5f;
        public float DistanceMinScale = 50f;
        public float CompasMarginRatio = 0.8f;

        public GameObject MarkerDirectionPrefab;

        [Header("Zombie markers")]
        public CompassMarker ZombieMarkerPrefab;
        public float ZombieMarkerDelay = 15f;
        public float ZombieMarkerBlinkInterval = 0.4f;

        Transform m_PlayerTransform;
        Dictionary<Transform, CompassMarker> m_ElementsDictionnary = new Dictionary<Transform, CompassMarker>();
        HashSet<Transform> m_ZombieMarkers = new HashSet<Transform>();

        float m_HeightOffset;
        float m_ZombieMarkersRevealTime;
        float m_NextZombieMarkerRefreshTime;
        bool m_ZombieMarkersRevealed;

        void Awake()
        {
            PlayerCharacterController playerCharacterController = FindFirstObjectByType<PlayerCharacterController>();
            DebugUtility.HandleErrorIfNullFindObject<PlayerCharacterController, Compass>(playerCharacterController,
                this);
            m_PlayerTransform = playerCharacterController.transform;

            m_HeightOffset = -CompasRect.rect.height / 2;
            m_ZombieMarkersRevealTime = Time.time + Mathf.Max(0f, ZombieMarkerDelay);
            EventManager.AddListener<EnemyKillEvent>(OnEnemyKilled);
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<EnemyKillEvent>(OnEnemyKilled);
        }

        void Update()
        {
            if (!m_ZombieMarkersRevealed && Time.time >= m_ZombieMarkersRevealTime)
            {
                m_ZombieMarkersRevealed = true;
                m_NextZombieMarkerRefreshTime = 0f;
            }

            if (m_ZombieMarkersRevealed && Time.time >= m_NextZombieMarkerRefreshTime)
            {
                RefreshZombieMarkers();
                m_NextZombieMarkerRefreshTime = Time.time + 0.5f;
            }

            // this is all very WIP, and needs to be reworked
            List<Vector2> placedZombieMarkerPositions = new List<Vector2>();
            foreach (var element in m_ElementsDictionnary)
            {
                float distanceRatio = 1;
                float heightDifference = 0;
                float angle;
                bool isZombieMarker = ShouldShowZombieMarker(element.Key);
                float visibilityAngle = isZombieMarker ? 360f : VisibilityAngle;
                float widthMultiplier = CompasRect.rect.width / Mathf.Max(visibilityAngle, 1f);

                if (element.Value.IsDirection)
                {
                    angle = Vector3.SignedAngle(m_PlayerTransform.forward,
                        element.Key.transform.localPosition.normalized, Vector3.up);
                }
                else
                {
                    Vector3 targetDir = (element.Key.transform.position - m_PlayerTransform.position).normalized;
                    targetDir = Vector3.ProjectOnPlane(targetDir, Vector3.up);
                    Vector3 playerForward = Vector3.ProjectOnPlane(m_PlayerTransform.forward, Vector3.up);
                    angle = Vector3.SignedAngle(playerForward, targetDir, Vector3.up);

                    Vector3 directionVector = element.Key.transform.position - m_PlayerTransform.position;

                    heightDifference = (directionVector.y) * HeightDifferenceMultiplier;
                    heightDifference = Mathf.Clamp(heightDifference, -CompasRect.rect.height / 2 * CompasMarginRatio,
                        CompasRect.rect.height / 2 * CompasMarginRatio);

                    distanceRatio = directionVector.magnitude / DistanceMinScale;
                    distanceRatio = Mathf.Clamp01(distanceRatio);
                }

                if (angle >= -visibilityAngle / 2 && angle <= visibilityAngle / 2)
                {
                    element.Value.CanvasGroup.alpha = 1;
                    Vector2 markerPosition = new Vector2(widthMultiplier * angle,
                        heightDifference + m_HeightOffset);
                    if (isZombieMarker)
                    {
                        markerPosition = FindFreeZombieMarkerPosition(markerPosition, placedZombieMarkerPositions);
                        placedZombieMarkerPositions.Add(markerPosition);
                    }

                    element.Value.CanvasGroup.transform.localPosition = markerPosition;
                    element.Value.CanvasGroup.transform.localScale =
                        Vector3.one * Mathf.Lerp(1, MinScale, distanceRatio);
                }
                else
                {
                    element.Value.CanvasGroup.alpha = 0;
                }

                if (isZombieMarker)
                    element.Value.CanvasGroup.alpha *= IsZombieBlinkVisible() ? 1f : 0f;
            }
        }

        void RefreshZombieMarkers()
        {
            EnemyZombieMelee[] zombies = FindObjectsByType<EnemyZombieMelee>(FindObjectsSortMode.None);
            foreach (EnemyZombieMelee zombie in zombies)
            {
                if (zombie == null)
                    continue;

                Health zombieHealth = zombie.GetComponent<Health>();
                if (zombieHealth == null || zombieHealth.CurrentHealth <= 0f)
                    continue;

                if (m_ElementsDictionnary.TryGetValue(zombie.transform, out CompassMarker existingMarker))
                {
                    if (existingMarker.MainImage != null)
                        existingMarker.MainImage.color = existingMarker.AltColor;
                    m_ZombieMarkers.Add(zombie.transform);
                    continue;
                }

                if (ZombieMarkerPrefab == null)
                    continue;

                CompassMarker marker = Instantiate(ZombieMarkerPrefab);
                if (marker.MainImage != null)
                    marker.MainImage.color = marker.AltColor;
                RegisterCompassElement(zombie.transform, marker);
                m_ZombieMarkers.Add(zombie.transform);
            }

            List<Transform> staleMarkers = new List<Transform>();
            foreach (Transform zombieTransform in m_ZombieMarkers)
            {
                if (zombieTransform == null || !zombieTransform.gameObject.activeInHierarchy)
                {
                    staleMarkers.Add(zombieTransform);
                    continue;
                }

                Health zombieHealth = zombieTransform.GetComponent<Health>();
                if (zombieTransform.GetComponent<EnemyZombieMelee>() == null || zombieHealth == null ||
                    zombieHealth.CurrentHealth <= 0f)
                    staleMarkers.Add(zombieTransform);
            }

            foreach (Transform staleMarker in staleMarkers)
            {
                m_ZombieMarkers.Remove(staleMarker);
                UnregisterCompassElement(staleMarker);
            }
        }

        bool ShouldShowZombieMarker(Transform element) =>
            m_ZombieMarkersRevealed && m_ZombieMarkers.Contains(element);

        bool IsZombieBlinkVisible()
        {
            float interval = Mathf.Max(0.01f, ZombieMarkerBlinkInterval);
            return Mathf.FloorToInt(Time.time / interval) % 2 == 0;
        }

        Vector2 FindFreeZombieMarkerPosition(Vector2 desiredPosition, List<Vector2> placedPositions)
        {
            const float minimumSpacing = 14f;
            Vector2 candidate = desiredPosition;

            for (int attempt = 0; attempt <= placedPositions.Count * 2; attempt++)
            {
                bool overlaps = false;
                foreach (Vector2 placedPosition in placedPositions)
                {
                    if ((candidate - placedPosition).sqrMagnitude < minimumSpacing * minimumSpacing)
                    {
                        overlaps = true;
                        break;
                    }
                }

                if (!overlaps)
                    return candidate;

                int row = (attempt + 2) / 2;
                float direction = attempt % 2 == 0 ? 1f : -1f;
                candidate = desiredPosition + Vector2.up * (row * minimumSpacing * direction);
            }

            return candidate;
        }

        void OnEnemyKilled(EnemyKillEvent evt)
        {
            if (evt.Enemy == null)
                return;

            Transform enemyTransform = evt.Enemy.transform;
            if (m_ZombieMarkers.Remove(enemyTransform))
                UnregisterCompassElement(enemyTransform);
        }

        public void RegisterCompassElement(Transform element, CompassMarker marker)
        {
            marker.transform.SetParent(CompasRect);

            m_ElementsDictionnary.Add(element, marker);
        }

        public void UnregisterCompassElement(Transform element)
        {
            if (m_ElementsDictionnary.TryGetValue(element, out CompassMarker marker) && marker.CanvasGroup != null)
                Destroy(marker.CanvasGroup.gameObject);
            m_ElementsDictionnary.Remove(element);
        }
    }
}
