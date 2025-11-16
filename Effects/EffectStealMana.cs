using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Effect that steals a random mana from the opponent.
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/StealMana", order = 10)]
    public class EffectStealMana : EffectData
    {
        public override void DoEffectPlayerTarget(GameLogic logic, AbilityArgs args) //Target is opponent
        {
            PlayerMana.ManaType manaToSteal = logic.Game.GetPlayer(args.PlayerTarget.playerID).playerMana.GetRandomOwnedMana();

            if (manaToSteal == PlayerMana.ManaType.None)
            {
                return;
            }
            
            logic.Game.GetPlayer(args.PlayerTarget.playerID).playerMana.SpendMana(manaToSteal);
            logic.Game.GetPlayer(args.caster.playerID).playerMana.AddMana(manaToSteal);
        }
    }
}