using System;
using System.Collections.Generic;
using Monarchs;
using Monarchs.Ability;
using Monarchs.Ability.Target;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using TcgEngine;
using UnityEngine;

namespace Ability.Target
{
    public class AbilityTargetExtractor
    {
        private AbilityTargetSelf _self = new();
        
        private AbilityTargetPlayerSelf _playerSelf = new ();
        private AbilityTargetPlayerOpponent _playerOpponent = new ();
        private AbilityTargetAllPlayers _allPlayers = new ();
        
        private AbilityTargetSavedCardForAbility _savedCardForAbility = new ();
        private AbilityTargetCurrentPiece _currentPiece = new ();
        private AbilityTargetAllCardsBoard _allCardsBoard = new ();
        private AbilityTargetAllCardsHands _allCardsHands = new ();
        private AbilityTargetAllCardsAllPiles _allCardsAllPiles = new ();
        private AbilityTargetAllSlots _allSlots = new ();
        
        private AbilityTargetAbilityTriggerer _abilityTriggerer = new ();
        
        private AbilityTargetLastAttackedSlot _lastAttackedSlot = new ();
        private AbilityTargetLastAttackedCard _lastAttackedCard = new ();
        private AbilityTargetLastPlayed _lastPlayed = new ();
        private AbilityTargetLastTargetted _lastTargetted = new ();
        private AbilityTargetLastKilled _lastKilled = new ();

        public List<ITargetable> GetAllPotentialTargets(Game data, AbilityArgs args,
            ListSwap<ITargetable> memoryArray = null)
        {
            List<ITargetable> targets = new List<ITargetable>();
            
            foreach (var target in _allSlots.GetAllTargets(data, args, memoryArray))
            {
                args.target = target;
                if (target != null && AreTargetConditionsMet(data, args))
                {
                    targets.Add(target);
                }
            }

            targets = FilterAllTargets(data, args.ability, args.caster, targets, memoryArray.GetOther(targets));

            return targets;
        }

        public List<ITargetable> GetAllTargets(Game data, AbilityArgs args,
            ListSwap<ITargetable> memoryArray = null)
        {
            if (memoryArray == null)
                memoryArray = new ListSwap<ITargetable>(); //Slow operation

            List<ITargetable> targets = new List<ITargetable>();
            
            foreach (var target in GetAllRawTargets(data, args, memoryArray))
            {
                args.target = target;
                if (target != null && AreTargetConditionsMet(data, args))
                {
                    targets.Add(target);
                }
            }

            targets = FilterAllTargets(data, args.ability, args.caster, targets, memoryArray.GetOther(targets));
            args.target = null;
            return targets;
        }
        
        public List<ITargetable> GetAllRawTargets(Game data, AbilityArgs args,
            ListSwap<ITargetable> memoryArray = null)
        {
            switch (args.ability.targetType)
            {
                case AbilityTargetType.Self:
                    return _self.GetAllTargets(data, args, memoryArray);
                case AbilityTargetType.PlayerSelf:
                    return _playerSelf.GetAllTargets(data, args, memoryArray);
                case AbilityTargetType.PlayerOpponent:
                    return _playerOpponent.GetAllTargets(data, args, memoryArray);
                case AbilityTargetType.AllPlayers:
                    return _allPlayers.GetAllTargets(data, args, memoryArray);
                case AbilityTargetType.CurrentPiece:
                    return _currentPiece.GetAllTargets(data, args, memoryArray);
                case AbilityTargetType.AllCardsBoard:
                    return _allCardsBoard.GetAllTargets(data, args, memoryArray);
                case AbilityTargetType.AllCardsHand:
                    return _allCardsHands.GetAllTargets(data, args, memoryArray);
                case AbilityTargetType.AllCardsAllPiles:
                    return _allCardsAllPiles.GetAllTargets(data, args, memoryArray);
                case AbilityTargetType.AllSlots:
                    return _allSlots.GetAllTargets(data, args, memoryArray);
                case AbilityTargetType.AbilityTriggerer:
                    return _abilityTriggerer.GetAllTargets(data, args, memoryArray);
                case AbilityTargetType.LastAttackedSlot:
                    return _lastAttackedSlot.GetAllTargets(data, args, memoryArray);
                case AbilityTargetType.LastAttackedCard:
                    return _lastAttackedCard.GetAllTargets(data, args, memoryArray);
                case AbilityTargetType.LastPlayed:
                    return _lastPlayed.GetAllTargets(data, args, memoryArray);
                case AbilityTargetType.LastTargeted:
                    return _lastTargetted.GetAllTargets(data, args, memoryArray);
                case AbilityTargetType.LastKilled:
                    return _lastKilled.GetAllTargets(data, args, memoryArray);
                case AbilityTargetType.CardSavedForAbility:
                    return _savedCardForAbility.GetAllTargets(data, args, memoryArray);
                case AbilityTargetType.SelectTarget:
                    return _allSlots.GetAllTargets(data, args, memoryArray);
            }

            return memoryArray.Get();
        }
        
