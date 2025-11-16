using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Change owner of target card to the owner of the caster (or the opponent player)
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/ChangeOwner", order = 10)]
    public class EffectChangeOwner : EffectData
    {
        public bool owner_opponent; //Change to self or opponent?

        public override void DoEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            Game game = logic.GetGameData();
            Player tplayer = owner_opponent ? game.GetOpponentPlayer(args.caster.playerID) : game.GetPlayer(args.caster.playerID);
            logic.ChangeOwner(args.CardTarget, tplayer);

            if (args.CardTarget.CardData.type == PieceType.Champion)
            {
                args.CardTarget.AddStatus(StatusType.MindControlled, 0, 1);
            }
        }
    }
}