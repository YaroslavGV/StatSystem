using System;

namespace StatSystem
{
    [Serializable]
    public struct StatModifierData
    {
        public string stat;
        public ModificationType type;
        public float value;
    }

    /// <summary>
    /// Prevents changing the modifier type in the inspector, forcing it to a specific type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class LockModifierTypeAttribute : Attribute
    {
        public ModificationType ForcedType { get; }

        public LockModifierTypeAttribute (ModificationType forcedType = ModificationType.RawValue)
            => ForcedType = forcedType;
    }

    /// <summary>
    /// Turns the stat string into a drop-down list based on the passed Enum.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class StatEnumAttribute : Attribute
    {
        public Type EnumType { get; }

        public StatEnumAttribute (Type enumType)
        {
            if (enumType.IsEnum == false)
                throw new ArgumentException("Type must be an Enum");
            EnumType = enumType;
        }
    }
}