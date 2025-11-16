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
        public bool removeOnApplierTurn = false;

        public override void DoEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            FindCardWithStatusAndRemoveStatus(logic.Game, type, args.caster);
            
            Card target = (Card) args.target;

            int duration = args.ability.duration;
            if (removeOnApplierTurn && duration != 0)
            {
                duration++; // Otherwise the status is removed directly when we finish the caster turn instead of on its next turn.
            }
            target.AddStatus(type, args.ability.value, duration, args.caster, removeOnApplierTurn);
            
        }
        
        private void FindCardWithStatusAndRemoveStatus(Game game, StatusType type, Card caster)
        {
            foreach (var player in game.players)
            {
                foreach (var card in player.cards_board)
                {
                    if (card.HasStatus(type) && card.GetStatus(type).applierUID == caster.uid)
                    {
                        card.RemoveStatus(type);
                        return;
                    }
                }
            }
        }
        
    }
}