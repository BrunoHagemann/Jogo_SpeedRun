using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.AI;

namespace Unity.FPS.AI
{
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(EnemyController), typeof(Health), typeof(NavMeshAgent))]
    [RequireComponent(typeof(Actor))]
    public class EnemyZombieMelee : MonoBehaviour
    {
        [Header("Melee attack")]
        public Animator Animator;
        public Transform AttackPoint;
        public float AttackDamage = 25f;
        public float AttackCooldown = 1.25f;
        public float AttackWindup = 0.38f;
        public float MeleeRadius = 1.15f;
        public float CollisionPadding = 0.12f;
        [Range(0f, 1f)] public float AttackStopDistanceRatio = 0.75f;

        const string MoveSpeedParameter = "MoveSpeed";
        const string AttackParameter = "Attack";
        const string DeathParameter = "Death";

        EnemyController m_Enemy;
        Health m_Health;
        NavMeshAgent m_Agent;
        Actor m_Actor;
        bool m_Dead;
        bool m_AttackPending;
        float m_NextAttackTime;
        float m_DamageTime;

        void Start()
        {
            m_Enemy = GetComponent<EnemyController>();
            m_Health = GetComponent<Health>();
            m_Agent = GetComponent<NavMeshAgent>();
            m_Actor = GetComponent<Actor>();
            if (Animator == null) Animator = GetComponentInChildren<Animator>();
            if (AttackPoint == null) AttackPoint = transform;

            AlignVisualsToNavMesh();

            m_Enemy.onDetectedTarget += OnDetectedTarget;
            m_Enemy.onLostTarget += OnLostTarget;
            m_Health.OnDie += OnDie;
            m_Enemy.SetPathDestinationToClosestNode();
        }

        void AlignVisualsToNavMesh()
        {
            if (Animator == null || m_Agent == null || !m_Agent.isOnNavMesh)
                return;

            SkinnedMeshRenderer[] renderers = Animator.GetComponentsInChildren<SkinnedMeshRenderer>();
            if (renderers.Length == 0)
                return;

            float lowestPoint = float.PositiveInfinity;
            foreach (SkinnedMeshRenderer renderer in renderers)
                lowestPoint = Mathf.Min(lowestPoint, renderer.bounds.min.y);

            float navMeshHeight = m_Agent.nextPosition.y;
            float penetration = navMeshHeight - lowestPoint;
            if (penetration > 0.02f)
                Animator.transform.position += Vector3.up * (penetration + 0.02f);
        }

        void Update()
        {
            if (m_Dead || m_Enemy.DetectionModule == null)
                return;

            UpdateAttackImpact();
            GameObject target = m_Enemy.KnownDetectedTarget;
            if (target == null)
            {
                m_Agent.isStopped = false;
                m_Enemy.UpdatePathDestination();
                m_Enemy.SetNavDestination(m_Enemy.GetDestinationOnPath());
            }
            else
            {
                Vector3 targetPosition = target.transform.position;
                m_Enemy.OrientTowards(targetPosition);

                if (m_Enemy.IsSeeingTarget && m_Enemy.IsTargetInAttackRange)
                {
                    float horizontalDistance = Vector3.ProjectOnPlane(targetPosition - transform.position, Vector3.up).magnitude;
                    float targetRadius = GetTargetRadius(target);
                    CapsuleCollider zombieCapsule = GetComponentInChildren<CapsuleCollider>();
                    float bodyRadius = Mathf.Max(m_Agent.radius, zombieCapsule != null ? zombieCapsule.radius : 0f);
                    float safeStopDistance = Mathf.Max(bodyRadius + targetRadius + CollisionPadding,
                        m_Enemy.DetectionModule.AttackRange * AttackStopDistanceRatio);

                    m_Agent.stoppingDistance = safeStopDistance;
                    m_Agent.isStopped = horizontalDistance <= safeStopDistance;
                    if (horizontalDistance <= MeleeRadius + targetRadius + 0.1f && Time.time >= m_NextAttackTime)
                        BeginAttack();
                    if (!m_Agent.isStopped)
                        m_Enemy.SetNavDestination(targetPosition);
                }
                else
                {
                    m_Agent.isStopped = false;
                    m_Enemy.SetNavDestination(targetPosition);
                }
            }

            if (Animator != null)
                Animator.SetFloat(MoveSpeedParameter, m_Agent.velocity.magnitude);
        }

        void BeginAttack()
        {
            m_NextAttackTime = Time.time + AttackCooldown;
            m_DamageTime = Time.time + AttackWindup;
            m_AttackPending = true;
            if (Animator != null)
                Animator.SetTrigger(AttackParameter);
        }

        void UpdateAttackImpact()
        {
            if (!m_AttackPending || Time.time < m_DamageTime)
                return;

            m_AttackPending = false;
            Collider[] hits = Physics.OverlapSphere(AttackPoint.position, MeleeRadius, Physics.AllLayers,
                QueryTriggerInteraction.Ignore);
            foreach (Collider hit in hits)
            {
                Actor targetActor = hit.GetComponentInParent<Actor>();
                if (targetActor == null || targetActor == m_Actor || targetActor.Affiliation == m_Actor.Affiliation)
                    continue;

                Damageable damageable = hit.GetComponentInParent<Damageable>();
                if (damageable != null)
                {
                    damageable.InflictDamage(AttackDamage, false, gameObject);
                    return;
                }

                Health targetHealth = hit.GetComponentInParent<Health>();
                if (targetHealth != null)
                {
                    targetHealth.TakeDamage(AttackDamage, gameObject);
                    return;
                }
            }
        }

        float GetTargetRadius(GameObject target)
        {
            CharacterController characterController = target.GetComponentInParent<CharacterController>();
            if (characterController != null)
                return characterController.radius;

            CapsuleCollider capsule = target.GetComponentInParent<CapsuleCollider>();
            return capsule != null ? capsule.radius : 0.35f;
        }

        void OnDetectedTarget() { }
        void OnLostTarget() { }

        void OnDie()
        {
            m_Dead = true;
            m_AttackPending = false;
            if (m_Agent != null && m_Agent.isOnNavMesh)
            {
                m_Agent.isStopped = true;
                m_Agent.ResetPath();
            }
            if (Animator != null)
                Animator.SetTrigger(DeathParameter);
        }

        void OnDestroy()
        {
            if (m_Enemy != null)
            {
                m_Enemy.onDetectedTarget -= OnDetectedTarget;
                m_Enemy.onLostTarget -= OnLostTarget;
            }
            if (m_Health != null)
                m_Health.OnDie -= OnDie;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(AttackPoint != null ? AttackPoint.position : transform.position + Vector3.up,
                MeleeRadius);
        }
    }
}
