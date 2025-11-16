using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using TcgEngine;
using UnityEngine;

namespace Monarchs.Effects
{
    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/EffectMoveToSquareAsIfTargetWasAttacking", order = 10)]
    public class EffectMoveToSquareAsIfTargetWasAttacking : EffectData
    {
        public int targetNumber;
        public bool damageIsAppliedBeforeChecking;
        
        public override void DoEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            Slot slotTarget;
            Card cardTarget = logic.Game.selectorTargets[targetNumber - 1] as Card;
            if ((cardTarget.GetHP() <= args.CardTarget.GetAttack() && !damageIsAppliedBeforeChecking) 
                || cardTarget.GetHP() <= 0)
            {
                slotTarget = cardTarget.slot;
            }
            else
            {
                Vector2S moveTarget =
                    args.caster.GetCurrentMovementScheme().GetClosestAvailableSquaresOnMoveTrajectory(
                        args.caster.GetCoordinates(), cardTarget.GetCoordinates(), logic.Game)[0];
                slotTarget = Slot.Get(moveTarget);
            }
            logic.ForceMoveCard(args.CardTarget, slotTarget, true, false);
        }
    }
}
