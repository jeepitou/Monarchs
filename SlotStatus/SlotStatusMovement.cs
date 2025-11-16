using System.Collections.Generic;
using Monarchs.Logic;
using UnityEngine;

public abstract class SlotStatusMovement : ScriptableObject
{
    public abstract void DoMovement(GameLogic logic, SlotStatus slotStatus, List<Card> endTurnCard);
}