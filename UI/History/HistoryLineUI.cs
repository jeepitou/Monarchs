using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Client;
using Monarchs.Logic;
using UnityEngine;

namespace Monarchs
{
    public class HistoryLineUI : MonoBehaviour
    {
        public GameObject cardPrefab;
        public GameObject abilityPrefab;
        public GameObject iconPrefab;
        public GameObject coordinatePrefab;
        public GameObject damagePrefab;
        public int index;
        public HistoryTextParser parser;
        
        public void SetText(HistoryAction history)
        {
            bool isPlayerMove = history.playerId == GameClient.Get().GetPlayerID();
            
            if (history.type == GameAction.Move)
            {
                SetMoveHistory(history);
                return;
            }
            if (history.type == GameAction.Attack)
            {
                SetAttackHistory(history, false);
                return;
            }
            if (history.type == GameAction.RangeAttack)
            {
                SetAttackHistory(history, true);
                return;
            }
            if (history.type == GameAction.CardPlayed)
            {
                SetCardPlayedHistory(history);
                return;
            }

            if (history.type == GameAction.CastAbility)
            {
                SetAbilityHistory(history);
            }
        }

        private void SetMoveHistory(HistoryAction history)
        {
            HistoryMoveAction historyMove = history as HistoryMoveAction;
            
            AddCard(historyMove.cardUID, historyMove.startSlot);
            AddMoveIcon();
            AddCoordinate(historyMove.endSlot);
        }

        private void SetAttackHistory(HistoryAction history, bool rangedAttack)
        {
            HistoryAttackAction historyAttack = history as HistoryAttackAction;
            
            AddCard(historyAttack.cardUID, historyAttack.startSlot);
            AddAttackIcon(rangedAttack);
            AddCard(historyAttack.targetUID, historyAttack.targetSlot);
            AddDamage(historyAttack.damage);
        }
        
        private void SetCardPlayedHistory(HistoryAction history)
        {
            HistoryCardPlayedAction historyCardPlayed = history as HistoryCardPlayedAction;
            
            AddCard(historyCardPlayed.casterUID, historyCardPlayed.casterSlot);
            AddCardPlayedIcon();
            AddCard(historyCardPlayed.cardPlayedUID, historyCardPlayed.targetSlots);
        }
        
        private void SetAbilityHistory(HistoryAction history)
        {
            HistoryAbilityCastAction historyAbility = history as HistoryAbilityCastAction;
            
            AddCard(historyAbility.casterUID, historyAbility.casterSlot);
            AddAbility(historyAbility.abilityUID, historyAbility.targetSlots);
        }

        private void AddCard(string uid, Slot slot)
        {
            Card card = GameClient.GetGameData().GetCard(uid);
            HistoryCard historyCard = Instantiate(cardPrefab, transform).GetComponent<HistoryCard>();
            historyCard.SetCard(card, slot);
        }
        
        private void AddCard(string uid, List<Slot> slots)
        {
            Card card = GameClient.GetGameData().GetCard(uid);
            HistoryCard historyCard = Instantiate(cardPrefab, transform).GetComponent<HistoryCard>();
            historyCard.SetCard(card, slots);
        }
        
        private void AddAbility(string uid, List<Slot> slots)
        {
            AbilityData ability = AbilityData.Get(uid);
            HistoryCard historyCard = Instantiate(abilityPrefab, transform).GetComponent<HistoryCard>();
            historyCard.SetAbility(ability, slots);
        }

        private void AddAttackIcon(bool ranged = false)
        {
            if (ranged)
            {
                AddRangedAttackIcon();
                return;
            }
            
            HistoryIcon icon = Instantiate(iconPrefab, transform).GetComponent<HistoryIcon>();
            icon.SetIcon(HistoryIcon.HistoryIconType.Attack);
        }
        
        private void AddRangedAttackIcon()
        {
            HistoryIcon icon = Instantiate(iconPrefab, transform).GetComponent<HistoryIcon>();
            icon.SetIcon(HistoryIcon.HistoryIconType.RangedAttack);
        }
        
        private void AddMoveIcon()
        {
            HistoryIcon icon = Instantiate(iconPrefab, transform).GetComponent<HistoryIcon>();
            icon.SetIcon(HistoryIcon.HistoryIconType.Move);
        }
        
        private void AddCardPlayedIcon()
        {
            HistoryIcon icon = Instantiate(iconPrefab, transform).GetComponent<HistoryIcon>();
            icon.SetIcon(HistoryIcon.HistoryIconType.CardPlayed);
        }

        private void AddCoordinate(Slot slot)
        {
            var coordinate = Instantiate(coordinatePrefab,transform).GetComponent<HistoryCoordinate>();
            coordinate.SetCoordinates(slot);
        }

        private void AddDamage(int damage)
        {
            HistoryDamage historyDamage = Instantiate(damagePrefab, transform).GetComponent<HistoryDamage>();
            historyDamage.SetDamage(damage);
        }
        
    }
}
