using System.Collections.Generic;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using UnityEngine;

namespace TcgEngine
{
    /// <summary>
    /// Effect switch between different movement types. First movement type in the list should be the default one.
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/MovementSwitch", order = 10)]
    public class MovementSwitch : EffectData
    {
        public List<MovementScheme> switchToMovements = new List<MovementScheme>();
        public override void DoEffectCardTarget(GameLogic logic, AbilityArgs args) //Target is opponent
        {
            MovementScheme currentMovement = args.CardTarget.GetCurrentMovementScheme();
            int currentIndex = switchToMovements.IndexOf(currentMovement);
            int nextIndex = (currentIndex + 1) % switchToMovements.Count;
            MovementScheme nextMovement = switchToMovements[nextIndex];

            logic.ChangeMovementScheme(args.CardTarget, nextMovement);
        }
    }
}