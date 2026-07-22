using System;

namespace StatSystem
{
    /// <summary>
    /// Extends the contract for modifiers whose value can dynamically change at runtime 
    /// (e.g., scale based on time, health missing, or proximity alerts).
    /// </summary>
    public interface INotifiableModifier : IModifier
    {
        /// <summary> Triggered whenever the internal logic updates the modifier's Value. </summary>
        event Action<IModifier> OnValueChanged;
    }
}