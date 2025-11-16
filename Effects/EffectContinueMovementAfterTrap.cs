using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using TcgEngine;
using UnityEngine;

namespace Monarchs.Effects
{
    /// <summary>
    /// Effect that continues the piece movement after a trap has been triggered
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/EffectContinueMovementAfterTrap", order = 10)]
    public class EffectContinueMovementAfterTrap : EffectData
    {
        public override void DoEffectCardTarget(GameLogic logic, AbilityArgs args)
        {
            if (args.CardTarget.uid != ((Card)args.triggerer).uid)
            {
                return;
            }
            
            if (args.CardTarget.GetHP() <= 0)
            {
                return;
            }
            
            Card target = (Card)args.triggerer;
            if (logic.Game.lastPlayed?.uid == target.uid)
            {
                return;
            }
            
            Slot moveDestination = logic.Game.lastMoveDestination;
            Card cardOnMoveDestination = logic.Game.GetSlotCard(moveDestination);
            if (target.slot == moveDestination)
            {
                return;
            }
            
            target.exhausted = false;
            target.numberOfMoveThisTurn = 0;

            if (cardOnMoveDestination == null)
            {
                logic.ForceMoveCard(target, moveDestination);
                return;
            }
            
            
            if (cardOnMoveDestination.playerID != target.playerID)
            {
                logic.AttackTarget(target, moveDestination, true);
            }
            else
            {
                Vector2S moveTarget =
                    target.GetCurrentMovementScheme().GetClosestAvailableSquaresOnMoveTrajectory(
                        target.GetCoordinates(), moveDestination.GetCoordinate(), logic.Game)[0];
                logic.ForceMoveCard(target, Slot.Get(moveTarget));
            }
        }
    }
}