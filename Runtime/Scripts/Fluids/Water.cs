using UnityEngine;

namespace Concept.Fluids
{
    /// <summary>
    /// Water — the one natural fluid every package shares (a drink, a coolant, a contaminant in a fuel tank, the
    /// ammunition of a water pistol). Fresh, sea or dirty water are assets of this.
    /// </summary>
    [CreateAssetMenu(menuName = "Concept Factory/Fluids/Water", fileName = "Water")]
    public class Water : Fluid
    {
        [Header("Water")]
        [Tooltip("Dissolved salt, g/L (fresh water ~0, sea water ~35). Salty water corrodes faster and doesn't quench thirst.")]
        [SerializeField, Min(0f)] private float salinity;
        [Tooltip("Safe to drink as it is.")]
        [SerializeField] private bool potable = true;

        public float Salinity => salinity;
        public bool Potable => potable;
    }
}
