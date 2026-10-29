using System.Collections.Generic;
using UnityEngine;

namespace Concept.Fluids
{
    /// <summary>
    /// Every fluid by its key, whichever package defines it: the core's water and a vehicles package's diesel are
    /// found the same way (<c>FluidRegistry.Find("diesel")</c>). Fluids register themselves as they load; the
    /// first lookup also loads every fluid kept under a <c>Resources/Fluids</c> folder of any package, so a key
    /// resolves without anyone holding a reference to the asset. Keys are read when looked up, not when a fluid
    /// registers — renaming one (or its id) just works.
    /// </summary>
    public static class FluidRegistry
    {
        public const string ResourcesFolder = "Fluids";

        private static readonly HashSet<Fluid> Known = new HashSet<Fluid>();
        private static readonly Dictionary<string, Fluid> ById = new Dictionary<string, Fluid>(System.StringComparer.OrdinalIgnoreCase);
        private static bool _scanned;
        private static bool _dirty = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetScan() => _scanned = false; // play mode without a domain reload: look again

        public static void Register(Fluid fluid)
        {
            if (fluid != null && Known.Add(fluid)) _dirty = true;
        }

        public static void Unregister(Fluid fluid)
        {
            if (fluid != null && Known.Remove(fluid)) _dirty = true;
        }

        /// <summary>A fluid's id changed (edited in the inspector): re-read the keys on the next lookup.</summary>
        public static void MarkDirty() => _dirty = true;

        public static Fluid Find(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            Scan();
            if (_dirty) Rebuild();
            if (ById.TryGetValue(id, out var fluid) && fluid != null && fluid.Matches(id)) return fluid;
            Rebuild(); // a key that went stale (destroyed or renamed asset)
            return ById.TryGetValue(id, out fluid) ? fluid : null;
        }

        public static T Find<T>(string id) where T : Fluid => Find(id) as T;

        public static IEnumerable<Fluid> All
        {
            get
            {
                Scan();
                if (_dirty) Rebuild();
                foreach (var fluid in ById.Values)
                    if (fluid != null) yield return fluid;
            }
        }

        private static void Scan()
        {
            if (_scanned) return;
            _scanned = true;
            foreach (var fluid in Resources.LoadAll<Fluid>(ResourcesFolder)) Register(fluid);
        }

        private static void Rebuild()
        {
            _dirty = false;
            ById.Clear();
            Known.RemoveWhere(f => f == null);
            foreach (var fluid in Known)
            {
                string key = fluid.Id;
                if (string.IsNullOrEmpty(key)) continue;
                if (ById.TryGetValue(key, out var other))
                {
                    Debug.LogWarning($"[FluidRegistry] Two fluids share the key \"{key}\": {other.name} and {fluid.name}. Keeping {other.name}.", fluid);
                    continue;
                }
                ById[key] = fluid;
            }
        }
    }
}
