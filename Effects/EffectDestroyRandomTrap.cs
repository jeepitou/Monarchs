using System;
using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/DestroyRandomTrap")]
    public class EffectDestroyRandomTrap : EffectData
    {
        public override void DoEffectPlayerTarget(GameLogic logic, AbilityArgs args)
        {
            int trapCount = args.PlayerTarget.cards_trap.Count;

            if (trapCount == 0)
            {
                return;
            }

            System.Random random = new System.Random();

            int index = random.Next(0, trapCount);
            Card trap = args.PlayerTarget.cards_trap[index];
            args.PlayerTarget.RemoveCard(args.PlayerTarget.cards_trap, trap);
            
            logic.DiscardCard(trap);

        }

        public override int GetAiValue(AbilityData ability)
        {
            return -1;
        }
    }
}