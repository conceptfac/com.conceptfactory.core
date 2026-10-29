using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Concept.Fluids
{
    /// <summary>An amount of one fluid in a mixture, and its condition (1 = as new; old, burnt or spoilt = less).</summary>
    [Serializable]
    public struct FluidPortion
    {
        public Fluid fluid;
        [Min(0f)] public float liters;
        [Range(0f, 1f)] public float health;

        public FluidPortion(Fluid fluid, float liters, float health = 1f)
        {
            this.fluid = fluid;
            this.liters = liters;
            this.health = health;
        }
    }

    /// <summary>
    /// Whatever is in something: any number of fluids, each with its volume and condition — 80 % water, 20 % syrup;
    /// 40 L of petrol with half a litre of water at the bottom. Knows only physics: what dissolves into what, and
    /// that fluids which don't settle into layers, densest at the bottom — so what a tap or a pickup at the bottom
    /// draws first isn't always the average. Whoever uses the mixture decides what each fluid in it means.
    /// </summary>
    [Serializable]
    public class FluidMixture
    {
        public const float Epsilon = 1e-6f;
        /// <summary>Water, as a share of the amphiphilic fluids (ethanol…), they still keep dissolved; past it they leave with the water for a layer of their own (phase separation).</summary>
        public const float PhaseSeparationWater = 0.1f;

        [SerializeField] private List<FluidPortion> portions = new List<FluidPortion>();

        /// <summary>Anything added, taken or changed.</summary>
        public event Action Changed;

        public IReadOnlyList<FluidPortion> Portions => portions;

        public float Volume
        {
            get
            {
                float volume = 0f;
                foreach (var p in portions) volume += p.liters;
                return volume;
            }
        }

        public bool IsEmpty => Volume <= Epsilon;

        /// <summary>kg/L of the whole.</summary>
        public float Density
        {
            get
            {
                float mass = 0f, volume = 0f;
                foreach (var p in portions)
                {
                    if (p.fluid == null) continue;
                    mass += p.fluid.Density * p.liters;
                    volume += p.liters;
                }
                return volume > Epsilon ? mass / volume : 0f;
            }
        }

        /// <summary>mPa·s of the whole (log-mixing: a little honey thickens water a lot less than a little water thins honey).</summary>
        public float Viscosity
        {
            get
            {
                float log = 0f, volume = 0f;
                foreach (var p in portions)
                {
                    if (p.fluid == null) continue;
                    log += Mathf.Log(p.fluid.Viscosity) * p.liters;
                    volume += p.liters;
                }
                return volume > Epsilon ? Mathf.Exp(log / volume) : 0f;
            }
        }

        /// <summary>Volume-weighted condition, 0-1.</summary>
        public float Health
        {
            get
            {
                float health = 0f, volume = 0f;
                foreach (var p in portions)
                {
                    health += p.health * p.liters;
                    volume += p.liters;
                }
                return volume > Epsilon ? health / volume : 1f;
            }
        }

        public float MaxParticleSize
        {
            get
            {
                float size = 0f;
                foreach (var p in portions) if (p.fluid != null) size = Mathf.Max(size, p.fluid.ParticleSize);
                return size;
            }
        }

        /// <summary>The fluid there is most of.</summary>
        public Fluid Dominant
        {
            get
            {
                Fluid best = null;
                float most = 0f;
                foreach (var p in portions)
                    if (p.liters > most) { most = p.liters; best = p.fluid; }
                return best;
            }
        }

        public float LitersOf(Fluid fluid)
        {
            int i = IndexOf(fluid);
            return i >= 0 ? portions[i].liters : 0f;
        }

        public float HealthOf(Fluid fluid)
        {
            int i = IndexOf(fluid);
            return i >= 0 ? portions[i].health : 1f;
        }

        public float ShareOf(Fluid fluid) => Share(LitersOf(fluid));

        public float LitersWhere(Predicate<Fluid> match)
        {
            float liters = 0f;
            foreach (var p in portions) if (p.fluid != null && match(p.fluid)) liters += p.liters;
            return liters;
        }

        public float ShareWhere(Predicate<Fluid> match) => Share(LitersWhere(match));

        /// <summary>Liters of fluids with this id or tag.</summary>
        public float LitersOf(string key) => LitersWhere(f => f.Matches(key));

        public void Add(Fluid fluid, float liters, float health = 1f)
        {
            if (Merge(fluid, liters, health)) Changed?.Invoke();
        }

        /// <summary>Adds a copy of everything in the other mixture (the other one is left as it is).</summary>
        public void Add(FluidMixture other)
        {
            if (other == null || other == this) return;
            bool any = false;
            foreach (var p in other.portions) any |= Merge(p.fluid, p.liters, p.health);
            if (any) Changed?.Invoke();
        }

        /// <summary>Takes this much out evenly, as if stirred: every fluid in its share.</summary>
        public FluidMixture Take(float liters)
        {
            var taken = new FluidMixture();
            float volume = Volume;
            if (volume <= Epsilon || !(liters > 0f)) return taken;
            TakeShare(null, Mathf.Clamp01(liters / volume), taken);
            Changed?.Invoke();
            return taken;
        }

        /// <summary>Takes this much of one fluid only (a water separator, a skimmer).</summary>
        public FluidMixture Take(Fluid fluid, float liters)
        {
            var taken = new FluidMixture();
            int i = IndexOf(fluid);
            if (i < 0 || !(liters > 0f)) return taken;
            var p = portions[i];
            float amount = Mathf.Min(liters, p.liters);
            taken.Merge(p.fluid, amount, p.health);
            p.liters -= amount;
            if (p.liters <= Epsilon) portions.RemoveAt(i);
            else portions[i] = p;
            Changed?.Invoke();
            return taken;
        }

        /// <summary>
        /// Takes this much from the bottom, as a drain plug or a pump pickup does: from the densest layer up. Stirred
        /// (agitation 0-1: sloshing, shaking) that share of it comes out evenly mixed instead.
        /// </summary>
        public FluidMixture TakeFromBottom(float liters, float agitation = 0f)
        {
            var taken = new FluidMixture();
            float volume = Volume;
            if (volume <= Epsilon || !(liters > 0f)) return taken;
            liters = Mathf.Min(liters, volume);
            float mixed = liters * Mathf.Clamp01(agitation);
            if (mixed > Epsilon) TakeShare(null, mixed / volume, taken);
            float rest = liters - mixed;
            foreach (var layer in BuildLayers())
            {
                if (rest <= Epsilon) break;
                float layerVolume = 0f;
                foreach (var p in portions) if (layer.Contains(p.fluid)) layerVolume += p.liters;
                if (layerVolume <= Epsilon) continue;
                float amount = Mathf.Min(rest, layerVolume);
                TakeShare(layer, amount / layerVolume, taken);
                rest -= amount;
            }
            Changed?.Invoke();
            return taken;
        }

        /// <summary>What the bottom would give, without taking it (what a pickup is about to draw).</summary>
        public FluidMixture SampleFromBottom(float liters, float agitation = 0f) => Clone().TakeFromBottom(liters, agitation);

        /// <summary>The mixture as it settles, bottom layer first (one layer when everything dissolves together).</summary>
        public List<FluidMixture> Layers()
        {
            var layers = new List<FluidMixture>();
            foreach (var layer in BuildLayers())
            {
                var mixture = new FluidMixture();
                foreach (var p in portions) if (layer.Contains(p.fluid)) mixture.Merge(p.fluid, p.liters, p.health);
                layers.Add(mixture);
            }
            return layers;
        }

        /// <summary>Lowers the condition of every fluid (or those matching) by this much — ageing, burning, spoiling.</summary>
        public void Degrade(float amount, Predicate<Fluid> match = null)
        {
            if (!(amount > 0f)) return;
            for (int i = 0; i < portions.Count; i++)
            {
                var p = portions[i];
                if (match != null && (p.fluid == null || !match(p.fluid))) continue;
                p.health = Mathf.Max(0f, p.health - amount);
                portions[i] = p;
            }
            Changed?.Invoke();
        }

        public void Clear()
        {
            if (portions.Count == 0) return;
            portions.Clear();
            Changed?.Invoke();
        }

        public FluidMixture Clone()
        {
            var copy = new FluidMixture();
            copy.portions.AddRange(portions);
            return copy;
        }

        /// <summary>"80% Water, 20% Syrup".</summary>
        public string Describe()
        {
            float volume = Volume;
            if (volume <= Epsilon) return "empty";
            var text = new StringBuilder();
            foreach (var p in portions)
            {
                if (p.fluid == null) continue;
                if (text.Length > 0) text.Append(", ");
                text.Append(Mathf.RoundToInt(p.liters / volume * 100f)).Append("% ").Append(p.fluid.DisplayName);
            }
            return text.ToString();
        }

        /// <summary>Drops portions whose fluid asset is gone and clamps the numbers (after editing in the inspector).</summary>
        public void Sanitize()
        {
            for (int i = portions.Count - 1; i >= 0; i--)
            {
                var p = portions[i];
                if (p.fluid == null && p.liters <= Epsilon) { portions.RemoveAt(i); continue; }
                p.liters = Mathf.Max(0f, p.liters);
                p.health = Mathf.Clamp01(p.health);
                portions[i] = p;
            }
        }

        private float Share(float liters)
        {
            float volume = Volume;
            return volume > Epsilon ? liters / volume : 0f;
        }

        private int IndexOf(Fluid fluid)
        {
            if (fluid == null) return -1;
            for (int i = 0; i < portions.Count; i++) if (portions[i].fluid == fluid) return i;
            return -1;
        }

        private bool Merge(Fluid fluid, float liters, float health)
        {
            if (fluid == null || !(liters > Epsilon)) return false;
            health = Mathf.Clamp01(health);
            int i = IndexOf(fluid);
            if (i < 0)
            {
                portions.Add(new FluidPortion(fluid, liters, health));
                return true;
            }
            var p = portions[i];
            float total = p.liters + liters;
            p.health = (p.health * p.liters + health * liters) / total;
            p.liters = total;
            portions[i] = p;
            return true;
        }

        // Every fluid in the set (all when null) gives up this share of itself.
        private void TakeShare(HashSet<Fluid> set, float share, FluidMixture into)
        {
            for (int i = portions.Count - 1; i >= 0; i--)
            {
                var p = portions[i];
                if (set != null && !set.Contains(p.fluid)) continue;
                float amount = p.liters * share;
                into.Merge(p.fluid, amount, p.health);
                p.liters -= amount;
                if (p.liters <= Epsilon) portions.RemoveAt(i);
                else portions[i] = p;
            }
        }

        // Which fluids settle together, bottom layer first. Aqueous and organic fluids separate; amphiphilic ones hold
        // a little water dissolved in the organic layer (one phase), and past that go down with the water.
        private List<HashSet<Fluid>> BuildLayers()
        {
            float aqueous = 0f, organic = 0f, amphiphilic = 0f;
            foreach (var p in portions)
            {
                if (p.fluid == null) continue;
                switch (p.fluid.Affinity)
                {
                    case FluidAffinity.Aqueous: aqueous += p.liters; break;
                    case FluidAffinity.Organic: organic += p.liters; break;
                    default: amphiphilic += p.liters; break;
                }
            }

            var layers = new List<HashSet<Fluid>>();
            bool separates = aqueous > Epsilon && organic > Epsilon && aqueous >= PhaseSeparationWater * amphiphilic;
            if (!separates)
            {
                var all = new HashSet<Fluid>();
                foreach (var p in portions) all.Add(p.fluid);
                layers.Add(all);
                return layers;
            }

            var watery = new HashSet<Fluid>();
            var oily = new HashSet<Fluid>();
            float wateryMass = 0f, wateryVolume = 0f, oilyMass = 0f, oilyVolume = 0f;
            foreach (var p in portions)
            {
                bool toWater = p.fluid == null || p.fluid.Affinity != FluidAffinity.Organic;
                (toWater ? watery : oily).Add(p.fluid);
                float density = p.fluid != null ? p.fluid.Density : 1f;
                if (toWater) { wateryMass += density * p.liters; wateryVolume += p.liters; }
                else { oilyMass += density * p.liters; oilyVolume += p.liters; }
            }
            bool waterBelow = wateryMass / Mathf.Max(wateryVolume, Epsilon) >= oilyMass / Mathf.Max(oilyVolume, Epsilon);
            layers.Add(waterBelow ? watery : oily);
            layers.Add(waterBelow ? oily : watery);
            return layers;
        }
    }
}
