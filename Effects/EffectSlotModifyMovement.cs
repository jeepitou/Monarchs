using System;
using System.Collections;
using System.Collections.Generic;
using Monarchs.Logic;
using TcgEngine;
using UnityEngine;

[CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/SlotModifyMovement", order = 10)]
public class EffectSlotModifyMovement : EffectData
{
    public MovementModification movementModification;
    public int value; //only used for reducing and extending movement
    
    public override void DoEffect(GameLogic logic, SlotStatusData slotStatusData, Card caster, Slot slotWithStatus, Slot destinationSlot)
    {
        if (caster.HasStatus(StatusType.Stunned) || caster.GetPieceType() == PieceType.Knight)
        {
            return;
        }
        
        if (movementModification == MovementModification.ExtendOrReduceMovement)
        {
            Slot newDestination = ModifyMovement(caster.slot, destinationSlot, value); 
            if (logic.Game.GetSlotCard(newDestination) == null) //This will not work if a status modify movement by more than one square.
            {
                caster.slot = newDestination;
            }
            else
            {
                caster.slot = destinationSlot;
            }
            
            return;
        }

        if (movementModification == MovementModification.StopsMovement)
        {
            caster.AddStatus(StatusType.Stunned, 1, 1);
            return;
        }

        if (movementModification == MovementModification.PreventsMovement)
        {
            Slot newDestination = ModifyMovement(caster.slot, slotWithStatus, -1);
            caster.slot = newDestination;
            caster.AddStatus(StatusType.Stunned, 1, 1);
            return;
        }
    }

    private Slot ModifyMovement(Slot slot1, Slot slot2, int modif)
    {
        int deltaX = slot2.x - slot1.x;
        int deltaY = slot2.y - slot1.y;
        int signX = Math.Sign(deltaX);
        int signY = Math.Sign(deltaY);
        int X = 0;
        int Y = 0;

        // Horizontal
        if (deltaY == 0)
        {
            Y = slot1.y;
            X = slot1.x + (Math.Abs(deltaX) + modif) * signX;
        }
            
        // Vertical
        if (deltaX == 0)
        {
            X = slot1.x;
            Y = slot1.y + (Math.Abs(deltaY) + modif) * signY;
        }
            
        //Diagonal
        if (Math.Abs(deltaX) == Math.Abs(deltaY))
        {
            X = slot1.x + (Math.Abs(deltaX) + modif) * signX;
            Y = slot1.y + (Math.Abs(deltaY) + modif) * signY;
        }

        return Slot.KeepMovementOnBoard(slot1, Slot.Get(X, Y));
    }

    public enum MovementModification
    {
        ExtendOrReduceMovement, // Movement further or less on trajectory (slippery tile / rough terrain)
        StopsMovement, // Stops movement when you walk on tile (kinda like a trap)
        PreventsMovement, // Movement is impossible on the tile 
    }
}
