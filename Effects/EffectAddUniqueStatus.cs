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
    /// Effect that adds a "unique" status, which mean it will remove the previous status of the same type
    /// that was applied by the same caster.
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/AddUniqueStatus")]
    public class EffectAddUniqueStatus : EffectData
    {
        public StatusType type;
        public string statusID;
        public bool removeAtBeginningOfTurn = false;

        public override void DoEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            FindCardWithStatusAndRemoveStatus(logic.Game, type, args.caster);
            
            Card target = (Card) args.target;

            int duration = args.ability.duration;
            
            target.AddStatus(new CardStatus(type, args.ability.value, duration, id: statusID, applier: args.caster.uid, removeAtBeginningOfTurn));
            
        }
        
        private void FindCardWithStatusAndRemoveStatus(Game game, StatusType type, Card caster)
        {
            foreach (var player in game.players)
            {
                foreach (var card in player.cards_board)
                {
                    if (card.HasStatus(statusID) && card.GetStatus(statusID).applierUID == caster.uid)
                    {
                        card.RemoveStatus(type);
                        return;
                    }
                }
            }
        }
        
    }
}