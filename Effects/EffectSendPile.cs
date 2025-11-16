using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    //Sends the target card to a pile of your choice (deck/discard/hand)
    //Dont use to send to board since it needs a slot, use EffectPlay instead to send to board
    //Also dont send to discard from the board because it wont trigger OnKill effects, use EffectDestroy instead

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/SendPile", order = 10)]
    public class EffectSendPile : EffectData
    {
        public PileType pile;
        public PlayerMana.ManaType additionalManaCost;

        public override void DoEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            if (args.CardTarget.IsMonarch())
            {
                return; //Monarchs can't be sent back to hand
            }

            Game data = logic.GetGameData();
            Player player = data.GetPlayer(args.CardTarget.playerID);
            
            if (pile == PileType.Deck)
            {
                player.RemoveCardFromAllGroups(args.CardTarget);
                player.cards_deck.Add(args.CardTarget);
            }

            if (pile == PileType.Hand)
            {
                player.RemoveCardFromAllGroups(args.CardTarget);
                args.CardTarget.ResetCohortSize();
                logic.GetGameData().DestroyAllOtherCohortCards(args.CardTarget);
                player.cards_hand.Add(args.CardTarget);
            }

            if (pile == PileType.Discard)
            {
                player.RemoveCardFromAllGroups(args.CardTarget);
                player.cards_discard.Add(args.CardTarget);
            }

            if (pile == PileType.Temp)
            {
                player.RemoveCardFromAllGroups(args.CardTarget);
                player.cards_temp.Add(args.CardTarget);
            }
            
            if (pile != PileType.Board)
            {
                logic.Game.initiativeManager.RemoveCard(args.CardTarget, logic.Game);
            }
            
            args.CardTarget.mana |= additionalManaCost;
        }
    }

    public enum PileType
    {
        None = 0,
        Board = 10,
        Hand = 20,
        Deck = 30,
        Discard = 40,
        Temp = 90,
    }

}
