using System.Collections.Generic;
using UnityEngine;

namespace Concept.Fluids
{
    /// <summary>
    /// "I hold fluids": a can, a bottle, a canteen, a jerrycan, a barrel, a pan, a car's fuel tank. Says how much
    /// it takes, what's inside (any mix: a 350 mL can of 90 % water and 10 % syrup), what its opening lets
    /// through and what it's meant for. Knows nothing about what the fluids are for — a drinks package, a weapons
    /// package or the vehicles package reads the contents and decides. An object can have several (a car: fuel,
    /// oil, coolant). Solids belong in an inventory of the object's own, not here.
    /// </summary>
    public class FluidReservoir : MonoBehaviour
    {
        private static readonly List<FluidReservoir> ActiveReservoirs = new List<FluidReservoir>();

        [Tooltip("What this reservoir is on its object (\"Fuel tank\", \"Engine oil\"…), for UI and for finding it among several.")]
        [SerializeField] private string label;
        [SerializeField] private FluidContainer container = new FluidContainer();

        /// <summary>Every reservoir enabled in the loaded scenes.</summary>
        public static IReadOnlyList<FluidReservoir> Active => ActiveReservoirs;

        public string Label => string.IsNullOrEmpty(label) ? name : label;
        public FluidContainer Container => container;
        public FluidMixture Contents => container.Contents;
        /// <summary>Cap off (true) or on. Whoever opens it — a hand, an animation event — sets it; nothing pours through a closed opening.</summary>
        public bool IsOpen
        {
            get => container.IsOpen;
            set => container.IsOpen = value;
        }

        /// <summary>The reservoir with this label on the object or its children (null when none).</summary>
        public static FluidReservoir Find(GameObject owner, string label)
        {
            if (owner == null) return null;
            foreach (var reservoir in owner.GetComponentsInChildren<FluidReservoir>(true))
                if (string.Equals(reservoir.Label, label, System.StringComparison.OrdinalIgnoreCase)) return reservoir;
            return null;
        }

        /// <summary>"Fuel tank — 32.0 / 48 L: 97% Gasoline, 3% Water".</summary>
        public string Describe() => $"{Label} — {container.Volume:0.0} / {container.Capacity:0.#} L: {container.Contents.Describe()}";

        private void OnEnable() => ActiveReservoirs.Add(this);
        private void OnDisable() => ActiveReservoirs.Remove(this);
        private void OnValidate() => container.Sanitize();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetActive() => ActiveReservoirs.Clear();
    }
}
