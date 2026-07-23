namespace StatSystem
{
    public class Modifier : IModifier
    {
        public Modifier (string targetStat, int type, float value, IModifierSource source = null)
        {
            StatId = targetStat;
            Type = type;
            Value = value;
            Source = source;
        }

        public string StatId { get; }
        public int Type { get; }
        public float Value { get; }
        public IModifierSource Source { get; }
    }
}