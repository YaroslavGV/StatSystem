using System.Collections.Generic;

namespace StatSystem
{
    public class DefaultStatCalculator : IStatCalculator
    {
        public StatCalculationResult Calculate (IEnumerable<IModifier> modifiers)
        {
            StatCalculationResult result = CalculateSum(modifiers);
            result.Value = (result.RawValue * result.RawMultiplier + result.AdditiveValue) * result.ResultMultiplier;
            return result;
        }

        protected StatCalculationResult CalculateSum (IEnumerable<IModifier> modifiers)
        {
            StatCalculationResult result = StatCalculationResult.DefaultValues;

            foreach (var m in modifiers)
            {
                switch (m.Type)
                {
                    case ModificationType.RawValue:
                        result.RawValue += m.Value;
                        break;
                    case ModificationType.RawMultiplier:
                        result.RawMultiplier += m.Value;
                        break;
                    case ModificationType.AdditiveValue:
                        result.AdditiveValue += m.Value;
                        break;
                    case ModificationType.ResultMultiplier:
                        result.ResultMultiplier += m.Value;
                        break;
                }
            }
            return result;
        }
    }
}