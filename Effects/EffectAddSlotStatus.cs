using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TcgEngine
{
    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/AddSlotStatus", order = 10)]
    public class EffectAddSlotStatus : EffectData
    {
        public SlotStatusData slotStatus;
        public bool showVFXOnlyOnMainTarget = true;
        public override void DoEffectSlotTarget(GameLogic logic, AbilityArgs args)
        {
            bool showVFX = !showVFXOnlyOnMainTarget || args.target.GetSlot() == args.castedCard.slot;
            logic.AddSlotStatus((Slot)args.target, slotStatus, showVFX, (Card)args.triggerer);
        }
    }
}