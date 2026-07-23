using System;
using System.Collections.Generic;

namespace StatSystem
{
    public static class ModifierDataExtensions
    {
        public static IModifier ToModifier<T> (this ModifierData<T> data, IModifierSource source) where T : Enum
            => new Modifier(data.stat, Convert.ToInt32(data.type), data.value, source);

        public static IEnumerable<IModifier> ToModifiers<T> (this IEnumerable<ModifierData<T>> datas, IModifierSource source) where T : Enum
        {
            foreach (var data in datas)
                yield return data.ToModifier(source);
        }
    }
}