using System;
using System.Collections.Generic;

namespace StatSystem
{
    /// <summary>
    /// An architectural mediator encapsulating the storage and management of modifier sources.
    /// Implemented by major subsystems (inventory, buff manager) to reactively notify 
    /// the stat system when sources are added, removed, or fully reset.
    /// </summary>
    public interface IModifierAggregator
    {
        event Action<IModifierSource> OnSourceAdded;

        event Action<IModifierSource> OnSourceRemoved;
        /// <summary>
        /// Triggered during mass structural changes (e.g., loading a save game, clearing all debuffs, 
        /// swapping equipment sets). Forces the tracking linker to fully rebuild the stat values from scratch.
        /// </summary>

        event Action OnAllSourcesUpdated;

        IEnumerable<IModifierSource> GetModifierSources ();
    }
}