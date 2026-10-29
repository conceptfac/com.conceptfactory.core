using System;

namespace Concept.Condition
{
    /// <summary>
    /// Generic "this object has health" contract — a step on a ladder, a destructible wall, a vehicle
    /// part, anything. <see cref="Health"/> is the raw, continuous value (0-100); <see cref="Condition"/>
    /// is always DERIVED from it via <see cref="HealthThresholds.ToCondition"/>, never an independent
    /// field — the two can't drift out of sync because there's only one source of truth. Implementers
    /// decide how Health changes (damage amount, repair, whether use-while-damaged risks further loss);
    /// this contract only describes the resulting state, never the policy.
    /// </summary>
    public interface IHasHealth
    {
        /// <summary>0-100. 100 = undamaged, 0 = destroyed.</summary>
        float Health { get; }
        /// <summary>Derived from Health — see HealthThresholds.ToCondition.</summary>
        ObjectCondition Condition { get; }
        /// <summary>Fires on every Health change, however small.</summary>
        event Action<float> HealthChanged;
        /// <summary>Fires only when Health crosses a threshold into a different ObjectCondition — not on every HealthChanged.</summary>
        event Action<ObjectCondition> ConditionChanged;
    }
}
