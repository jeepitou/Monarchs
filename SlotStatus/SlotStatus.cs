using System.Collections;
using System.Collections.Generic;
using Monarchs.Logic;
using TcgEngine;
using UnityEngine;

[System.Serializable]
public class SlotStatus
{
    public Slot slot;
    public Card triggerer;
    public string slotID;
    public string uid;
    public int turnsLeft = -1;
    public bool showOnBoard = true;
    
    [System.NonSerialized]private SlotStatusData _slotStatusData = null;

    public SlotStatus(SlotStatusData data, Slot slot, bool showOnBoard = true, Card triggerer = null)
    {
        _slotStatusData = data;
        slotID = data.id;
        this.slot = slot;
        this.uid = GameTool.GenerateRandomID(11,15);
        this.showOnBoard = showOnBoard;
        this.triggerer = triggerer;
        if (data.duration == SlotStatusDuration.Temporary)
            turnsLeft = data.durationTurns;
        
    }
    
    public SlotStatusData SlotStatusData 
    { 
        get { 
            if(_slotStatusData == null || _slotStatusData?.id != slotID)
                _slotStatusData = SlotStatusData.Get(slotID); //Optimization, store for future use
            return _slotStatusData;
        } 
    }
    
    public void DoMovement(GameLogic logic, List<Card> endTurnCard)
    {
        if (SlotStatusData.movement == null)
            return;
        SlotStatusData.movement.DoMovement(logic, this, endTurnCard);
    }

    public void ReduceDuration()
    {
        if (_slotStatusData.duration == SlotStatusDuration.Temporary)
        {
            turnsLeft--;
        }
        
    }
}
