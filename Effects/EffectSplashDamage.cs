using System;
using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Effect that damages a card or a player (lose hp)
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/SplashDamage", order = 10)]
    public class EffectSplashDamage : EffectData
    {
        public MovementScheme splashDamagePattern;
        public bool friendlyFire;
        public bool halfDamage;
        public int range;

        public override void DoEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            args.target = args.CardTarget.slot;
            DoEffectSlotTarget(logic, args);
        }
        
        public override void DoEffectSlotTarget(GameLogic logic, AbilityArgs args)
        {
            if (args.caster.HasStatus(StatusType.Sabotage))
            {
                return;
            }
            
            Vector2S[] coordinates =
                splashDamagePattern.GetAllSquaresOnMovementScheme(args.SlotTarget.GetCoordinate(), range, logic.Game, args.caster.playerID);

            foreach (var coordinate in coordinates)
            {
                Card card = logic.Game.GetSlotCard(Slot.Get(coordinate.x, coordinate.y));
                if (card != null)
                {
                    if (!friendlyFire && card.playerID == args.caster.playerID)
                    {
                        continue;
                    }
                    
                    int damage = args.caster.GetAttack();

                    if (halfDamage)
                    {
                        damage = (int)Math.Ceiling((double)damage/2);
                    }
                    
                    logic.DamageCard(args.caster, card, damage);
                }
            }
        }
        
    }
}