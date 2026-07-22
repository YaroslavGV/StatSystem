namespace StatSystem
{
    using System.Collections.Generic;

    /// <summary>
    /// Acts as a data provider that turns any gameplay object (item, buff, level profile) 
    /// into a valid source of stat modifications.
    /// Serves as a unique identifier reference used for subsequent bulk removal of effects.
    /// </summary>
    public interface IModifierSource
    {
        IEnumerable<IModifier> GetModifiers ();
    }
}