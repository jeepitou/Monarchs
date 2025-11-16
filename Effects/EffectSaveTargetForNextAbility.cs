using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Effect that adds or removes basic card/player stats such as hp, attack, mana
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/EffectSaveTargetForNextAbility", order = 10)]
    public class EffectSaveTargetForNextAbility : EffectData
    {
        public override void DoEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            logic.Game.savedTargetForAbility = args.CardTarget;
        }
        
        public override void DoEffectSlotTarget(GameLogic logic, AbilityArgs args)
        {
            logic.Game.savedTargetForAbility = args.SlotTarget;
        }
    }
}