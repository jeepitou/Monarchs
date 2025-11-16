using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Ability.Target;
using Monarchs.Initiative;
using TcgEngine;
using UnityEngine.Events;

namespace Monarchs.Logic.AbilitySystem
{
    public class AbilityLogicSystem
    {
        public UnityAction<AbilityData, Card> onAbilityStart;        
        public UnityAction<AbilityArgs, bool> onAbilityTarget;
        public UnityAction<AbilityArgs, List<Slot>> onAbilityTargetMultiple;
        public UnityAction<AbilityArgs> onAbilityEnd;
        public UnityAction<Card, Slot, bool> onSelectCasterPlayCard;
        
        
        public UnityAction onSelectorSelect;
        public UnityAction onSelectorStart;
        
        public UnityAction<Card, Card> onTrapTriggered;
        public UnityAction<Card, Card> onTrapResolved;

        private GameLogic _gameLogic;
        protected virtual AbilityTriggerManager _abilityTriggerManager { set; get; }
        protected virtual AbilityResolve _abilityResolve { set; get; }
        protected virtual AbilitySelector _abilitySelector { set; get; }
        protected virtual AbilityTraps _abilityTraps { set; get; }
        protected virtual AbilityOngoingEffects _abilityOngoingEffects { set; get; }

        public static AbilityLogicSystem Create(GameLogic gameLogic)
        {
            return new (gameLogic);
        }
        
        public AbilityLogicSystem(GameLogic gameLogic, AbilityTriggerManager abilityTriggerManager, AbilityResolve abilityResolve, 
            AbilitySelector abilitySelector, AbilityTraps abilityTraps, AbilityOngoingEffects abilityOngoingEffects)
        {
            _gameLogic = gameLogic;

            _abilityTriggerManager = abilityTriggerManager;
            _abilitySelector = abilitySelector;
            _abilityResolve = abilityResolve;
            _abilityTraps = abilityTraps;
            _abilityOngoingEffects = abilityOngoingEffects;
        }

        public AbilityLogicSystem(GameLogic gameLogic)
        {
            _gameLogic = gameLogic;

            _abilityTriggerManager = new AbilityTriggerManager(this);
            _abilitySelector = new AbilitySelector(this);
            _abilityResolve = new AbilityResolve(this, _abilitySelector);
            _abilityTraps = new AbilityTraps(this);
            _abilityOngoingEffects = new AbilityOngoingEffects();
        }

        #region TriggerManager wraps
        public virtual void TriggerCardAbilityType(AbilityTrigger triggerType, AbilityArgs args)
        {
            _abilityTriggerManager.TriggerCardAbilityType(_gameLogic.Game, triggerType, args);
        }
        
        public virtual void TriggerCardAbilityType(AbilityTrigger triggerType, Card playedCard, Card caster=null)
        {
            AbilityArgs args = new AbilityArgs() {caster = caster, castedCard = playedCard};
            _abilityTriggerManager.TriggerCardAbilityType(_gameLogic.Game, triggerType, args);
        }

        public virtual void TriggerAllAbilityWithTriggerType(AbilityTrigger triggerType, Card triggerer)
        {
            _abilityTriggerManager.TriggerAllAbilityWithTriggerType(_gameLogic.Game, triggerType, triggerer);
        }

        public virtual void TriggerAllAbilityWithTriggerTypeOfPlayer(Player player,
            AbilityTrigger triggerType)
        {
            _abilityTriggerManager.TriggerAllAbilityWithTriggerTypeOfPlayer(_gameLogic.Game, player, triggerType);
        }
        
        public virtual void TriggerCardAbility(AbilityArgs args)
        {
            _abilityTriggerManager.TriggerCardAbility(_gameLogic.Game, args);
        }
        
        public virtual void TriggerCardAbility(AbilityData ability, Card castedCard, Card triggerer=null)
        {
            AbilityArgs args = new AbilityArgs() {ability = ability, castedCard = castedCard, triggerer = triggerer};
            if (GameplayData.Get().InitiativeType == InitiativeManager.InitiativeType.AllPiecesEveryTurn)
            {
                args.caster = castedCard;
                if (castedCard.exhausted)
                {
                    return;
                }
            }
            _abilityTriggerManager.TriggerCardAbility(_gameLogic.Game, args);
        }
        

        #endregion

        #region OngoingEffects wraps
        public virtual void UpdateOngoingEffect()
        {
            _abilityOngoingEffects.UpdateOngoingAbilities(_gameLogic);
        }
        #endregion

        #region Selector wraps

