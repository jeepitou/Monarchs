using System;
using System.Collections.Generic;
using System.Linq;
using Monarchs.Ability;
using Monarchs.Ability.Target;
using TcgEngine;
using UnityEngine;
using UnityEngine.Events;

namespace Monarchs.Logic.AbilitySystem
{
    public class AbilitySelector
    {
        private ListSwap<ITargetable> _targetArray;
        protected virtual AbilitySelectorRequest AbilitySelectorRequest { set; get; }
        public AbilityLogicSystem logic;

        public AbilitySelector(AbilityLogicSystem logic)
        {
            AbilitySelectorRequest = new AbilitySelectorRequest();
            this.logic = logic;
        }

        /// <summary>
        /// This functions check if the ability has a selector and triggers the correct selector if it's needed
        /// </summary>
        /// <returns>True if a selector is triggered</returns>
        public virtual bool ResolveCardAbilitySelector(Game gameData, AbilityArgs abilityArgs)
        {
            if (gameData == null || abilityArgs.ability == null)
            {
                Debug.LogError("AbilitySelector:ResolveCardAbilitySelector - Invalid data: Null Game or AbilityData");
                return false;
            }
            
            SelectorType selectorType = GetSelector(abilityArgs.ability.targetType);

            if (selectorType != SelectorType.None)
            {
                AbilitySelectorRequest.SetupRequestSelector(gameData, selectorType, abilityArgs);
                logic.OnSelectorStart();
                return true;
            }

            return false;
        }

        public virtual void SelectTarget(Game gameData, ITargetable target, Card selectedCaster)
        {
            if (!ValidateGameData(gameData)) return;
            if (!ValidateSelectTargetSelector(gameData)) return;
            AbilityArgs args = GenerateSelectArgs(gameData, target);
            if (!ValidateSelectTargetArgs(args)) return;
            
            if (gameData.selectorCasterUID != "")
            {
                args.caster  = gameData.GetCard(gameData.selectorCasterUID);
            }
            else
            {
                args.caster = ValidatePossibleCastersAbilities(gameData, args, selectedCaster);

                if (ValidateIfWaitingOnCasterSelection(gameData) || args.caster == null) return;
            }

            int targetNumber = gameData.selectorTargets.Count;
            if (!args.ability.CanTarget(gameData, args, targetNumber)) return;
            if (!ValidateSelectorCardSelectTarget(gameData, args)) return;
            
            gameData.selectorTargets.Add(target);

            if (gameData.selectorTargets.Count <
                args.ability.GetQuantityOfTargets(gameData.GetCard(gameData.selectorCardUID)))
            {
                
                gameData.selectorCasterUID = args.caster.uid;
                logic.OnSelectorSelect(args);
                return;
            }
            
            gameData.lastTarget = target;
            gameData.lastAbilityDone = gameData.selectorAbilityID;
            gameData.lastAbilityCasterUID = args.caster.uid;
            gameData.selector = SelectorType.None;
            logic.OnSelectorSelect(args);
        }
        
        public virtual void SelectChoice(Game gameData, int choice)
        {
            if (!ValidateGameData(gameData)) return;
            AbilityArgs args = GenerateSelectArgs(gameData);

            if (!ValidateSelectChoiceArgs(args)) return;
            if (!ValidateSelectChoiceSelector(gameData, args.ability)) return;
            if (!ValidateSelectChoiceChoice(args, choice)) return;

            AbilityData abilityChoice = args.ability.chain_abilities[choice];
            if (!ValidateAbilityChoice(gameData, abilityChoice, args)) return;

            //No need to select caster here, it will be selected when the selected ability is triggered
            gameData.selector = SelectorType.None;
            args.ability = abilityChoice; 
            logic.OnSelectorSelect(args);
        }
        
        // This is called by a server event after player select caster
        public virtual void SelectCaster(Game gameData, Card selectedCaster)
        {
            if (!ValidateGameData(gameData)) return;
            
            if (gameData.selector != SelectorType.SelectCaster)
                return;

            if (gameData.selectorAbilityID != "")
            {
                gameData.selector = SelectorType.SelectTarget;
                SelectTarget(gameData, gameData.selectorLastTargetSlot, selectedCaster);
            }
            else if (gameData.selectorCardUID != "")
            {
                gameData.selectorCasterUID = selectedCaster.uid;
                var cardToPlay = gameData.GetCard(gameData.selectorCardUID);
                gameData.selector = SelectorType.None;
                logic.OnSelectCasterPlayCard(cardToPlay, gameData.selectorLastTargetSlot, false);
            }
        }

        public virtual void RequestRangeAttackerChoice(Game game, Slot target, List<Card> potentialAttackers)
        {
            AbilitySelectorRequest.SetupRequestSelectRangeAttacker(game, target, potentialAttackers);
            logic.OnSelectorStart();
        }
        
        public virtual void SelectRangeAttacker(AbilityLogicSystem abilityLogic, Game gameData, Card selectedRangeAttacker)
        {
            if (gameData.selector != SelectorType.SelectRangeAttacker) return;

            if (!gameData.selectorPotentialCasters.Contains(selectedRangeAttacker.uid)) return;
            
            gameData.selector = SelectorType.None;
            abilityLogic.StartAttackWithSelectedRangeAttacker(selectedRangeAttacker, gameData.selectorLastTargetSlot);
        }
        
