using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Checks if its your turn
    /// </summary>
    
    [CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/ConditionHasMana", order = 10)]
    public class ConditionHasMana : ConditionData
    {
        public PlayerMana.ManaType manaType;
        public bool mustHaveMana;

        public override bool IsTriggerConditionMetNoTarget(Game data, AbilityArgs args)
        {
            int playerID = args.castedCard != null ? args.castedCard.playerID : args.caster.playerID;
            Player player = data.GetPlayer(playerID);
            
            return player.playerMana.HasMana(manaType) == mustHaveMana;
        }
    }
}