        public virtual void RequestRangeAttackerChoice(Slot target, List<Card> potentialAttackers)
        {
            _abilitySelector.RequestRangeAttackerChoice(_gameLogic.Game, target, potentialAttackers);
        }
        
        public virtual void RequestChooseMana(Card cardToPlay, Slot slot, Card caster)
        {
            _abilitySelector.RequestManaChoice(_gameLogic.Game, cardToPlay, slot, caster);
        }

        public virtual void ReceiveSelectRangeAttacker(Card selectedRangeAttacker)
        {
            _abilitySelector.SelectRangeAttacker(this, _gameLogic.Game, selectedRangeAttacker);
        }
        
        public void ReceiveSelectManaType(PlayerMana.ManaType manaType)
        {
            _abilitySelector.SelectManaType(this, _gameLogic.Game, manaType);
        }

        public virtual void StartAttackWithSelectedRangeAttacker(Card selectedRangeAttacker, Slot slot)
        {
            _gameLogic.AttackTarget(selectedRangeAttacker, slot, false, true);
        }

        public virtual Card ValidatePossibleCastersPlayCard(Slot slot, Card card, Card caster)
        {
            AbilityArgs args = new AbilityArgs() {target = slot, castedCard = card, caster = caster};
            return _abilitySelector.ValidatePossibleCastersPlayCard(_gameLogic.Game, args);
        }

        public virtual void SelectTarget(ITargetable target, Card selectedCaster = null)
        {
            _abilitySelector.SelectTarget(_gameLogic.Game, target, selectedCaster);
        }

        public virtual void SelectChoice(int choice)
        {
            _abilitySelector.SelectChoice(_gameLogic.Game, choice);
        }

        public virtual void SelectCaster(Card selectedCaster)
        {
            _abilitySelector.SelectCaster(_gameLogic.Game, selectedCaster);
        }
        
        public virtual void SkipSelection()
        {
            _abilitySelector.SkipSelection(_gameLogic.Game);
        }

        public virtual void CancelSelection()
        {
            _abilitySelector.CancelSelection(_gameLogic.Game);
        }

        #endregion

        /// <summary>
        /// This checks if there are any trap on move path, and trigger the closest one.
        /// </summary>
        public virtual bool TriggerTrapsOnMovePath(AbilityTrigger trapTrigger, Card movingCard, Slot destinationSlot)
        {
            List<Slot> trapsOnMovePath = _abilityTraps.GetTrapsOnMovePath(_gameLogic.Game, movingCard, destinationSlot);
            if (trapsOnMovePath.Count > 0)
            {
                _abilityTraps.TriggerClosestTrapOnMovePath(_gameLogic.Game, movingCard, destinationSlot, trapsOnMovePath);
                return true;
            }

            return false;
        }

        public virtual bool TriggerTrapsOnSpecificSlot(AbilityTrigger trapTrigger, Card card, Slot slot)
        {
            if (card.CardData.cardType != CardType.Character)
            {
                return false;
            }
            Card trap = _abilityTraps.GetTrapOnSlot(_gameLogic.Game, card, slot);
            if (trap != null)
            {
                _abilityTraps.TriggerTrapOnSlot(_gameLogic.Game, card, trap);
                return true;
            }

            return false;
        }
        
        public virtual void OnTrapTrigger(Card trap, Card triggerer)
        {
            if (trap == null || trap.exhausted)
            {
                return;
            }

            if (trap.slot != triggerer.slot)
            {
                _gameLogic.MoveCardToTrap(triggerer, trap.slot);
            }

    
            trap.exhausted = true;
            AbilityArgs args = new AbilityArgs() {triggerer = triggerer, castedCard = trap};
            _gameLogic.ResolveQueue.AddTrapTrigger(args, ResolveTrap);
            

            onTrapTriggered?.Invoke(trap, triggerer);
            _gameLogic.DiscardCard(trap);
            _gameLogic.ResolveQueue.ResolveAll();
        }
        
