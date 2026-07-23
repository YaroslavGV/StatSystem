using System.Collections.Generic;

namespace StatSystem
{
    public class DefaultStatCalculator : ICalculator
    {
        public CalculationResult Calculate (IEnumerable<IModifier> modifiers)
        {
            CalculationResult result = CalculateSum(modifiers);
            result.Value = (result.RawValue * result.RawMultiplier + result.AdditiveValue) * result.ResultMultiplier;
            return result;
        }

        protected CalculationResult CalculateSum (IEnumerable<IModifier> modifiers)
        {
            CalculationResult result = CalculationResult.DefaultValues;

            foreach (var m in modifiers)
            {
                switch ((ModifierType)m.Type)
                {
                    case ModifierType.RawValue:
                        result.RawValue += m.Value;
                        break;
                    case ModifierType.RawMultiplier:
                        result.RawMultiplier += m.Value;
                        break;
                    case ModifierType.AdditiveValue:
                        result.AdditiveValue += m.Value;
                        break;
                    case ModifierType.ResultMultiplier:
                        result.ResultMultiplier += m.Value;
                        break;
                }
            }
            return result;
        }
    }
}