using System.Text;
using System.Collections.Generic;

namespace StatSystem
{
    public class StatsGroup : IEnumerable<Stat>
    {
        private readonly Dictionary<string, Stat> _stats = new Dictionary<string, Stat>();
        private readonly IStatCalculator _calculator;

        public Stat this[string id] => GetStat(id);

        public StatsGroup (IStatCalculator calculator = null)
            => _calculator = calculator ?? new DefaultStatCalculator();
        
        public override string ToString ()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"=== StatsGroup (Total Stats: {_stats.Count}) ===");

            foreach (var stat in _stats.Values)
            {
                // Смещаем вывод каждой характеристики для читаемости в общей группе
                string statStr = stat.ToString().Replace("\n", "\n  ");
                sb.AppendLine($"  {statStr}");
            }

            sb.Append("=================================");
            return sb.ToString();
        }

        public void AddModifier (IModifier modifier)
        {
            if (_stats.TryGetValue(modifier.StatId, out var stat) == false)
                stat = GetStat(modifier.StatId);
            stat.AddModifier(modifier);
        }

        public void AddModifiersFromSource (IModifierSource source)
        {
            foreach (var m in source.GetModifiers())
                AddModifier(m);
        }

        public void RemoveModifier (IModifier modifier)
        {
            if (_stats.TryGetValue(modifier.StatId, out var stat))
                stat.RemoveModifier(modifier);
        }

        public void RemoveModifiersFromSource (IModifierSource source)
        {
            foreach (var stat in _stats.Values)
                stat.RemoveModifiersFromSource(source);
        }

        public void CleanAllModifiers ()
        {
            foreach (var stat in _stats.Values)
                stat.CleanModifiers();
        }

        public Stat GetStat (string id)
        {
            if (_stats.TryGetValue(id, out var stat) == false)
            {
                stat = new Stat(id, _calculator);
                _stats.Add(id, stat);
            }
            return stat;
        }

        public IEnumerator<Stat> GetEnumerator ()
            => _stats.Values.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
            => GetEnumerator();
    }
}