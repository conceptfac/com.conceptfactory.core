using System;
using UnityEngine;

namespace Concept.Fluids
{
    /// <summary>
    /// Something that holds fluids: a capacity, what's in it, and an opening. The opening is the only hard rule —
    /// nothing thicker or chunkier than it lets through goes in (no chocolate spread up a syringe needle).
    /// What the container is meant for is only advice: a water pistol recommends water and still takes petrol.
    /// A plain object, so systems can own one (a car's fuel tank, an engine's sump); <see cref="FluidReservoir"/>
    /// puts one on a GameObject.
    /// </summary>
    [Serializable]
    public class FluidContainer
    {
        [Tooltip("Litres it holds.")]
        [SerializeField, Min(0f)] private float capacity = 1f;
        [SerializeField] private FluidMixture contents = new FluidMixture();
        [Tooltip("Thickest fluid the opening lets through, mPa·s (0 = no limit). A syringe needle ~50, a bottle neck ~20 000, a bucket: no limit.")]
        [SerializeField, Min(0f)] private float maxViscosity;
        [Tooltip("Largest particles the opening (or its filter) lets through, mm (0 = no limit).")]
        [SerializeField, Min(0f)] private float maxParticleSize;
        [Tooltip("What it's meant for: fluid ids or tags (\"water\", \"fuel\"…). Advice only — anything that passes the opening goes in.")]
        [SerializeField] private string[] recommended = Array.Empty<string>();
        [Tooltip("Cap off: it can be filled and poured from. Closed, nothing goes in or out through the opening (a tank's own outlet — an engine's fuel line — still draws).")]
        [SerializeField] private bool open = true;

        public FluidContainer() { }

        public FluidContainer(float capacity, float maxViscosity = 0f, float maxParticleSize = 0f, params string[] recommended)
        {
            this.capacity = Mathf.Max(0f, capacity);
            this.maxViscosity = maxViscosity;
            this.maxParticleSize = maxParticleSize;
            this.recommended = recommended ?? Array.Empty<string>();
        }

        /// <summary>Anything added or taken.</summary>
        public event Action Changed
        {
            add => contents.Changed += value;
            remove => contents.Changed -= value;
        }

        public float Capacity => capacity;
        public FluidMixture Contents => contents;
        public float Volume => contents.Volume;
        public float FreeVolume => Mathf.Max(0f, capacity - Volume);
        /// <summary>0-1.</summary>
        public float FillLevel => capacity > 0f ? Mathf.Clamp01(Volume / capacity) : 0f;
        public bool IsEmpty => contents.IsEmpty;
        public bool IsFull => FreeVolume <= FluidMixture.Epsilon;
        public float MaxViscosity => maxViscosity;
        public float MaxParticleSize => maxParticleSize;
        /// <summary>Whether the cap is off. Set it from whoever opens and closes it (a hand, an animation event).</summary>
        public bool IsOpen
        {
            get => open;
            set
            {
                if (open == value) return;
                open = value;
                OpenChanged?.Invoke(value);
            }
        }
        /// <summary>The cap came off (true) or went back on (false).</summary>
        public event Action<bool> OpenChanged;

        /// <summary>Whether it gets through the opening at all.</summary>
        public bool CanPass(Fluid fluid) =>
            fluid != null
            && (maxViscosity <= 0f || fluid.Viscosity <= maxViscosity)
            && (maxParticleSize <= 0f || fluid.ParticleSize <= maxParticleSize);

        /// <summary>Whether it's what the container is meant for (always true when it recommends nothing).</summary>
        public bool IsRecommended(Fluid fluid)
        {
            if (fluid == null) return false;
            if (recommended == null || recommended.Length == 0) return true;
            foreach (var key in recommended) if (fluid.Matches(key)) return true;
            return false;
        }

        /// <summary>Puts this much of a fluid straight in, cap or opening regardless — the system that owns the container filling it (a fresh tank, a top-up, a debug button). Returns the litres that fit.</summary>
        public float Put(Fluid fluid, float liters, float health = 1f)
        {
            if (fluid == null || !(liters > 0f)) return 0f;
            float added = Mathf.Min(liters, FreeVolume);
            contents.Add(fluid, added, health);
            return added;
        }

        /// <summary>Pours this much of a fluid in; returns the litres that went in (what passes the opening and fits).</summary>
        public float Fill(Fluid fluid, float liters, float health = 1f)
        {
            if (!open || !CanPass(fluid) || !(liters > 0f)) return 0f;
            float added = Mathf.Min(liters, FreeVolume);
            contents.Add(fluid, added, health);
            return added;
        }

        /// <summary>Pours in as much of a mixture as fits, taking it out of the source; fluids the opening refuses stay behind.</summary>
        public float Fill(FluidMixture source, float liters = float.PositiveInfinity)
        {
            if (source == null || !open) return 0f;
            float passing = source.LitersWhere(CanPass);
            float amount = Mathf.Min(liters, passing, FreeVolume);
            if (!(amount > FluidMixture.Epsilon)) return 0f;
            float share = amount / passing;
            float moved = 0f;
            var portions = source.Portions;
            for (int i = portions.Count - 1; i >= 0; i--)
            {
                var p = portions[i];
                if (!CanPass(p.fluid)) continue;
                var taken = source.Take(p.fluid, p.liters * share);
                moved += taken.Volume;
                contents.Add(taken);
            }
            return moved;
        }

        /// <summary>Pours this much out of the opening, well mixed (tipped and shaken). Nothing with the cap on.</summary>
        public FluidMixture Pour(float liters) => open ? contents.Take(Mathf.Min(liters, Volume)) : new FluidMixture();

        /// <summary>Drains this much from the bottom — the drain plug or a pump's pickup: the densest layer first.</summary>
        public FluidMixture Drain(float liters, float agitation = 0f) => contents.TakeFromBottom(liters, agitation);

        /// <summary>
        /// Pours from this container into another, as much as the other's opening and space allow; returns the litres
        /// that went in. With <paramref name="spillOver"/> what doesn't go in is still poured — over the rim, lost,
        /// reported by the target's <see cref="Spilled"/> — instead of staying in this one.
        /// </summary>
        public float TransferTo(FluidContainer target, float liters, bool spillOver = false)
        {
            if (target == null || target == this || !open || !target.open || !(liters > 0f)) return 0f;
            var poured = Pour(liters);
            float moved = target.Fill(poured);
            if (poured.IsEmpty) return moved;
            if (spillOver) target.Spill(poured);
            else contents.Add(poured); // whatever didn't go in stays where it was
            return moved;
        }

        /// <summary>Fluid poured at this container that ran over its rim (or past its opening): gone — a puddle, a stain, a fire to start, whoever listens.</summary>
        public event Action<FluidMixture> Spilled;

        /// <summary>Reports fluid that missed or overflowed this container.</summary>
        public void Spill(FluidMixture spilled)
        {
            if (spilled != null && !spilled.IsEmpty) Spilled?.Invoke(spilled);
        }

        /// <summary>Clamps what's inside to the capacity (after editing in the inspector).</summary>
        public void Sanitize()
        {
            capacity = Mathf.Max(0f, capacity);
            contents.Sanitize();
            float over = Volume - capacity;
            if (over > FluidMixture.Epsilon) contents.Take(over);
        }
    }
}
