using System.Collections.Generic;

namespace StatSystem
{
    public interface IStatCalculator
    {
        StatCalculationResult Calculate (IEnumerable<IModifier> modifiers);
    }
}