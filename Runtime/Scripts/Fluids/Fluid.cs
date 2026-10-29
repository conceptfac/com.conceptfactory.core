using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Concept.Fluids
{
    /// <summary>How a fluid flows: water runs, oil clings, syrup drags, a paste has to be pushed.</summary>
    public enum FluidPhase { Liquid, Oily, Viscous, Paste }

    /// <summary>
    /// What a fluid dissolves into. Aqueous and organic fluids don't mix: left standing they separate into layers,
    /// the denser at the bottom (water under petrol, vinegar under oil). Amphiphilic ones (ethanol, soap, many
    /// additives) mix with either — and, given enough water, leave the organic layer for the watery one.
    /// </summary>
    public enum FluidAffinity { Aqueous, Organic, Amphiphilic }

    /// <summary>
    /// A fluid as data: only what physics knows about it — never what it is for. Water, petrol, blood, cola syrup
    /// or acid are all just assets of this (or of a package's subclass that adds its own knowledge: a fuel's
    /// heating value lives in the vehicles package, not here). Found anywhere by its <see cref="Id"/> through
    /// <see cref="FluidRegistry"/>; quantities, proportions and condition live in a <see cref="FluidMixture"/>.
    /// </summary>
    [CreateAssetMenu(menuName = "Concept Factory/Fluids/Fluid", fileName = "Fluid")]
    public class Fluid : ScriptableObject
    {
        [Tooltip("Unique key every package finds this fluid by (lower case, e.g. \"water\", \"gasoline\"). Empty = the asset's name.")]
        [SerializeField] private string id;
        [SerializeField] private string displayName = "Fluid";
        [Tooltip("Free-form tags for what it's usually taken as (\"water\", \"fuel\", \"drink\", \"corrosive\"…) — containers recommend by these.")]
        [SerializeField] private string[] tags = Array.Empty<string>();

        [Header("Physical")]
        [SerializeField] private FluidPhase phase = FluidPhase.Liquid;
        [SerializeField] private FluidAffinity affinity = FluidAffinity.Aqueous;
        [Tooltip("kg/L at 20 °C (water 1.0, petrol ~0.745, diesel ~0.83, honey ~1.42).")]
        [FormerlySerializedAs("densityKgPerLiter")]
        [SerializeField, Min(0.01f)] private float density = 1f;
        [Tooltip("Dynamic viscosity at 20 °C, mPa·s (water 1, petrol ~0.5, diesel ~3, motor oil ~100-250, honey ~10 000, chocolate spread ~100 000).")]
        [SerializeField, Min(0.01f)] private float viscosity = 1f;
        [Tooltip("Largest solid particles it carries, mm (0 = none: a true liquid). Muddy water ~0.1, pulpy juice ~1.")]
        [SerializeField, Min(0f)] private float particleSize;
        [Tooltip("°C.")]
        [SerializeField] private float freezingPoint;
        [Tooltip("°C.")]
        [SerializeField] private float boilingPoint = 100f;
        [SerializeField] private bool flammable;
        [Tooltip("°C: the lowest temperature its vapour catches fire at (petrol −43, diesel ~55, ethanol 13).")]
        [SerializeField] private float flashPoint = 100f;
        [Tooltip("How it looks in a glass (alpha = opacity).")]
        [SerializeField] private Color color = new Color(0.6f, 0.75f, 0.9f, 0.3f);

        public string Id => string.IsNullOrEmpty(id) ? name.ToLowerInvariant() : id;
        public string DisplayName => displayName;
        public IReadOnlyList<string> Tags => tags;
        public FluidPhase Phase => phase;
        public FluidAffinity Affinity => affinity;
        /// <summary>kg/L.</summary>
        public float Density => density;
        /// <summary>mPa·s.</summary>
        public float Viscosity => viscosity;
        /// <summary>mm.</summary>
        public float ParticleSize => particleSize;
        public float FreezingPoint => freezingPoint;
        public float BoilingPoint => boilingPoint;
        public bool Flammable => flammable;
        public float FlashPoint => flashPoint;
        public Color Color => color;

        public bool HasTag(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return false;
            foreach (var t in tags)
                if (string.Equals(t, tag, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        /// <summary>Its id or one of its tags.</summary>
        public bool Matches(string key) => string.Equals(Id, key, StringComparison.OrdinalIgnoreCase) || HasTag(key);

        /// <summary>Whether the two dissolve into one another rather than settling into layers.</summary>
        public bool MixesWith(Fluid other) =>
            other != null && (affinity == other.affinity || affinity == FluidAffinity.Amphiphilic || other.affinity == FluidAffinity.Amphiphilic);

        protected virtual void OnEnable() => FluidRegistry.Register(this);
        protected virtual void OnDisable() => FluidRegistry.Unregister(this);
        protected virtual void OnValidate() => FluidRegistry.MarkDirty();
    }
}