        public virtual void RequestManaChoice(Game game, Card card, Slot slot, Card caster)
        {
            AbilitySelectorRequest.SetupRequestChooseManaTypeToSpend(game, card, slot, caster);
            logic.OnSelectorStart();
        }
        
        public void SelectManaType(AbilityLogicSystem abilityLogic, Game game, PlayerMana.ManaType manaType)
        {
            if (!ValidateGameData(game)) return;

            if (game.selector == SelectorType.SelectManaTypeToGenerate)
            {
                game.selectorManaType = PlayerMana.ManaType.None;
                game.selector = SelectorType.None;
                AbilityArgs args = GenerateSelectArgs(game);
                args.manaType = manaType;
                if (args.ability.trigger == AbilityTrigger.Activate)
                {
                    args.caster = args.castedCard;
                }
                logic.OnSelectorSelect(args);
                return;
            }

            if (game.selector != SelectorType.SelectManaTypeToSpend)
                return;
            

            if (game.selectorCastedCardUID != "")
            {
                var caster = game.GetCard(game.selectorCasterUID);
                
                if (caster == null)
                {
                    Debug.Log("caster is null, can't select mana type to spend");
                    return;
                }
                
                if (caster.GetPieceType() == PieceType.Monarch)
                {
                    Debug.Log("Caster is Monarch, shouldn't be selecting mana to spend");
                    return;
                }
                
                var cardToPlay = game.GetCard(game.selectorCastedCardUID);
                if (cardToPlay.GetManaCost().HasFlag(manaType))
                {
                    return;
                }
                
                var player = game.GetPlayer(caster.playerID);
                if (!player.playerMana.HasMana(manaType))
                {
                    return;
                }
                
                game.selectorManaType = manaType;
                game.selector = SelectorType.None;
                logic.OnSelectCasterPlayCard(cardToPlay, game.selectorLastTargetSlot, false);
            }
        }
        
        // Called by GameServer when a mana type is selected by the player (network event)
        public void SelectMana(Game game, PlayerMana.ManaType manaType)
        {
            if (!ValidateGameData(game)) return;
            if (game.selector != SelectorType.SelectManaTypeToGenerate)
                return;

            game.selectorManaType = manaType;
            game.selector = SelectorType.None;
            AbilityArgs args = GenerateSelectArgs(game);
            args.manaType = manaType;
            if (args.ability.trigger == AbilityTrigger.Activate)
            {
                args.caster = args.castedCard;
            }
            logic.OnSelectorSelect(args);
        }
        
        public virtual void SkipSelection(Game gameData)
        {
            if (gameData.selector == SelectorType.SelectMultipleTarget)
            {
                AbilityData ability = GetAbility(gameData.selectorAbilityID);
                if (ability != null)
                {
                    int quantityOfTargets = ability.GetQuantityOfTargets(gameData.GetCard(gameData.selectorCasterUID));
                    for (int i = gameData.selectorTargets.Count; i < quantityOfTargets; i++)
                    {
                        gameData.selectorTargets.Add(null);
                    }
                }
                
                gameData.lastAbilityDone = gameData.selectorAbilityID;
                gameData.selector = SelectorType.None;
                AbilityArgs args = new AbilityArgs();
                args.castedCard = gameData.GetCard(gameData.selectorCastedCardUID);
                args.caster = gameData.GetCard(gameData.selectorCasterUID);
                args.ability = GetAbility(gameData.selectorAbilityID);
                args.target = null;
                logic.OnSelectorSelect(args);
            }
        }

        public virtual void CancelSelection(Game gameData)
        {
            if (gameData.selector != SelectorType.None || gameData.selectorAbilityID == "activate_choose_mana")
            {
                if (AbilityData.Get(gameData.selectorAbilityID)?.trigger == AbilityTrigger.OnPlay)
                {
                    gameData.GetCard(gameData.selectorCasterUID).playedCardThisRound = false;
                    Card castedCard = gameData.GetCard(gameData.selectorCastedCardUID);
                    Player player = gameData.GetPlayer(castedCard.playerID);
                    player.RemoveCardFromAllGroups(castedCard);
                    player.cards_hand.Add(castedCard);
                }
                
                if (gameData.selectorAbilityID == "activate_choose_mana")
                {
                    gameData.GetCard(gameData.selectorCasterUID).exhausted = false;
                }
                gameData.selector = SelectorType.None;
                gameData.selectorTargets = new List<ITargetable>();
                gameData.selectorCardUID = "";
                gameData.selectorCasterUID = "";
                gameData.selectorCastedCardUID = "";
                gameData.selectorAbilityID = "";
                gameData.selectorPotentialCasters = Array.Empty<string>();
                gameData.selectorManaType = PlayerMana.ManaType.None;
                logic.OnSelectorSelect(null);
            }
        }

