using System.Collections.Generic;

namespace StatSystem
{
    public interface ICalculator
    {
        CalculationResult Calculate (IEnumerable<IModifier> modifiers);
    }
}