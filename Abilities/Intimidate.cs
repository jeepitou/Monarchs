using System;
using System.Collections.Generic;
using System.Linq;
using Monarchs.Ability.Target;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;

namespace Monarchs.Abilities
{
    [Serializable]
    public class Intimidate
    {
        public List<Card> _cardsOnBoardWithIntimidate = new List<Card>();

        public bool AbilityMustTargetIntimidatorInsteadOfCurrentTarget(Game game, AbilityArgs args)
        {
            Card target = null;
            if (args.target is Card)
            {
                target = (Card)args.target;
            }
            else if (args.target is Slot)
            {
                target = game.GetSlotCard((Slot) args.target);
            }

            if (target == null || _cardsOnBoardWithIntimidate.Contains(target))
            {
                return false;
            }

            if (target.playerID == args.caster.playerID)
            {
                return false;
            }

            foreach (var intimidator in _cardsOnBoardWithIntimidate)
            {
                if (intimidator.playerID != args.caster.playerID)
                {
                    args.target = intimidator;
                    if (args.ability.CanTarget(game, args))
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
        
        public void AddCardWithIntimidation(Card card)
        {
            if (!ValidateIfCardIsInList(card) && card != null)
            {
                _cardsOnBoardWithIntimidate.Add(card);
            }
        }

        public void RemoveCardWithIntimidation(Card card)
        {
            Card cardInList = FindCardInList(card);

            if (cardInList != null)
            {
                _cardsOnBoardWithIntimidate.Remove(cardInList);
            }
        }

        public List<ITargetable> FilterIntimidate(Game game, int currentPlayerId, List<ITargetable> targets)
        {
            if (targets.Count == 0)
            {
                return targets;
            }
            
            if (targets[0] is Card)
            {
                return FilterIntimidateCards(game, currentPlayerId, targets.Cast<Card>().ToList());
            }
            else if (targets[0] is Slot)
            {
                return FilterIntimidateSlots(game, currentPlayerId, targets.Cast<Slot>().ToList());
            }

            return targets;
        }
        
        public List<Vector2S> FilterIntimidate(Game game, int currentPlayerId, List<Vector2S> targets)
        {
            List<Slot> slotList = Slot.GetSlotListFromCoordinates(targets);

            slotList = FilterIntimidateSlots(game, currentPlayerId, slotList).Cast<Slot>().ToList();

            return Slot.GetCoordinateListFromSlots(slotList);
        }

        public Intimidate Clone()
        {
            Intimidate clone = new Intimidate();

            clone._cardsOnBoardWithIntimidate = this._cardsOnBoardWithIntimidate;
            return clone;
        }

        private List<ITargetable> FilterIntimidateCards(Game game, int currentPlayerId, List<Card> targets)
        {
            List<Card> opponentIntimidators = GetOpponentIntimidators(currentPlayerId, targets);
            
            if (opponentIntimidators.Count == 0 )
            {
                return targets.Cast<ITargetable>().ToList();
            }
            
            List<ITargetable> returnList = new List<ITargetable>(targets.Cast<ITargetable>());
            foreach (var target in targets)
            {
                if (opponentIntimidators.Contains(target))
                {
                    continue;
                }

                if (target != null && target.playerID != currentPlayerId)
                {
                    returnList.Remove(target);
                }
            }

            return returnList;
        }
        
        private List<ITargetable> FilterIntimidateSlots(Game game, int currentPlayerId, List<Slot> targets)
        {
            List<Slot> opponentIntimidators = GetOpponentIntimidators(currentPlayerId, targets);
            
            if (opponentIntimidators.Count() == 0 )
            {
                return targets.Cast<ITargetable>().ToList();
            }

            List<ITargetable> returnList = new List<ITargetable>(targets.Cast<ITargetable>());
            foreach (var target in targets)
            {
                if (opponentIntimidators.Contains(target))
                {
                    continue;
                }
                
                Card slotCard = game.GetSlotCard(target);
                if (slotCard != null && slotCard.playerID != currentPlayerId)
                {
                    returnList.Remove(target);
                }
            }

            return returnList;
        }

        private List<Card> GetOpponentIntimidators(int currentPlayerId, List<Card> cards)
        {
            List<Card> returnList = new List<Card>();

            foreach (var card in _cardsOnBoardWithIntimidate)
            {
                if (card.playerID != currentPlayerId && cards.Contains(card))
                {
                    returnList.Add(card);
                }
            }

            return returnList;
        }
        
        private List<Slot> GetOpponentIntimidators(int currentPlayerId, List<Slot> slots)
        {
            List<Slot> returnList = new List<Slot>();

            foreach (var card in _cardsOnBoardWithIntimidate)
            {
                if (card.playerID != currentPlayerId && slots.Contains(card.slot))
                {
                    returnList.Add(card.slot);
                }
            }

            return returnList;
        }
        
        

        private Card FindCardInList(Card card)
        {
            foreach (var cardInList in _cardsOnBoardWithIntimidate)
            {
                if (card.uid == cardInList.uid)
                {
                    return cardInList;
                }
            }

            return null;
        }
        
        private bool ValidateIfCardIsInList(Card card)
        {
            return FindCardInList(card) != null;
        }
    }
}