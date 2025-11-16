using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Effect that steals a random mana from the opponent.
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/AddSelectMana", order = 10)]
    public class EffectAddSelectMana : EffectData
    {
        public override void DoEffectNoTarget(GameLogic logic, AbilityArgs args)
        {
            int playerID = args.castedCard.playerID;

            logic.Game.GetPlayer(playerID).playerMana.AddMana(args.manaType);
        }

        public override void DoEffectPlayerTarget(GameLogic logic, AbilityArgs args) //Target is opponent
        {
            int playerID = args.castedCard.playerID;

            logic.Game.GetPlayer(playerID).playerMana.AddMana(args.manaType);
        }
    }
}