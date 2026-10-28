using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ObjectivePickupItem : Objective
    {
        [Tooltip("Item to pickup to complete the objective")]
        public GameObject ItemToPickup;

        void Awake()
        {
            if (UnlockAfterObjective != null && ItemToPickup != null &&
                ItemToPickup.TryGetComponent(out Pickup pickup))
            {
                pickup.UnlockAfterObjective = UnlockAfterObjective;
            }
        }

        protected override void Start()
        {
            base.Start();

            EventManager.AddListener<PickupEvent>(OnPickupEvent);
        }

        void OnPickupEvent(PickupEvent evt)
        {
            if (IsCompleted || ItemToPickup != evt.Pickup)
                return;

            // this will trigger the objective completion
            // it works even if the player can't pickup the item (i.e. objective pickup healthpack while at full heath)
            CompleteObjective(string.Empty, string.Empty, "Objective complete : " + Title);

            if (gameObject)
            {
                Destroy(gameObject);
            }
        }

        protected override void OnDestroy()
        {
            EventManager.RemoveListener<PickupEvent>(OnPickupEvent);
            base.OnDestroy();
        }
    }
}
