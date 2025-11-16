using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Ability.Target;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using TcgEngine;

namespace Ability.Target
{
    public class AbilityTargetAllCardsAllPiles: AbilityTarget
    {
        public override List<ITargetable> GetAllTargets(Game data, AbilityArgs args,
            ListSwap<ITargetable> memoryArray = null)
        {
            if (memoryArray == null)
                memoryArray = new ListSwap<ITargetable>(); //Slow operation

            List<ITargetable> targets = memoryArray.Get();
            
            foreach (Player player in data.players)
            {
                AddList(data, player.cards_deck, targets);
                AddList(data, player.cards_discard, targets);
                AddList(data, player.cards_hand, targets);
                AddList(data, player.cards_trap, targets);
                AddList(data, player.cards_board, targets);
                AddList(data, player.cards_temp, targets);
            }

            return targets;
        }
        
        private void AddList(Game data, List<Card> source, List<ITargetable> targets)
        {
            foreach (Card card in source)
            {
                targets.Add(card);
            }
        }
    }
}