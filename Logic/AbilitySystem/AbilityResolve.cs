using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Ability.Target;
using TcgEngine;

namespace Monarchs.Logic.AbilitySystem
{
    public class AbilityResolve
    {
        protected ListSwap<ITargetable> targetArray = new ListSwap<ITargetable>();
        private AbilitySelector _abilitySelector;
        public AbilityLogicSystem logic;

        public AbilityResolve(AbilityLogicSystem logic, AbilitySelector abilitySelector)
        {
            _abilitySelector = abilitySelector;
            this.logic = logic;
        }

        //Resolve a card ability, may stop to ask for target. In most situation caster will be null when this is called. Caster will be defined later.
        public virtual void ResolveSelectorAndAbility(Game game, AbilityArgs args)
        {
            //Debug.Log("Trigger Ability " + ability.id + " : " + caster.card_id);
            
            if (args.triggerer is Card)
            {
                game.abilityTriggerer = (Card)args.triggerer;
            }

            if (!ValidateSelectors(game, ref args))
            {
                if (args.ability.trigger == AbilityTrigger.OnPlay && args.caster != null)
                {
                    args.caster.playedCardThisRound = false; // So that the player can play another card if he cancels the ability.
                }
                return;
            }

            ResolveAbility(game, args);
            
            logic.AfterAbilityResolved(args);
        }

        public virtual void ResolveAbility(Game game, AbilityArgs args)
        {
            if (args.caster == null)
            {
                args.caster = game.GetCurrentCardTurn()[0];
            }
            logic.OnAbilityStart(args.ability, args.caster);

            List<ITargetable> targets;
            
            if (args.ability.targetType == AbilityTargetType.PlayTarget)
            {
                Slot targetSlot = args.castedCard.slot;
                Card targetCard = game.GetSlotCard(targetSlot);
                args.target = targetCard != null ? targetCard: targetSlot;
            }
            
            

            if (args.ability.targetType == AbilityTargetType.PlayTargetAndSelectMultiplesTarget ||
                args.ability.targetType == AbilityTargetType.SelectMultipleTarget)
            {
                logic.AddAbilityHistory(args, null);
                logic.DoAbilityEffectsMultiple(args, Slot.GetSlotsOfTargets(game.selectorTargets));
                //onAbilityTarget?.Invoke(args, true);
                return;
            }

            targets = args.ability.GetTargets(game, args.caster, args.target, targetArray, true);
            logic.AddAbilityHistory(args, targets);
            
            foreach (ITargetable target in targets)
            {
                AbilityArgs targetArgs = args.Clone();
                targetArgs.target = target;
                bool isSelectTarget = args.target !=null && args.target.GetSlot().Equals(target?.GetSlot());
                logic.DoAbilityEffects(targetArgs, isSelectTarget);
            }

            if (targets.Count == 0)
            {
                args.target = null;
                logic.DoAbilityEffects(args, false);
            }
        }
        
        private bool ValidateSelectors(Game game, ref AbilityArgs args)
        {
            bool selectorChosenAlready = game.lastAbilityDone == args.ability.id && game.lastAbilityCasterUID == args.caster?.uid;

            if (!selectorChosenAlready)
            {
                bool isSelector = _abilitySelector.ResolveCardAbilitySelector(game, args);
                if (isSelector)
                    return false;
            }
            

            bool isActivateAbility = args.ability.trigger == AbilityTrigger.Activate;
            bool isPlaySpell = args.ability.trigger == AbilityTrigger.OnPlay &&
                               args.castedCard.CardData.cardType == CardType.Spell;
            if (isActivateAbility || isPlaySpell)
            {
                if (args.caster == null && game.selectorCasterUID != "")
                {
                    args.caster = game.GetCard(game.selectorCasterUID);
                }
                
                if (args.caster == null)
                {
                    args.caster = _abilitySelector.ValidatePossibleCastersAbilities(game, args, null);
                }
                
                if (args.caster == null)
                {
                    return false;
                }
            }

            return true;
        }
    }
}