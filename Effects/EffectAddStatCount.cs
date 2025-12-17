using System.Collections;
using System.Collections.Generic;
using Monarchs;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Effect that sets stats equal to a dynamic calculated value from a pile (number of cards on board/hand/deck)
    /// </summary>
    
    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/SetStatCustomCount", order = 10)]
    public class EffectAddStatCount : EffectData
    {
        public EffectStatType type;
        public PileType pile;

        [Header("Count Traits")]
        public CardType has_type;
        public GuildData hasGuild;
        public TraitData has_trait;

        public override void DoEffectPlayerTarget(GameLogic logic, AbilityArgs args)
        {
            int val = GetCount(logic.GetGameData(), args.caster);
            
        }

        public override void DoEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            int val = GetCount(logic.GetGameData(), args.caster);
            Card target = (Card) args.target;
            if (type == EffectStatType.Attack)
                target.AddValueToAttack(val);
            if (type == EffectStatType.HP)
                target.hp += val;
            if (type == EffectStatType.Mana)
                target.mana += val;
        }

        private int GetCount(Game data, Card caster)
        {
            Player player = data.GetPlayer(caster.playerID);
            return CountPile(player, pile);
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
                    if (IsTrait(card))
                        count++;
                }
                return count;
            }
            return 0;
        }

        private bool IsTrait(Card card)
        {
            bool is_type = card.CardData.cardType == has_type || has_type == CardType.None;
            bool is_team = card.CardData.guild == hasGuild || hasGuild == null;
            bool is_trait = card.HasTrait(has_trait) || has_trait == null;
            return (is_type && is_team && is_trait);
        }
    }
}