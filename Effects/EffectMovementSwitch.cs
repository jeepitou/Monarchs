using System.Collections.Generic;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using TcgEngine;
using UnityEngine;

namespace Monarchs.Effects
{
    /// <summary>
    /// Effect switch between different movement types. First movement type in the list should be the default one.
    /// </summary>
    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/MovementSwitch", order = 10)]
    public class EffectMovementSwitch : EffectData
    {
        public List<MovementScheme> switchToMovements = new List<MovementScheme>();
        public override void DoEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            if (switchToMovements == null || switchToMovements.Count == 0)
            {
                Debug.LogError("MovementSwitch: switchToMovements list is empty. Cannot switch movement scheme.");
                return;
            }
            
            MovementScheme currentMovement = args.CardTarget.GetCurrentMovementScheme();
            int currentIndex = switchToMovements.IndexOf(currentMovement);
            int nextIndex = (currentIndex + 1) % switchToMovements.Count;
            MovementScheme nextMovement = switchToMovements[nextIndex];

            logic.ChangeMovementScheme(args.CardTarget, nextMovement);
        }
    }
}