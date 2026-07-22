using System;
using System.Collections.Generic;

namespace StatSystem
{
    /// <summary>
    /// A synchronization handler that subscribes to aggregators (inventory, buffs) and propagates 
    /// changes into the character's StatsGroup, decoupling stats from external gameplay systems.
    /// The Dispose() method must be explicitly called during Unity's OnDestroy event to prevent memory leaks.
    /// </summary>
    public class StatsLinker : IDisposable
    {
        private readonly StatsGroup _statsGroup;
        private readonly List<IModifierAggregator> _aggregators = new List<IModifierAggregator>();

        public StatsLinker (StatsGroup statsGroup) => _statsGroup = statsGroup;

        public void RegisterAggregator (IModifierAggregator aggregator)
        {
            if (_aggregators.Contains(aggregator))
                return;

            _aggregators.Add(aggregator);

            aggregator.OnSourceAdded += HandleSourceAdded;
            aggregator.OnSourceRemoved += HandleSourceRemoved;
            aggregator.OnAllSourcesUpdated += UpdateAllModifiers;

            foreach (var source in aggregator.GetModifierSources())
                _statsGroup.AddModifiersFromSource(source);
        }

        public void UnregisterAggregator (IModifierAggregator aggregator)
        {
            if (_aggregators.Remove(aggregator) == false)
                return;

            aggregator.OnSourceAdded -= HandleSourceAdded;
            aggregator.OnSourceRemoved -= HandleSourceRemoved;
            aggregator.OnAllSourcesUpdated -= UpdateAllModifiers;
        }

        public void Clear ()
        {
            for (int i = 0; i < _aggregators.Count; i++)
            {
                _aggregators[i].OnSourceAdded -= HandleSourceAdded;
                _aggregators[i].OnSourceRemoved -= HandleSourceRemoved;
                _aggregators[i].OnAllSourcesUpdated -= UpdateAllModifiers;
            }
            _aggregators.Clear();
            _statsGroup.CleanAllModifiers();
        }

        public void UpdateAllModifiers ()
        {
            _statsGroup.CleanAllModifiers();

            for (int i = 0; i < _aggregators.Count; i++)
            {
                foreach (var source in _aggregators[i].GetModifierSources())
                    _statsGroup.AddModifiersFromSource(source);
            }
        }

        private void HandleSourceAdded (IModifierSource source)
            => _statsGroup.AddModifiersFromSource(source);

        private void HandleSourceRemoved (IModifierSource source)
            => _statsGroup.RemoveModifiersFromSource(source);

        public void Dispose ()
            => Clear();
    }
}