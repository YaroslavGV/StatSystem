namespace StatSystem
{
    /// <summary> Sum of modifiers of different types. </summary>
    public struct StatCalculationResult
    {
        public float RawValue      { get; set; }
        public float RawMultiplier { get; set; }
        public float AdditiveValue      { get; set; }
        public float ResultMultiplier { get; set; }

        public float Value { get; set; }

        public static StatCalculationResult DefaultValues 
            => new StatCalculationResult() { RawMultiplier = 1f, ResultMultiplier = 1f };
    }
}