        public List<ITargetable> GetAllSelectTargets(Game data, AbilityArgs args,
            ListSwap<ITargetable> memoryArray = null, bool dontVerifyPlayTarget=false) //dontVerifyPlayTarget is used to force the trapped card to be added
        {
            if (memoryArray == null)
                memoryArray = new ListSwap<ITargetable>(); //Slow operation

            List<ITargetable> targets = memoryArray.Get();
            
            if (args.target != null && (AreTargetConditionsMet(data, args) || dontVerifyPlayTarget))
            {
                targets.Add(args.target);
            }
            
            if (args.ability.trigger == AbilityTrigger.Activate && 
                (args.ability.targetType == AbilityTargetType.SelectTarget || args.ability.targetType == AbilityTargetType.PlayTarget))
            {
                targets = data.intimidateManager.FilterIntimidate(data, args.caster.playerID, targets);
            }

            targets = FilterAllTargets(data, args.ability, args.caster, targets, memoryArray.GetOther(targets));

            return targets;
        }

        public bool AreTargetConditionsMet(Game data, AbilityArgs args, int targetNumber = -1, bool validateInput=false)
        {
            if (args.ability.trigger == AbilityTrigger.Activate && 
                (args.ability.targetType == AbilityTargetType.SelectTarget 
                 || args.ability.targetType == AbilityTargetType.PlayTarget ||
                 args.ability.targetType == AbilityTargetType.SelectMultipleTarget))
            {
                if (data.intimidateManager.AbilityMustTargetIntimidatorInsteadOfCurrentTarget(data, args))
                {
                    Debug.Log($"Ability {args.ability.id} cannot target the current target {args.target} because it must target the Intimidator instead");
                    return false;
                }
            }

            
            if (targetNumber != -1 && !args.ability.useCohortSizeForQuantityOfTargets && args.ability.targetType != AbilityTargetType.SelectTarget )
            {
                return AreTargetConditionsMetWithTargetNumber(data, args, targetNumber, validateInput);
            }

            if (args.ability.useCohortSizeForQuantityOfTargets)
            {
                foreach (ConditionData cond in args.ability.targetSpecifications[0].conditions)
                {
                    if (cond != null && !cond.IsTargetConditionMet(data, args))
                    {
                        Debug.Log($"Condition {cond.name} is not met for ability {args.ability.id} on target {args.target}");
                        return false;
                    }
                        
                }
            }
            
            foreach (ConditionData cond in args.ability.conditions_target)
            {
                if (cond != null && !cond.IsTargetConditionMet(data, args))
                {
                    Debug.Log($"Condition {cond.name} is not met for ability {args.ability.id} on target {args.target}");
                    return false;
                }
            }
            return true;
        }

        private bool AreTargetConditionsMetWithTargetNumber(Game data, AbilityArgs args, int targetNumber, bool validateInput=false)
        {
            if (args.ability.targetSpecifications.Length < targetNumber)
            {
                Debug.Log($"All targets are selected for ability {args.ability.id} but the ability is requiring target#{targetNumber}");
                return false;
            }
            foreach (ConditionData cond in args.ability.targetSpecifications[targetNumber].conditions)
            {
                if (cond != null && !cond.IsTargetConditionMet(data, args))
                {
                    Debug.Log($"Condition {cond.name} is not met for ability {args.ability.id} on target {args.target}");
                    return false;
                }
            }
            return true;
        }
        
        private List<ITargetable> FilterAllTargets(Game data, AbilityData ability, Card caster, List<ITargetable> targets,
            List<ITargetable> dest)
        {
            if (ability.filters_target != null && targets.Count > 0)
            {
                foreach (FilterData filter in ability.filters_target)
                {
                    if (filter != null)
                        targets = filter.FilterTargets(data, ability, caster, targets, dest);
                }
            }

            return targets;
        }
    }
}