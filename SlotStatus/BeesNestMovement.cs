using System;
using System.Collections;
using System.Collections.Generic;
using Monarchs.Logic;
using UnityEngine;

namespace Monarchs
{
    [CreateAssetMenu(fileName = "slotstatusdata", menuName = "TcgEngine/SlotStatusMovement/BeesNestMovement", order = 10)]
    public class BeesNestMovement : SlotStatusMovement
    {
        public override void DoMovement(GameLogic logic, SlotStatus slotStatus, List<Card> endTurnCard)
        {
            if (!endTurnCard.Contains(slotStatus.triggerer))
                return;
            
            Slot startSlot = slotStatus.slot;
            Slot triggererSlot = slotStatus.triggerer.slot;
            
            int deltaX = Math.Sign(triggererSlot.x - startSlot.x);
            int deltaY = Math.Sign(triggererSlot.y - startSlot.y);
            
            Slot targetSlot = Slot.Get(startSlot.x + deltaX, startSlot.y + deltaY);

            slotStatus.slot = targetSlot;
        }
    }
}