        public virtual Card ValidatePossibleCastersPlayCard(Game gameData, AbilityArgs abilityArgs)
        {
            List<Card> possibleCasters;
            string cohortUid = gameData.GetCurrentCardTurn()[0].CohortUid;


            possibleCasters = abilityArgs.castedCard.GetCurrentPieceTurnThatCanTarget(gameData, cohortUid, (Slot)abilityArgs.target, false);

            return ValidatePossibleCasters(gameData, abilityArgs, possibleCasters, null);
        }

        public virtual Card ValidatePossibleCastersAbilities(Game gameData, AbilityArgs args, Card selectedCaster)
        {
            if (selectedCaster != null)
            {
                return selectedCaster;
            }
            
            if (args.caster == null)
            {
                args.caster = gameData.GetCurrentCardTurn()[0];
            }
            
            List<Card> possibleCasters;
            if (IsValid(args.target))
            {
                possibleCasters = args.ability.GetPiecesOfCohortThatCanTarget(gameData, args);
            }
            else
            {
                possibleCasters = args.ability.GetPiecesOfCohortThatCanCast(gameData, args.caster);
            }
            
            return ValidatePossibleCasters(gameData, args, possibleCasters, selectedCaster);
        }

        public virtual Card ValidatePossibleCasters(Game gameData, AbilityArgs args, List<Card> possibleCasters, Card selectedCaster)
        {
            if (selectedCaster != null)
            {
                return selectedCaster;
            }

            if (possibleCasters.Count == 1)
            {
                return possibleCasters[0];
            }

            if (possibleCasters.Count > 1)
            {
                AbilitySelectorRequest.SetupRequestSelectCaster(gameData, args, possibleCasters);
                logic.OnSelectorStart();
            }
            return null;
        }

        protected virtual SelectorType GetSelector(AbilityTargetType targetType)
        {
            if (targetType == AbilityTargetType.SelectTarget)
            {
                return SelectorType.SelectTarget;
            }

            if (targetType == AbilityTargetType.SelectMultipleTarget ||
                targetType == AbilityTargetType.PlayTargetAndSelectMultiplesTarget)
            {
                return SelectorType.SelectMultipleTarget;
            }

            if (targetType == AbilityTargetType.CardSelector)
            {
                return SelectorType.SelectorCard;
            }
            
            if (targetType == AbilityTargetType.ChoiceSelector)
            {
                return SelectorType.SelectorChoice;
            }

            if (targetType == AbilityTargetType.SelectManaType)
            {
                return SelectorType.SelectManaTypeToGenerate;
            }

            return SelectorType.None;
        }

        public virtual AbilityData GetAbility(string abilityID)
        {
            return AbilityData.Get(abilityID);
        }

        protected virtual bool IsValid(ITargetable target)
        {
            return Targetable.IsValid(target);
        }

        private bool ValidateGameData(Game game)
        {
            if (game == null)
            {
                return false;
            }

            return true;
        }

        private bool ValidateSelectTargetSelector(Game game)
        {
            if (game.selector == SelectorType.None || game.selector == SelectorType.SelectCaster || game.selector == SelectorType.SelectorChoice)
                return false;

            return true;
        }

        private bool ValidateSelectorCardSelectTarget(Game game, AbilityArgs args)
        {
            if (game.selector == SelectorType.SelectorCard)
            {
                if (args.target is not Card || !args.ability.IsCardSelectionValid(game, args.castedCard, (Card)args.target, _targetArray))
                {
                    return false; 
                }
            }

            return true;
        }

        private bool ValidateIfWaitingOnCasterSelection(Game game)
        {
            if (game.selector == SelectorType.SelectCaster)
            {
                return true;
            }

            return false;
        }

        private AbilityArgs GenerateSelectArgs(Game game, ITargetable target = null)
        {
            AbilityArgs args = new AbilityArgs() {target = target};
            args.castedCard = game.GetCard(game.selectorCastedCardUID);
            args.ability = GetAbility(game.selectorAbilityID);
            if (args.ability?.trigger != AbilityTrigger.Activate && args.ability?.trigger != AbilityTrigger.OnPlay)
            {
                args.caster = args.castedCard;
            }
            return args;
        }

        private bool ValidateSelectTargetArgs(AbilityArgs args)
        {
            if (args.castedCard == null || args.ability == null || !IsValid(args.target))
            {
                return false;
            }

            return true;
        }
        
        private bool ValidateSelectChoiceArgs(AbilityArgs args)
        {
            if (args.castedCard == null || args.ability == null)
            {
                return false;
            }

            return true;
        }

        private bool ValidateSelectChoiceSelector(Game game, AbilityData ability)
        {
            return game.selector == SelectorType.SelectorChoice &&
                   ability.targetType == AbilityTargetType.ChoiceSelector;
        }

        private bool ValidateSelectChoiceChoice(AbilityArgs args, int choice)
        {
            return choice >= 0 && choice < args.ability.chain_abilities.Length;
        }

        private bool ValidateAbilityChoice(Game game, AbilityData abilityChoice, AbilityArgs args)
        {
            return abilityChoice != null && abilityChoice.AreTriggerConditionsMet(game, args);
        }
    }
}
