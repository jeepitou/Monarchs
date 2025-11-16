using System.Collections;
using System.Collections.Generic;
using Monarchs.Logic;
using UnityEngine;

namespace Monarchs
{
    public class SlotStatusTrigger
    {
        public void TriggerAllSlotStatus(GameLogic logic, Game game, SlotStatusTriggerType trigger)
        {
            foreach (var slotStatus in game.slotStatusList)
            {
                if (slotStatus.SlotStatusData.type == SlotStatusType.ModifyMoveDestination)
                {
                    return;
                }
                
                if (slotStatus.SlotStatusData.triggerType.HasFlag(trigger))
                {
                    if (trigger == SlotStatusTriggerType.OnTurnStart)
                    {
                        Card target = logic.Game.GetSlotCard(slotStatus.slot);
                        if (target != null && game.GetCurrentCardTurn().Contains(target))
                        {
                            TriggerSlotStatus(logic, game, trigger, slotStatus);
                        }
                    }
                    else
                    {
                        TriggerSlotStatus(logic, game, trigger, slotStatus);
                    }
                }
            }
        }

        public void TriggerAllSlotStatusOnMovePath(GameLogic logic, Game game, Slot startSlot, Slot endSlot, Card target)
        {
            foreach (var status in game.slotStatusList)
            {
                if (status.SlotStatusData.type == SlotStatusType.ModifyMoveDestination)
                {
                    return;
                }
                
                bool isOnPath = endSlot == status.slot || Slot.IsBetween(startSlot, endSlot, status.slot);
                
                if (isOnPath && status.SlotStatusData.triggerType.HasFlag(SlotStatusTriggerType.WalkedOnSlot))
                {
                    TriggerSlotStatus(logic, game, SlotStatusTriggerType.WalkedOnSlot, status, target);
                }
            }
        }

        public void TriggerSlotStatus(GameLogic logic, Game game, SlotStatusTriggerType trigger, SlotStatus status)
        {
            if (status.SlotStatusData.triggerType.HasFlag(trigger))
            {
                Card target = logic.Game.GetSlotCard(status.slot);
                if (target != null)
                {
                    status.SlotStatusData.DoEffect(logic, target, status.slot, status.slot);
                }
            }
        }
        
        public void TriggerSlotStatus(GameLogic logic, Game game, SlotStatusTriggerType trigger, SlotStatus status, Card target)
        {
            if (status.SlotStatusData.triggerType.HasFlag(trigger))
            {
                if (target != null)
                {
                    status.SlotStatusData.DoEffect(logic, target, status.slot, status.slot);
                }
            }
        }
    }
}
