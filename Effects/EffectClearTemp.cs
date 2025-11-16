using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Clear temporary array of player's card
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/ClearTemp ", order = 10)]
    public class EffectClearTemp : EffectData
    {
        public override void DoEffectNoTarget(GameLogic logic, AbilityArgs args)
        {
            Player player = logic.Game.GetPlayer(args.caster.playerID);
            player.cards_temp.Clear();
        }

        public override void DoEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            Player player = logic.Game.GetPlayer(args.caster.playerID);
            player.cards_temp.Clear();
        }
    }
}