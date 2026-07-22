namespace StatSystem
{
    public class StatModifier : IModifier
    {
        public StatModifier (string targetStat, ModificationType type, float value, IModifierSource source)
        {
            StatId = targetStat;
            Type = type;
            Value = value;
            Source = source;
        }

        public string StatId { get; }
        public ModificationType Type { get; }
        public float Value { get; }
        public IModifierSource Source { get; }
    }
}