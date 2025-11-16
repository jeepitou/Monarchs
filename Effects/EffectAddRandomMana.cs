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
    /// Effect that adds a random mana
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/AddRandomMana", order = 10)]
    public class EffectAddRandomMana : EffectData
    {
        public bool canBeAManaThePlayerOwns;
        public PlayerMana.ManaType possibleManaType;
        
        public override void DoEffectPlayerTarget(GameLogic logic, AbilityArgs args)
        {
            Player target = (Player) args.target;
            
            target.playerMana.AddRandomMana(possibleManaType, canBeAManaThePlayerOwns, logic.GetRandom());
        }
    }
}