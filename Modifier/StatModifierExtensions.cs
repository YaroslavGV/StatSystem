using System.Collections.Generic;
using System.Linq;

namespace StatSystem
{
    public static class StatModifierExtensions
    {
        public static IModifier ToModifier (this StatModifierData data, IModifierSource source)
            => new StatModifier(data.stat, data.type, data.value, source);

        public static IEnumerable<IModifier> ToModifiers (this IEnumerable<StatModifierData> datas, IModifierSource source)
            => datas.Select(data => data.ToModifier(source));
    }
}