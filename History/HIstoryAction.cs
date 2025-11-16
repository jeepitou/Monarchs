using System.Collections.Generic;
using Monarchs.Ability.Target;
using Monarchs.Logic;
using UnityEngine.Serialization;

namespace Monarchs
{
    [System.Serializable]
    public class HistoryAction
    {
        public ushort type;
        public int playerId;
    }

    [System.Serializable]
    public class HistoryMoveAction : HistoryAction
    {
        public string cardUID;
        public Slot startSlot;
        public Slot endSlot;
    }

    [System.Serializable]
    public class HistoryAttackAction : HistoryAction
    {
        public string cardUID;
        public string targetUID;
        public Slot startSlot;
        public Slot targetSlot;
        public int damage;
    }
    
    [System.Serializable]
    public class HistoryCardPlayedAction : HistoryAction
    {
        public string casterUID;
        public string cardPlayedUID;
        public Slot casterSlot;
        public List<Slot> targetSlots;
        public string targetUID;
    }
    
    [System.Serializable]
    public class HistoryAbilityCastAction : HistoryAction
    {
        public string casterUID;
        public Slot casterSlot;
        public string abilityUID;
        public List<Slot> targetSlots;
        public string targetUID;
    }
}