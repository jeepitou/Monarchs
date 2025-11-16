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
    /// Effect that remove find all status of a type that was applied by the caster and remove it.
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/EffectRemoveAppliedStatus")]
    public class EffectRemoveAppliedStatus : EffectData
    {
        public StatusType type;
        public bool removeOnApplierTurn = false;

        public override void DoEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            FindCardWithStatusAndRemoveStatus(logic.Game, type, args.caster);
        }
        
        public override void DoEffectNoTarget(GameLogic logic, AbilityArgs args)
        {
            FindCardWithStatusAndRemoveStatus(logic.Game, type, args.caster);
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
                    }
                }
            }
        }
        
    }
}