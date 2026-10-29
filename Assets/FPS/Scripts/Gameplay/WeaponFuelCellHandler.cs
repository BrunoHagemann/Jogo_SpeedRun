using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    [RequireComponent(typeof(WeaponController))]
    public class WeaponFuelCellHandler : MonoBehaviour
    {
        [Tooltip("Retract All Fuel Cells Simultaneously")]
        public bool SimultaneousFuelCellsUsage = false;

        [Tooltip("List of GameObjects representing the fuel cells on the weapon")]
        public GameObject[] FuelCells;

        [Tooltip("Cell local position when used")]
        public Vector3 FuelCellUsedPosition;

        [Tooltip("Cell local position before use")]
        public Vector3 FuelCellUnusedPosition = new Vector3(0f, -0.1f, 0f);

        WeaponController m_Weapon;
        bool[] m_FuelCellsCooled;

        void Start()
        {
            m_Weapon = GetComponent<WeaponController>();
            DebugUtility.HandleErrorIfNullGetComponent<WeaponController, WeaponFuelCellHandler>(m_Weapon, this,
                gameObject);

            // Custom weapon models may not contain animated fuel-cell meshes.
            // Remove unassigned entries so pickups never cause a null reference in Update.
            int validFuelCellCount = 0;
            if (FuelCells != null)
            {
                for (int i = 0; i < FuelCells.Length; i++)
                {
                    if (FuelCells[i] != null)
                        validFuelCellCount++;
                }
            }

            GameObject[] validFuelCells = new GameObject[validFuelCellCount];
            int nextFuelCellIndex = 0;
            if (FuelCells != null)
            {
                for (int i = 0; i < FuelCells.Length; i++)
                {
                    if (FuelCells[i] != null)
                        validFuelCells[nextFuelCellIndex++] = FuelCells[i];
                }
            }
            FuelCells = validFuelCells;

            m_FuelCellsCooled = new bool[FuelCells.Length];
            for (int i = 0; i < m_FuelCellsCooled.Length; i++)
            {
                m_FuelCellsCooled[i] = true;
            }
        }

        void Update()
        {
            if (SimultaneousFuelCellsUsage)
            {
                for (int i = 0; i < FuelCells.Length; i++)
                {
                    FuelCells[i].transform.localPosition = Vector3.Lerp(FuelCellUsedPosition, FuelCellUnusedPosition,
                        m_Weapon.CurrentAmmoRatio);
                }
            }
            else
            {
                // TODO: needs simplification
                for (int i = 0; i < FuelCells.Length; i++)
                {
                    float length = FuelCells.Length;
                    float lim1 = i / length;
                    float lim2 = (i + 1) / length;

                    float value = Mathf.InverseLerp(lim1, lim2, m_Weapon.CurrentAmmoRatio);
                    value = Mathf.Clamp01(value);

                    FuelCells[i].transform.localPosition =
                        Vector3.Lerp(FuelCellUsedPosition, FuelCellUnusedPosition, value);
                }
            }
        }
    }
}
