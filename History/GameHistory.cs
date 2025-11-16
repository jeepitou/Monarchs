using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Ability.Target;
using Monarchs.Logic;
using UnityEngine;

namespace Monarchs
{
    [System.Serializable]
    public class GameHistory
    {
        public List<HistoryAction> historyList = new ();

        public void AddMoveHistory(int playerId, Card card, Slot startSlot, Slot endSlot)
        {
            HistoryMoveAction order = new HistoryMoveAction();
            order.playerId = playerId;
            order.type = GameAction.Move;
            order.cardUID = card.uid;
            order.startSlot = startSlot;
            order.endSlot = endSlot;
            historyList.Add(order);
        }
        
        public void AddMeleeAttackHistory(int playerId, Card card, Card target, Slot startSlot, int damage)
        {
            HistoryAttackAction order = new HistoryAttackAction();
            order.playerId = playerId;
            order.type = GameAction.Attack;
            order.cardUID = card.uid;
            order.targetUID = target.uid;
            order.startSlot = startSlot;
            order.targetSlot = target.slot;
            order.damage = damage;
            historyList.Add(order);
        }
        
        public void AddRangeAttackHistory(int playerId, Card card, Card target, Slot targetSlot, Slot startSlot, int damage)
        {
            HistoryAttackAction order = new HistoryAttackAction();
            order.playerId = playerId;
            order.type = GameAction.RangeAttack;
            order.cardUID = card.uid;
            order.targetUID = target != null ? target.uid : "";
            order.startSlot = startSlot;
            order.targetSlot = targetSlot;
            order.damage = damage;
            historyList.Add(order);
        }

        public void AddCardPlayedHistory(int playerId, Card caster, Card card, Card target, Slot casterSlot, List<Slot> targetSlots)
        {
            HistoryCardPlayedAction order = new HistoryCardPlayedAction();
            order.playerId = playerId;
            order.type = GameAction.CardPlayed;
            order.cardPlayedUID = card.uid;
            order.casterUID = caster.uid;
            if (target != null)
            {
                order.targetUID = target.uid;
            }
            order.casterSlot = casterSlot;
            order.targetSlots = targetSlots;
            
            historyList.Add(order);
        }
        
        public void AddAbilityCastHistory(int playerId, Card caster, AbilityData ability, Slot casterSlot, List<Slot> targetSlots)
        {
            if (ability.DontShowInHistory)
            {
                return;
            }
            HistoryAbilityCastAction order = new HistoryAbilityCastAction();
            order.playerId = playerId;
            order.type = GameAction.CastAbility;
            order.abilityUID = ability.id;
            order.casterUID = caster.uid;
            order.casterSlot = casterSlot;
            order.targetSlots = targetSlots;
            
            historyList.Add(order);
        }
        
        public GameHistory Clone()
        {
            GameHistory clone = new GameHistory();
            foreach (HistoryAction action in historyList)
            {
                clone.historyList.Add(action);
            }
            return clone;
        }
    }
}
