namespace StatSystem
{
    /// <summary>
    /// Declares the contract for a single stat modifier.
    /// Used for both permanent bonuses (equipment, passives) and temporary effects (buffs, debuffs, auras).
    /// </summary>
    public interface IModifier
    {
        string StatId { get; }
        ModificationType Type { get; }
        float Value { get; }
        IModifierSource Source { get; }
    }
}