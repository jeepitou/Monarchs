using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Monarchs.Ability;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using TcgEngine;
using TcgEngine.Client;
using UnityEngine;

/// <summary>
/// This condition is used with Troll Prankster Throw Ally ability
/// It checks if 
/// </summary>

[CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/ConditionCardHasFallbackSquareAsIfTargetWasAttacking")]
public class ConditionCardHasFallbackSquareAsIfTargetWasAttacking : ConditionData
{
    public int targetNumberThatIsAttacking;
    public int maxRangeFromTarget;
    
    public override bool IsTargetConditionMetCardTarget(Game data, AbilityArgs args)
    {
        Card cardAttacker = data.selectorTargets[targetNumberThatIsAttacking - 1] as Card;
        if (args.CardTarget.GetHP() <= cardAttacker.GetAttack())
        {
            return true;
        }
        
        
        Vector2S[] moveTargets =
            args.caster.GetCurrentMovementScheme().GetClosestAvailableSquaresOnMoveTrajectory(
                args.caster.GetCoordinates(), args.CardTarget.GetCoordinates(), data);

        if (moveTargets == null)
        {
            return false;
        }
        
        Slot moveTargetSlot = Slot.Get(moveTargets[0]);
        if (data.GetSlotCard(moveTargetSlot) != null)
        {
            return false;
        }
        
        if (!args.CardTarget.slot.IsInDistance(moveTargetSlot, maxRangeFromTarget))
        {
            return false;
        }

        return true;
    }
}