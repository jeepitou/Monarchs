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
    /// Effect that adds or removes basic card/player stats such as hp, attack, mana
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/AddStatus")]
    public class EffectAddStatus : EffectData
    {
        public bool customStatus = false;
        [HideIf("customStatus")] public StatusType type;
        [ShowIf("customStatus")] public StatusData statusData;
        public bool removeAtBeginningOfTurn = false;
        public string statusID;

        public override void DoEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            Card target = (Card) args.target;

            int duration = args.ability.duration;
            StatusType statusType = customStatus ? statusData.effect : type;
            
            target.AddStatus(new CardStatus(statusType, args.ability.value, duration, id: statusID, applier: args.caster.uid, removeAtBeginningOfTurn));
        }
    }
}