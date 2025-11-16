using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Effect to play a card from your hand for free
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/Play", order = 10)]
    public class EffectPlay : EffectData
    {
        public override void DoEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            Game game = logic.GetGameData();
            Player player = game.GetPlayer(args.caster.playerID);
            Slot slot = Slot.GetRandom(logic.GetRandom());

            player.RemoveCardFromAllGroups(args.CardTarget);
            player.cards_hand.Add(args.CardTarget);

            if (slot != Slot.None)
            {
                logic.PlayCard(args.CardTarget, slot, true);
            }
        }
    }
}