using System.Collections;
using System.Collections.Generic;
using Monarchs.Logic;
using UnityEngine;

namespace Monarchs
{
    public class RabiesAI
    {
        public void DoTurn(GameLogic logic, Game game, Card cardWithRabies)
        {
            bool rangeAttack = false;
            
            List<Card> validTargets = GetMeleeTargetsAtRange1(game, cardWithRabies);
            
            if (validTargets.Count == 0)
            {
                validTargets = GetRangedTargets(game, cardWithRabies);
                rangeAttack = true;
            }
            
            if (validTargets.Count == 0)
            {
                validTargets = GetMeleeTargets(game, cardWithRabies);
                rangeAttack = false;
            }
            
            if (validTargets.Count == 0)
            {
                logic.EndTurn();
                return;
            }
            
            int minDistance = GetMinimumDistanceToTargetInList(validTargets, cardWithRabies);
            var targetsAtMinDistance = GetTargetsAtMinimumDistance(validTargets, cardWithRabies, minDistance);
            var targetsWithLowestHealth = GetTargetsWithLowestHealth(targetsAtMinDistance);
            var target = GetTargetWithLowestHealth(targetsWithLowestHealth);
            
            
            logic.AttackTarget(cardWithRabies, target.slot, false, rangeAttack, true);
        }
        
        private List<Card> GetMeleeTargetsAtRange1(Game game, Card cardWithRabies)
        {
            List<Card> meleeTargets = new List<Card>();
            var validCoordinates = cardWithRabies.GetCurrentMovementScheme().GetLegalMeleeAttack(
                cardWithRabies.slot.GetCoordinate(), 
                1, 
                cardWithRabies.CanJump(), 
                cardWithRabies.playerID, 
                game, 
                true);
            
            return game.GetCardsOnCoordinates(validCoordinates);
        }
        
        private List<Card> GetRangedTargets(Game game, Card cardWithRabies)
        {
            List<Card> rangedTargets = new List<Card>();
            var validCoordinates = cardWithRabies.GetCurrentMovementScheme().GetLegalRangedAttack(
                cardWithRabies.slot.GetCoordinate(), 
                cardWithRabies.GetMinAttackRange(), 
                cardWithRabies.GetMaxAttackRange(),
                cardWithRabies.playerID, 
                game, 
                cardWithRabies.HasTrait("can_range_attack_ground"),
                cardWithRabies.HasTrait("indirect_fire"),
                true);
            
            return game.GetCardsOnCoordinates(validCoordinates);
        }
        
        private List<Card> GetMeleeTargets(Game game, Card cardWithRabies)
        {
            List<Card> meleeTargets = new List<Card>();
            var validCoordinates = cardWithRabies.GetCurrentMovementScheme().GetLegalMeleeAttack(
                cardWithRabies.slot.GetCoordinate(), 
                cardWithRabies.GetMoveRange(), 
                cardWithRabies.CanJump(), 
                cardWithRabies.playerID, 
                game, 
                true);
            
            return game.GetCardsOnCoordinates(validCoordinates);
        }

        private int GetMinimumDistanceToTargetInList(List<Card> validTargets, Card cardWithRabies)
        {
            int minDistance = int.MaxValue;
            foreach (var target in validTargets)
            {
                int distance = target.slot.GetDistanceTo(cardWithRabies.slot);
                if (distance < minDistance)
                {
                    minDistance = distance;
                }
            }

            return minDistance;
        }
        
        private List<Card> GetTargetsAtMinimumDistance(List<Card> validTargets, Card cardWithRabies, int minDistance)
        {
            List<Card> targetsAtMinDistance = new List<Card>();
            foreach (var target in validTargets)
            {
                if (target.slot.GetDistanceTo(cardWithRabies.slot) == minDistance)
                {
                    targetsAtMinDistance.Add(target);
                }
            }

            return targetsAtMinDistance;
        }

        private List<Card> GetTargetsWithLowestHealth(List<Card> targetsAtMinDistance)
        {
            List<Card> targetsWithLowestHealth = new List<Card>();
            int lowestHealth = int.MaxValue;
            foreach (var target in targetsAtMinDistance)
            {
                if (target.GetHP() < lowestHealth)
                {
                    lowestHealth = target.GetHP();
                }
            }

            foreach (var target in targetsAtMinDistance)
            {
                if (target.GetHP() == lowestHealth)
                {
                    targetsWithLowestHealth.Add(target);
                }
            }

            return targetsWithLowestHealth;
        }
        
        private Card GetTargetWithLowestHealth(List<Card> targetsWithLowestHealth)
        {
            return targetsWithLowestHealth[Random.Range(0, targetsWithLowestHealth.Count)];
        }
    }
}
