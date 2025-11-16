using Monarchs.Ability;
using UnityEngine.Events;

namespace Monarchs.Logic.AbilitySystem
{
    public class AbilityTriggerManager
    {
        public AbilityLogicSystem logic;
        
        public AbilityTriggerManager(AbilityLogicSystem logicSystem)
        {
            logic = logicSystem;
        }
        /// <summary>
        /// This triggers all abilities of a single card that correspond with the trigger type.
        /// </summary>
        public virtual void TriggerCardAbilityType(Game game,
                                                    AbilityTrigger triggerType, 
                                                    AbilityArgs args)
        {
            foreach (AbilityData ability in args.castedCard.GetAllCurrentAbilities())
            {
                if (ability && ability.trigger == triggerType)
                {
                    AbilityArgs abilityArgs = args.Clone();
                    abilityArgs.ability = ability;
                    TriggerCardAbility(game, abilityArgs);
                }
            }
        }

        /// <summary>
        /// This triggers all board cards ability of a type. Called with OnPlayOther and
        /// OnDeathOther triggers. This is why there is no caster required.
        /// </summary>
        public virtual void TriggerAllAbilityWithTriggerType(Game game, AbilityTrigger triggerType, Card triggerer)
        {
            foreach (Player player in game.players)
            {
                foreach (Card card in player.cards_board)
                {
                    AbilityArgs args = new AbilityArgs() {caster=card, castedCard = card, triggerer = triggerer};
                    TriggerCardAbilityType(game, triggerType, args);
                }
                    
            }
        }

        /// <summary>
        /// This trigger all board cards with a certain trigger type, for a single player.
        /// </summary>
        public virtual void TriggerAllAbilityWithTriggerTypeOfPlayer(Game game, Player player, AbilityTrigger triggerType)
        {
            foreach (Card card in player.cards_board)
            {
                AbilityArgs args = new AbilityArgs() {castedCard = card, caster = card};
                TriggerCardAbilityType(game, triggerType, args);
            }
        }

        /// <summary>
        /// This trigger a single card ability.
        /// </summary>
        public virtual void TriggerCardAbility(Game game, AbilityArgs args)
        {
            if (args.ability.trigger == AbilityTrigger.OnMoveOnSpecificSquare && args.ability.AreTriggerConditionsMet(game, args))
            {
                logic.OnTrapTriggered(args);
                return;
            }

            if (args.ability.AreTriggerConditionsMet(game, args))
            {
                logic.OnAbilityTriggered(args);
            }
        }
        
        /// <summary>
        /// This triggers all the chain abilities for a single ability.
        /// </summary>
        public virtual void TriggerChainAbility(Game game, AbilityArgs args)
        {
            if (args.ability.targetType != AbilityTargetType.ChoiceSelector && game.State != GameState.GameEnded)
            {
                foreach (AbilityData chainAbility in args.ability.chain_abilities)
                {
                    if (chainAbility != null)
                    {
                        game.selector = SelectorType.None;
                        game.selectorAbilityID = "";
                        game.selectorCardUID = "";
                        game.selectorCasterUID = "";
                        
                        AbilityArgs chainArg = args.Clone();
                        chainArg.ability = chainAbility;
                        TriggerCardAbility(game, chainArg);
                    }
                }
            }
        }
    }
}