        public void OnSelectorSelect(AbilityArgs args)
        {
            if (args == null)
            {
                onSelectorSelect?.Invoke();
                return;
            }

            if (args.ability.targetType == AbilityTargetType.SelectManaType)
            {
                OnSelectorSelectManaType(args);
                return;
            }
            
            Game game = _gameLogic.Game;

            if (args.caster == null) // caster is null when it's a choice ability
            {
                
                _abilityResolve.ResolveSelectorAndAbility(game, args);
                //onAbilityEnd?.Invoke(args);
            }
            else
            {
                if (args.ability.trigger == AbilityTrigger.OnPlay) // This means we were playing a card.
                {
                    if (game.selectorTargets.Count ==
                        args.ability.GetQuantityOfTargets(game.GetCard(game.selectorCardUID)))
                    {
                        _gameLogic.PlayCard(args.castedCard, args.castedCard.slot);
                        onSelectorSelect?.Invoke();
                        game.selectorTargets = new List<ITargetable>();
                        return;
                    }
                }
                else
                {
                    if (game.selectorTargets.Count >=
                        args.ability.GetQuantityOfTargets(game.GetCard(game.selectorCardUID)))
                    {
                        // if (args.ability.trigger == AbilityTrigger.Activate)
                        // {
                        //     game.history.AddHistory(GameAction.CastAbility, player.player_id, args.caster, args.ability, args.target); TODO
                        // }
                        onSelectorSelect?.Invoke();
                        onAbilityTargetMultiple?.Invoke(args, Slot.GetSlotsOfTargets(game.selectorTargets));
                        DoAbilityEffects(args, false);
                        AfterAbilityResolved(args);
                        AddAbilityHistory(args, game.selectorTargets);
                        game.selectorTargets = new List<ITargetable>();
                        return;
                    }
                }
            }
            onSelectorSelect?.Invoke();
        }

        private void OnSelectorSelectManaType(AbilityArgs args)
        {
            Game game = _gameLogic.Game;
            onSelectorSelect?.Invoke();
            DoAbilityEffects(args, false);
            AfterAbilityResolved(args);
            AddAbilityHistory(args, game.selectorTargets);
            game.selectorTargets = new List<ITargetable>();
        }
        
        public virtual void ResolveTrap(Game game, AbilityArgs args)
        {
            if (args.castedCard.CardData.cardType != CardType.Trap)
            {
                return;
            }
            Card trapCard = args.castedCard;
            Player player = game.GetPlayer(trapCard.playerID);

            Player trappedPlayer = game.GetPlayer(((Card)args.triggerer).playerID);
            //game.history.AddHistory(GameAction.TrapTriggered, player.player_id, trapCard, (Card)args.triggerer); //TODO

            TriggerCardAbilityType(AbilityTrigger.OnMoveOnSpecificSquare, args);
            onTrapResolved?.Invoke(trapCard, (Card)args.triggerer);
            _gameLogic.DiscardCard(trapCard);
            _gameLogic.ResolveQueue.ResolveAll();
        }

        public void OnAbilityStart(AbilityData ability, Card card)
        {
            onAbilityStart?.Invoke(ability, card);
        }
        
        public void OnSelectCasterPlayCard(Card card, Slot slot, bool skipCost)
        {
            onSelectCasterPlayCard?.Invoke(card, slot, skipCost);
            onSelectorSelect?.Invoke();
        }
        
        public void OnSelectorStart()
        {
            onSelectorStart?.Invoke();
        }

        public void DoAbilityEffects(AbilityArgs args, bool selectedTarget = false)
        {
            if (!args.ability.MultipleTargetToSelect)
                onAbilityTarget?.Invoke(args, selectedTarget);


            args.ability.DoEffects(_gameLogic, args);
        }

        public void DoAbilityEffectsMultiple(AbilityArgs args, List<Slot> slots)
        {
            onAbilityTargetMultiple?.Invoke(args, slots);
            args.ability.DoEffects(_gameLogic, args);
        }
        
        public void AddAbilityHistory(AbilityArgs args, List<ITargetable> targets)
        {
            _gameLogic.AddAbilityHistory(args, targets);
        }
        
        public virtual void AfterAbilityResolved(AbilityArgs args)
        {
            if (args.ability.exhaust)
            {
                args.caster.exhausted = true;
            }
            
            onAbilityEnd?.Invoke(args); //This events needs to be fired before chain ability to avoid infinite chain ability that would make the player win.
            
            _abilityTriggerManager.TriggerChainAbility(_gameLogic.Game, args);
        }

        public void OnAbilityTriggered(AbilityArgs args)
        {
            _gameLogic.ResolveQueue.AddAbility(args, _abilityResolve.ResolveSelectorAndAbility);
            _gameLogic.ResolveQueue.ResolveAll();
        }
        
        public void OnTrapTriggered(AbilityArgs args)
        {
            _gameLogic.ResolveQueue.AddTrapResolve(args, _abilityResolve.ResolveSelectorAndAbility);
            _gameLogic.ResolveQueue.AddCallback(_gameLogic.EndTurnAutomaticallyWhenExhausted);
            _gameLogic.ResolveQueue.ResolveAll();
        }
    }
}