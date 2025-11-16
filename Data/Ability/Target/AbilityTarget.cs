using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Ability.Target;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using TcgEngine;

namespace Ability.Target
{
    public class AbilityTarget
    {
        public virtual List<ITargetable> GetAllTargets(Game data, AbilityArgs args,
            ListSwap<ITargetable> memoryArray = null)
        {
            return null;
        }
    }
}