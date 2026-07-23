using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace StatSystem.Tests
{
    #region Test Mocks

    /// <summary>
    /// Mock implementation of IStatModifierSource to group multiple modifiers for integration testing.
    /// </summary>
    public class TestModifierSource : IModifierSource
    {
        public List<IModifier> Modifiers { get; set; } = new List<IModifier>();
        public IEnumerable<IModifier> GetModifiers () => Modifiers;
    }

    /// <summary>
    /// Mock implementation of IModifierAggregator to simulate systems like Inventory or Buffs.
    /// </summary>
    public class TestModifierAggregator : IModifierAggregator
    {
        public event Action<IModifierSource> OnSourceAdded;
        public event Action<IModifierSource> OnSourceRemoved;
        public event Action OnAllSourcesUpdated;

        private readonly List<IModifierSource> _sources = new List<IModifierSource>();

        public IEnumerable<IModifierSource> GetModifierSources () => _sources;

        public void AddSource (IModifierSource source)
        {
            _sources.Add(source);
            OnSourceAdded?.Invoke(source);
        }

        public void RemoveSource (IModifierSource source)
        {
            if (_sources.Remove(source))
            {
                OnSourceRemoved?.Invoke(source);
            }
        }

        public void RaiseAllSourcesUpdated ()
        {
            OnAllSourcesUpdated?.Invoke();
        }
    }

    /// <summary> Mock implementation of INotifiableModifier to simulate runtime dynamic changes (e.g., ticking updates). </summary>
    public class TestDynamicModifier : INotifiableModifier
    {
        private float _value;

        public event Action<IModifier> OnValueChanged;

        public string StatId { get; set; }
        public int Type { get; set; }
        public IModifierSource Source { get; set; }

        public float Value
        {
            get => _value;
            set
            {
                if (UnityEngine.Mathf.Approximately(_value, value)) return;
                _value = value;
                // Notify subscribers (like the Stat class) that the underlying value has shifted
                OnValueChanged?.Invoke(this);
            }
        }
    }
    #endregion

    [TestFixture]
    public class StatSystemTests
    {
        private StatsGroup _statsGroup;
        private TestModifierAggregator _inventoryAggregator;
        private StatsLinker _linker;

        [SetUp]
        public void Setup ()
        {
            _statsGroup = new StatsGroup();
            _inventoryAggregator = new TestModifierAggregator();
            _linker = new StatsLinker(_statsGroup);
        }

        [TearDown]
        public void TearDown ()
        {
            _linker.Dispose();
        }

        /// <summary> AddModifier with mismatched target stat throws argument exception. </summary>
        [Test]
        public void AddWrongTypeModifier ()
        {
            // Arrange
            var healthStat = _statsGroup.GetStat("Health");

            // Creating a modifier intended for "Mana" but trying to inject it into "Health"
            var wrongModifier = new Modifier("Mana", (int)ModifierType.RawValue, 10f);
            // Act & Assert
            // Assumes your validation throws an ArgumentException (or you can change to LogAssert if you use Debug.LogError)
            Assert.Throws<ArgumentException>(() => healthStat.AddModifier(wrongModifier),
                "Stat should reject modifiers whose TargetStat does not match the Stat's Name.");
        }

        [Test]
        public void StatRecalculation ()
        {
            // Arrange
            var stat = _statsGroup.GetStat("Attack");

            // Formula: (BaseValue * BaseMultiplier + AdditiveBonus) * ResultMultiplier
            // ( (0 + 100) * (1 + 0.2) + 15 ) * (1 + 0.1) = (120 + 15) * 1.1 = 135 * 1.1 = 148.5
            stat.AddModifier(new Modifier("Attack", (int)ModifierType.RawValue, 100f));
            stat.AddModifier(new Modifier("Attack", (int)ModifierType.RawMultiplier, 0.2f));
            stat.AddModifier(new Modifier("Attack", (int)ModifierType.AdditiveValue, 15f));
            stat.AddModifier(new Modifier("Attack", (int)ModifierType.ResultMultiplier, 0.1f));

            // Act & Assert
            Assert.AreEqual(148.5f, stat.Value, 0.001f, "Math order of operations failed.");
        }

        [Test]
        public void StatsLinkerInitializes ()
        {
            // Arrange
            var sword = new TestModifierSource();
            sword.Modifiers.Add(new Modifier("Attack", (int)ModifierType.RawValue, 50f, sword));
            _inventoryAggregator.AddSource(sword);

            // Act
            _linker.RegisterAggregator(_inventoryAggregator);

            // Assert
            Assert.AreEqual(50f, _statsGroup["Attack"].Value, "Linker failed to catch existing modifiers on registration.");
        }

        [Test]
        public void StatsLinkerAddRemoveSource ()
        {
            // Arrange
            _linker.RegisterAggregator(_inventoryAggregator);
            var shield = new TestModifierSource();
            shield.Modifiers.Add(new Modifier("Defense", (int)ModifierType.RawValue, 30f, shield));

            // Act: Dynamic Add
            _inventoryAggregator.AddSource(shield);

            // Assert Add
            Assert.AreEqual(30f, _statsGroup["Defense"].Value, "Linker failed to dynamically propagate added modifier source.");

            // Act: Dynamic Remove
            _inventoryAggregator.RemoveSource(shield);

            // Assert Remove
            Assert.AreEqual(0f, _statsGroup["Defense"].Value, "Linker failed to dynamically propagate removed modifier source.");
        }

        [Test]
        public void DynamicModifier ()
        {
            // Arrange
            var speedStat = _statsGroup.GetStat("Speed");
            speedStat.AddModifier(new Modifier("Speed", (int)ModifierType.RawValue, 10f)); // Requires the baseline feature we implemented earlier

            var dynamicBuff = new TestDynamicModifier
            {
                StatId = "Speed",
                Type = (int)ModifierType.AdditiveValue,
                Value = 5f,
                Source = null
            };

            // Act: Add the dynamic modifier to the stat
            speedStat.AddModifier(dynamicBuff);

            // Initial evaluation: Base (10) + Additive (5) = 15
            Assert.AreEqual(15f, speedStat.Value, 0.001f, "Initial dynamic calculation failed.");

            // Act: Modify the value inside the dynamic buff. 
            // This triggers OnValueChanged inside TestDynamicModifier, which should force speedStat to set _isDirty = true.
            dynamicBuff.Value = 12f;

            // Assert: Check if the stat lazily picked up the change on the next read
            // New evaluation: Base (10) + Additive (12) = 22
            Assert.AreEqual(22f, speedStat.Value, 0.001f, "Stat failed to invalidate cache and recalculate after dynamic modifier event.");

            // Act: Remove the modifier to verify safe unsubscription
            speedStat.RemoveModifier(dynamicBuff);

            // Changing value after removal should NOT affect the stat or cause errors
            dynamicBuff.Value = 100f;

            Assert.AreEqual(10f, speedStat.Value, 0.001f, "Stat failed to properly unsubscribe from dynamic modifier on removal.");
        }
    }
}
