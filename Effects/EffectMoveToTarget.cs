using System.Collections;
using System.Collections.Generic;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using TcgEngine;
using UnityEngine;

namespace Monarchs
{
    /// <summary>
    /// This is used for the smoke steed ability.
    /// It gets the transported ally card from current game state, and calculate it's new position.
    /// It then moves smoke steed and ally.
    /// </summary>
    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/EffectMoveToTarget", order = 10)]
    public class EffectMoveToTarget : EffectData
    {
        public int target_number;
        public override void DoEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            Slot slotTarget = logic.Game.selectorTargets[target_number - 1].GetSlot();
            logic.ForceMoveCard(args.CardTarget, slotTarget, true, false);
        }
    }
}
