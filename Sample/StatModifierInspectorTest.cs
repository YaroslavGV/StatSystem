using System.Collections.Generic;
using UnityEngine;

namespace StatSystem.Tests
{
    public enum RPGStats { Health, Mana, Attack, Defense, Speed }

    /// <summary>
    /// Test component for visual verification of StatModifierData in the Unity Inspector.
    /// Place this on any GameObject in the scene to test the UI Toolkit layout and attributes.
    /// </summary>
    public class StatModifierInspectorTest : MonoBehaviour
    {
        [Header("Standard Configuration")]
        [Tooltip("Standard list with no restrictions. Try entering duplicate stat names and types to see the error highlight.")]
        public List<ModifierData<ModifierType>> defaultModifiers;

        [Header("Enum Dropdown Attributes")]
        [Tooltip("Replaces the string text field with an enum dropdown. Duplicate checking still works based on selected items.")]
        [StatEnum(typeof(RPGStats))]
        public List<ModifierData<ModifierType>> enumBasedModifiers;

        [Header("Locked Modifier Type Attributes")]
        [Tooltip("Forces all modifiers to be BaseValue. The type field is disabled, and defaults are applied automatically.")]
        [LockModifierType(ModifierType.RawValue)]
        [StatEnum(typeof(RPGStats))]
        public List<ModifierData<ModifierType>> characterBaseStats;

        [Header("Single Structure Fields")]
        [Tooltip("Single instances to verify that drawing works correctly outside of lists.")]
        public ModifierData<ModifierType> singleDefault;

        [LockModifierType(ModifierType.ResultMultiplier)]
        [StatEnum(typeof(RPGStats))]
        public ModifierData<ModifierType> singleLockedMultiplier;
    }
}