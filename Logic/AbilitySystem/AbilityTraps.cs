using System.Collections.Generic;
using Monarchs.Ability;
using TcgEngine;
using UnityEngine.Events;

namespace Monarchs.Logic.AbilitySystem
{
    public class AbilityTraps
    {
        public AbilityLogicSystem logic;
        
        public AbilityTraps(AbilityLogicSystem logicSystem)
        {
            logic = logicSystem;
        }
        
        public virtual Card GetTrapOnSlot(Game game, Card card, Slot slotToCheck)
        {
            foreach (var player in game.players)
            {
                foreach (var trap in player.cards_trap)
                {
                    if (trap.CardData.cardType == CardType.Trap && !trap.exhausted)
                    {
                        if (trap.CardData.AreTrapConditionsMet(AbilityTrigger.OnMoveOnSpecificSquare, game, trap,
                                card) && trap.slot == slotToCheck)
                        {
                            return trap;
                        }
                    }
                }
            }
            return null;
        }
        
        public virtual List<Slot> GetTrapsOnMovePath(Game game, Card movingCard, Slot destinationSlot)
        {
            List<Slot> trapsOnMovementPath = new List<Slot>();
            foreach (var player in game.players)
            {
                foreach (var card in player.cards_trap)
                {
                    if (card.CardData.cardType == CardType.Trap && !card.exhausted)
                    {
                        if (card.CardData.AreTrapConditionsMet(AbilityTrigger.OnMoveOnSpecificSquare, game, card, movingCard) && 
                            card.IsTrapOnMovementPath(movingCard, destinationSlot))
                        {
                            trapsOnMovementPath.Add(card.slot);
                        }
                    }
                }
            }

            return trapsOnMovementPath;
        }
        
        /// <summary>
        /// This trigger the closest traps on the move path. This needs to be called before the piece is actually moved.
        /// </summary>
        public virtual void TriggerClosestTrapOnMovePath(Game game, Card movingCard, Slot destinationSlot, List<Slot> trapsOnMovementPath)
        {
            Slot closestTrapSlot = GetClosest(movingCard.slot, trapsOnMovementPath);

            Card closestTrap = game.GetSlotTrap(closestTrapSlot);
            
            logic.OnTrapTrigger(closestTrap, movingCard);
        }

        public virtual void TriggerTrapOnSlot(Game game, Card trappedCard, Card trap)
        {
            logic.OnTrapTrigger(trap, trappedCard);
        }

        protected virtual Slot GetClosest(Slot slot, List<Slot> slotList)
        { // For mocking
            return Slot.GetClosest(slot, slotList);
        }
    }
}