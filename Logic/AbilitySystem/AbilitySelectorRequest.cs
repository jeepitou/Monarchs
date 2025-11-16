using System.Collections.Generic;

namespace Monarchs.Logic.AbilitySystem
{
    public class AbilitySelectorRequest
    {
        public virtual void SetupRequestSelectRangeAttacker(Game gameData, Slot target, List<Card> potentialAttackers)
        {
            gameData.selector = SelectorType.SelectRangeAttacker;
            gameData.selectorPlayer = potentialAttackers[0].playerID;
            gameData.selectorCardUID = "";
            gameData.selectorAbilityID = "";
            gameData.selectorLastTargetSlot = target;
            
            List<string> potentialCasterUID = new List<string>();
            foreach (var card in potentialAttackers)
            {
                potentialCasterUID.Add(card.uid);
            }

            gameData.selectorPotentialCasters = potentialCasterUID.ToArray();
        }
        
        public virtual void SetupRequestSelectCaster(Game gameData, AbilityArgs abilityArgs, List<Card> potentialCaster)
        {
            gameData.selector = SelectorType.SelectCaster;
            gameData.selectorPlayer = potentialCaster[0].playerID;
            gameData.selectorCardUID = "";
            gameData.selectorAbilityID = "";

            // This is to know if we are choosing a caster for an ability, or for a played card.
            if (abilityArgs.castedCard != null && !abilityArgs.castedCard.CardData.IsBoardCard()) 
            {
                gameData.selectorCardUID = abilityArgs.castedCard.uid;
            }
            else
            {
                gameData.selectorAbilityID = abilityArgs.ability.id;
            }

            if (abilityArgs.target is Slot)
            {
                gameData.selectorLastTargetSlot = (Slot)abilityArgs.target;
            }
            
            List<string> potentialCasterUID = new List<string>();
            foreach (var card in potentialCaster)
            {
                potentialCasterUID.Add(card.uid);
            }

            gameData.selectorPotentialCasters = potentialCasterUID.ToArray();
        }

        public virtual void SetupRequestChooseManaTypeToSpend(Game game, Card cardToPlay, Slot slot, Card caster)
        {
            game.selectorLastTargetSlot = slot;
            game.selector = SelectorType.SelectManaTypeToSpend;
            game.selectorCastedCardUID = cardToPlay.uid;
            game.selectorPlayer = cardToPlay.playerID;
            game.selectorCasterUID = caster.uid;
        }

        public virtual void SetupRequestChooseManaTypeToGenerate(Game game, int playerID, string castedCard, string abilityID)
        {
            game.selectorPlayer = playerID;
            game.selector = SelectorType.SelectManaTypeToGenerate;
            game.selectorAbilityID = abilityID;
            game.selectorCastedCardUID = castedCard; 
        }
        
        public virtual void SetupRequestSelector(Game gameData, SelectorType selectorType, AbilityArgs abilityArgs)
        {
            if (selectorType == SelectorType.None || selectorType == SelectorType.SelectCaster)
            {
                return;
            }

            if (selectorType == SelectorType.SelectManaTypeToGenerate)
            {
                SetupRequestChooseManaTypeToGenerate(gameData, abilityArgs.castedCard.playerID, abilityArgs.castedCard.uid, abilityArgs.ability.id);
                return;
            }

            gameData.selector = selectorType;
            gameData.selectorPlayer = abilityArgs.castedCard.playerID;
            gameData.selectorAbilityID = abilityArgs.ability.id;
            if (abilityArgs.caster != null)
            {
                gameData.selectorCasterUID = abilityArgs.caster.uid;
            }
            gameData.selectorCastedCardUID = abilityArgs.castedCard.uid;
        }
    }
}