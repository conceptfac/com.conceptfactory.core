namespace Concept.Condition
{
    /// <summary>Shared Health(0-100) → ObjectCondition cut points — every IHasHealth implementer calls this instead of inventing its own thresholds, so "Damaged" means the same thing project-wide.</summary>
    public static class HealthThresholds
    {
        public const float HighlyDamagedAt = 25f;
        public const float DamagedAt = 60f;

        public static ObjectCondition ToCondition(float health0To100)
        {
            if (health0To100 <= 0f) return ObjectCondition.Ruined;
            if (health0To100 <= HighlyDamagedAt) return ObjectCondition.HighlyDamaged;
            if (health0To100 <= DamagedAt) return ObjectCondition.Damaged;
            return ObjectCondition.Pristine;
        }
    }
}
