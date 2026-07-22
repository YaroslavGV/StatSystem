using System;
using System.Collections.Generic;
using System.Text;

namespace StatSystem
{
    public class Stat
    {
        private readonly IStatCalculator _calculator;
        private bool _isDirty;
        private HashSet<IModifier> _modifiers = new HashSet<IModifier>();
        private List<IModifier> _toRemove = new List<IModifier>();
        
        public Stat (string id, IStatCalculator calculator)
        {
            Id = id;
            _calculator = calculator;
            _isDirty = true;
        }

        public string Id { get; }
        public float Value => Recalculate();
        public StatCalculationResult CalculationResult { get; private set; }

        public override string ToString ()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[Stat] {Id}: {Value} (Modifiers: {_modifiers.Count})");

            if (_modifiers.Count > 0)
                foreach (var m in _modifiers)
                    sb.AppendLine($"  - [{m.Type}] {m.Value:+#;-#;0} from {m.Source?.GetType().Name ?? "Unknown"} ({m.Source})");
            return sb.ToString().TrimEnd();
        }

        public void AddModifier (IModifier modifier)
        {
            if (modifier.StatId != Id)
                throw new ArgumentException($"Wrong target stat. Expected: {Id}, Got: {modifier.StatId}");

            if (_modifiers.Add(modifier))
            {
                _isDirty = true;

                if (modifier is INotifiableModifier nm)
                    nm.OnValueChanged += HandleDynamicModifierChanged;
            }
        }

        public void RemoveModifier (IModifier modifier)
        {
            if (_modifiers.Remove(modifier))
            {
                _isDirty = true;

                if (modifier is INotifiableModifier nm)
                    nm.OnValueChanged -= HandleDynamicModifierChanged;
            }
        }

        /// <summary>
        /// Selectively removes all modifiers applied by a specific causing object.
        /// Invoked by reactive managers when an item is unequipped or a buff expires.
        /// Garbage Collection safe: iterates the collection without allocating memory for closures.
        /// </summary>
        public void RemoveModifiersFromSource (IModifierSource source)
        {
            _toRemove.Clear();
            foreach (var m in _modifiers)
                if (m.Source == source) 
                    _toRemove.Add(m);

            for (int i = 0; i < _toRemove.Count; i++)
            {
                var m = _toRemove[i];
                _modifiers.Remove(m);

                if (m is INotifiableModifier nm)
                    nm.OnValueChanged -= HandleDynamicModifierChanged;
            }

            if (_toRemove.Count > 0) 
                _isDirty = true;
        }

        public void CleanModifiers ()
        {
            foreach (var m in _modifiers)
                if (m is INotifiableModifier nm)
                    nm.OnValueChanged -= HandleDynamicModifierChanged;
            _modifiers.Clear();
            _isDirty = true;
        }

        /// <summary>
        /// Performs a lazy recalculation of the final stat value based on standard RPG mathematical 
        /// order of operations (accumulating flat bonuses, then applying multipliers).
        /// Executes only if the _isDirty flag is true, minimizing CPU overhead.
        /// </summary>
        private float Recalculate ()
        {
            if (_isDirty == false)
                return CalculationResult.Value;

            CalculationResult = _calculator.Calculate(_modifiers);
            _isDirty = false;

            return CalculationResult.Value;
        }

        /// <summary> Reacts to internal changes of dynamic modifiers and invalidates the cache. </summary>
        private void HandleDynamicModifierChanged (IModifier modifier)
            => _isDirty = true;
    }
}