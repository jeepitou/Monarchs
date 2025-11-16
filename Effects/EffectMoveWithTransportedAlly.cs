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
    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/EffectMoveWithTransportedAlly", order = 10)]
    public class EffectMoveWithTransportedAlly : EffectData
    {
        public override void DoEffectSlotTarget(GameLogic logic, AbilityArgs args)
        {
            Card allyToTransport = logic.Game.savedTargetForAbility as Card;
            
            Slot currentAllySlot = allyToTransport.slot;
            Vector2S offset = currentAllySlot.GetCoordinate() - args.caster.slot.GetCoordinate();
            Vector2S newAllyPosition = args.SlotTarget.GetCoordinate() + offset;
            Slot newAllySlot = Slot.Get(newAllyPosition);
            
            logic.ForceMoveCard(args.caster, args.SlotTarget, true);
            logic.ForceMoveCard(allyToTransport, newAllySlot, true, false);
        }
    }
}
