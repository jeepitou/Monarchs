using System.Collections.Generic;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using TcgEngine;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
[CreateAssetMenu(fileName = "SlotStatusData", menuName = "TcgEngine/SlotStatus/SlotStatusData", order = 10)]
public class SlotStatusData : ScriptableObject
{
    public string id;
    public SlotStatusType type;
    [FormerlySerializedAs("trigger")] public SlotStatusTriggerType triggerType;
    public SlotStatusDuration duration;
    public int durationTurns;
    public EffectData[] effects;              //WHAT this does?
    public int value;
    
    public StatusData[] statusArray;               //Status added by this ability  
    public SlotStatusMovement movement;            //Movement added by this ability
    public GameObject VFX;
    public static List<SlotStatusData> slotStatusDataList = new List<SlotStatusData>();

    public static void Load(string folder = "")
    {
        if (slotStatusDataList.Count == 0)
            slotStatusDataList.AddRange(Resources.LoadAll<SlotStatusData>(folder));
    }
    
    public void DoOngoingEffect(GameLogic logic, Slot slotWithStatus)
    {
        if (triggerType != SlotStatusTriggerType.Ongoing)
            return;
        
        Card card = logic.Game.GetSlotCard(slotWithStatus);
        if (card == null)
            return;
        foreach (var effect in effects)
        {
            effect.DoOngoingEffect(logic, new AbilityArgs(){target=card});
        }
        
        foreach (var status in statusArray)
        {
            card.AddStatus(status, 1, -1);
        }
    }
    
    public void DoEffect(GameLogic logic, Card card, Slot slotWithStatus, Slot destinationSlot)
    {
        foreach (var effect in effects)
        {
            effect.DoEffect(logic, this, card, slotWithStatus, destinationSlot);
        }

        foreach (var status in statusArray)
        {
            card.AddStatus(status, 1, 1);
        }
    }
    
    
    
    /// <summary>
    /// This methods returns the modified destination after applying the slot status. Used to show correct
    /// destination when hovering on a move
    /// </summary>
    public virtual Slot GetModifiedDestination(GameLogic logic, Card card, Slot destinationSlot)
    {
        Slot slot = card.slot;
        GameLogic fakeLogic = new GameLogic(true);
        Game fakeGame = Game.CloneNew(logic.Game);
        Card fakeCard = Card.CloneNew(card);
        fakeLogic.SetData(fakeGame);
        SlotStatus slotStatus = fakeGame.GetSlotStatus(slot);
        
        if (IsSlotStatusMovement(slotStatus))
        {
            return RecursiveSlotStatus(fakeLogic, fakeGame, fakeCard, slot, destinationSlot, this);
        }
        else
        {
            slot = GetClosestSlotWithMovementStatus(fakeLogic, fakeGame, card.slot, destinationSlot);
            if (slot != Slot.None)
            {
                return RecursiveSlotStatus(fakeLogic, fakeGame, fakeCard, slot, destinationSlot, this);
            }
        }

        return destinationSlot;
    }

    public static SlotStatusData Get(string id)
    {
        foreach (SlotStatusData slotStatusData in GetAll())
        {
            if (slotStatusData.id == id)
                return slotStatusData;
        }
        return null;
    }
    
    public static List<SlotStatusData> GetAll()
    {
        return slotStatusDataList;
    }

    private Slot RecursiveSlotStatus(GameLogic fakeLogic, Game fakeGame, Card fakeCard, Slot currentSlotWithStatus, Slot destinationSlot, SlotStatusData slotStatus)
    {
        if (fakeCard.HasStatus(StatusType.Stunned))
        {
            return currentSlotWithStatus;
        }

        slotStatus.DoEffect(fakeLogic, fakeCard, currentSlotWithStatus, destinationSlot);
        
        if (fakeCard.slot == currentSlotWithStatus) //if fakeCard.Slot is the slotWithStatus, after we DoEffect, it means the SlotStatus didn't move the piece.
        {
            return currentSlotWithStatus;
        }
        
        Slot nextSlotWithStatus = GetClosestSlotWithMovementStatus(fakeLogic, fakeGame, currentSlotWithStatus, fakeCard.slot);

        if (nextSlotWithStatus != Slot.None)
        {
            if (fakeCard.slot == nextSlotWithStatus)
            {
                fakeCard.slot = currentSlotWithStatus; // I reset the slot to the last slot with status we stepped on so that the effect can know from which direction we're coming
                return RecursiveSlotStatus(fakeLogic, fakeGame, fakeCard, nextSlotWithStatus, nextSlotWithStatus, fakeGame.GetSlotStatus(nextSlotWithStatus).SlotStatusData);
            }
            return RecursiveSlotStatus(fakeLogic, fakeGame, fakeCard, nextSlotWithStatus, fakeCard.slot, fakeGame.GetSlotStatus(nextSlotWithStatus).SlotStatusData);
        }

        return fakeCard.slot;
    }

    private Slot GetClosestSlotWithMovementStatus(GameLogic logic, Game game, Slot slot1, Slot slot2)
    {
        SlotStatus newSlotStatus = null;
        if (!slot1.IsInDistance(slot2, 1)) // If we are further than one square away, we validate each square
        {
            foreach (Slot slotInBetween in Slot.GetSlotInBetween(slot1, slot2))
            {
                newSlotStatus = game.GetSlotStatus(slotInBetween);
                
                if (IsSlotStatusMovement(newSlotStatus))
                {
                    return slotInBetween;
                }
            }
        }
        
        newSlotStatus = game.GetSlotStatus(slot2);
        
        if (IsSlotStatusMovement(newSlotStatus))
        {
            return slot2;
        }

        return Slot.None;
    }

    private bool IsSlotStatusMovement(SlotStatus slotStatus)
    {
        if (slotStatus != null)
        {
            if (slotStatus.SlotStatusData.type == SlotStatusType.ModifyMoveDestination)
            {
                return true;
            }
        }

        return false;
    }
    
}

[System.Serializable]
public enum SlotStatusType
{
    ModifyMoveDestination,
    OngoingEffect,
    Other
}

[System.Flags] [System.Serializable]
public enum SlotStatusTriggerType
{
    WalkedOnSlot = 1 << 1,
    FinishMoveOnSlot = 1 << 2,
    OnSlotStatusSpawn = 1 << 3,
    Ongoing = 1 << 4,
    OnTurnStart = 1 << 5,
}

[System.Serializable]
public enum SlotStatusDuration
{
    ActivateOnce,
    Permanent,
    Temporary
}


