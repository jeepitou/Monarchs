using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Ability.Target;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using TcgEngine;

namespace Ability.Target
{
    public class AbilityTargetSelf: AbilityTarget
    {
        public override List<ITargetable> GetAllTargets(Game data, AbilityArgs args,
            ListSwap<ITargetable> memoryArray = null)
        {
            if (memoryArray == null)
                memoryArray = new ListSwap<ITargetable>(); //Slow operation

            List<ITargetable> targets = memoryArray.Get();
            targets.Add(args.caster);

            return targets;
        }
    }
}