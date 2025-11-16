using System.Collections;
using System.Collections.Generic;
using Monarchs.Logic;
using TcgEngine;
using UnityEngine;

public class BoardSlotStatus : MonoBehaviour
{
    public static List<BoardSlotStatus> allSlotStatus = new List<BoardSlotStatus>();
    public string uid;
    public Slot slot;
    
    protected virtual void Awake()
    {
        allSlotStatus.Add(this);
    }
        
    protected virtual void OnDestroy()
    {
        allSlotStatus.Remove(this);
    }
    
    public void SetSlotStatus(SlotStatus slotStatus)
    {
        uid = slotStatus.uid;
        slot = slotStatus.slot;
    }
    
    
    public static BoardSlotStatus Get(string uid)
    {
        foreach (BoardSlotStatus slotStatus in allSlotStatus)
        {
            if (slotStatus.uid == uid)
                return slotStatus;
        }
        return null;
    }
}
