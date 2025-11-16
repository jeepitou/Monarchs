using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Effect that adds or removes basic card/player stats such as hp, attack, mana, by the value of the dice roll
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/AddStatRoll", order = 10)]
    public class EffectAddStatRoll : EffectData
    {
        public EffectStatType type;
        
        public override void DoEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            Game data = logic.GetGameData();
            Card target = (Card) args.target;

            if (type == EffectStatType.Attack)
                target.AddValueToAttack(data.rolledValue);
            if (type == EffectStatType.HP)
                target.hp += data.rolledValue;
            if (type == EffectStatType.Mana)
                target.mana += data.rolledValue;
        }
    }
}