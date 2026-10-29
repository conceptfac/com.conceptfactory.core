using UnityEngine;

namespace Concept.Fluids
{
    /// <summary>
    /// Project-wide fluid rules (Resources/FluidSettings). Whether pouring into something full stops, or carries on
    /// and spills over the rim (a gas station game where a careless fill-up overflows, a bottle under a tap left
    /// running) is the game's call, not the container's. Runtime code can override it (<see cref="StopWhenFull"/>).
    /// </summary>
    [CreateAssetMenu(menuName = "Concept Factory/Fluids/Fluid Settings", fileName = "FluidSettings")]
    public class FluidSettings : ScriptableObject
    {
        private const string ResourcePath = "FluidSettings";
        private static FluidSettings _asset;
        private static bool? _stopWhenFullOverride;

        [Tooltip("Pouring into something full stops by itself. Off: it carries on and the excess spills over (reported by the container's Spilled event).")]
        [SerializeField] private bool stopWhenFull = true;

        /// <summary>The project's rule — or whatever runtime code set it to (a game mode, a level).</summary>
        public static bool StopWhenFull
        {
            get
            {
                if (_stopWhenFullOverride.HasValue) return _stopWhenFullOverride.Value;
                if (_asset == null) _asset = Resources.Load<FluidSettings>(ResourcePath);
                return _asset == null || _asset.stopWhenFull;
            }
            set => _stopWhenFullOverride = value;
        }

        /// <summary>Back to the project asset's rule.</summary>
        public static void ClearOverrides() => _stopWhenFullOverride = null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOverrides() => _stopWhenFullOverride = null;
    }
}
