using System.Collections;
using System.Collections.Generic;
using Monarchs;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    public enum ConditionPlayerType
    {
        Self = 0,
        Opponent = 1,
        Both = 2,
    }

    /// <summary>
    /// Trigger condition that count the amount of cards in pile of your choise (deck/discard/hand/board...)
    /// Can also only count cards of a specific type/team/trait
    /// </summary>

    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/Count", order = 10)]
    public class ConditionCount : ConditionData
    {
        [Header("Count cards of type")]
        public ConditionPlayerType type;
        public PileType pile;
        public ConditionOperatorInt oper;
        public int value;

        [Header("Traits")]
        public CardType has_type;
        public GuildData hasGuild;
        public TraitData has_trait;
        public SubtypeData has_subtype;

        public override bool IsTriggerConditionMetNoTarget(Game data, AbilityArgs args)
        {
            if (args.caster == null || data == null)
            {
                Debug.LogError("Tried to apply ConditionCount on null data or caster.");
                return false;
            }
            
            int count = 0;
            if (type == ConditionPlayerType.Self || type == ConditionPlayerType.Both)
            {
                Player player =  data.GetPlayer(args.caster.playerID);
                count += CountPile(player, pile);
            }
            if (type == ConditionPlayerType.Opponent || type == ConditionPlayerType.Both)
            {
                Player player = data.GetOpponentPlayer(args.caster.playerID);
                count += CountPile(player, pile);
            }
            return CompareInt(count, oper, value);
        }

        private int CountPile(Player player, PileType pile)
        {
            List<Card> card_pile = null;

            if (pile == PileType.Hand)
                card_pile = player.cards_hand;

            if (pile == PileType.Board)
                card_pile = player.cards_board;

            if (pile == PileType.Deck)
                card_pile = player.cards_deck;

            if (pile == PileType.Discard)
                card_pile = player.cards_discard;

            if (pile == PileType.Temp)
                card_pile = player.cards_temp;

            if (card_pile != null)
            {
                int count = 0;
                foreach (Card card in card_pile)
                {
                    if (card.IsTeamTraitAndType(hasGuild, has_trait, has_subtype, has_type))
                        count++;
                }
                return count;
            }
            return 0;
        }
    }
}