using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Ability.Target;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using TcgEngine;

namespace Ability.Target
{
    public class AbilityTargetSelectManaType : AbilityTarget
    {
        //When dealing with mana, the real target is one of the player. 
        public override List<ITargetable> GetAllTargets(Game data, AbilityArgs args, ListSwap<ITargetable> memoryArray = null)
        {
            return new List<ITargetable>();
        }
    }
}
