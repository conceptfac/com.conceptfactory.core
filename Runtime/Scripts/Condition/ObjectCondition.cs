namespace Concept.Condition
{
    /// <summary>
    /// Discrete, derived bucketing of an object's <see cref="IHasHealth.Health"/> — never set directly,
    /// always computed from the numeric value via <see cref="HealthThresholds.ToCondition"/>, so every
    /// object shares the same cut points instead of each one inventing its own. Orthogonal to being
    /// interactable (ConceptFactory.Interactions): a lever is interactable with no health at all, a
    /// destructible wall has health and is never interacted with — this lives in Core, the one layer
    /// every package can already see, not in Interactions.
    /// </summary>
    public enum ObjectCondition
    {
        Pristine,
        Damaged,
        HighlyDamaged,
        Ruined,
    }
}
