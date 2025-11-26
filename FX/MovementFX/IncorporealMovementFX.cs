using System.Collections;
using DG.Tweening;
using Monarchs.Client;
using UnityEngine;

namespace Monarchs.FX.MovementFX
{
    public class IncorporealMovementFX : MovementFX
    {
        protected override IEnumerator DoSpecificMove(BoardCard boardCard, Vector3 endPos, float duration)
        {
            yield return RemoveAndReapplyHoverEffectAfterFunction(boardCard, base.DoSpecificMove(boardCard, endPos, duration));
        }
        
        protected override IEnumerator DoFallBackMove(BoardCard boardCard, BoardCard target)
        {
            yield return RemoveAndReapplyHoverEffectAfterFunction(boardCard, base.DoFallBackMove(boardCard, target));
        }
        
        protected override IEnumerator DoAttackMove(BoardCard boardCard, BoardCard target, Vector3 currentPos, Vector3 dir)
        {
            yield return RemoveAndReapplyHoverEffectAfterFunction(boardCard, base.DoAttackMove(boardCard, target, currentPos, dir));
        }
        
        protected override IEnumerator DoAttackMoveWithKillAndNoDyingWish(BoardCard boardCard, BoardCard target, Vector3 currentPos, Vector3 dir)
        {
            yield return RemoveAndReapplyHoverEffectAfterFunction(boardCard, base.DoAttackMoveWithKillAndNoDyingWish(boardCard, target, currentPos, dir));
        }
        
        protected override IEnumerator WaitForDyingWishAnimation(BoardCard boardCard, BoardCard target, Vector3 dir)
        {
            boardCard.GetComponent<PieceOnBoard>().StopIncorporealHoverEffect();
            yield return base.WaitForDyingWishAnimation(boardCard, target, dir);
        }
        
        public override IEnumerator OnAttackEnd(BoardCard boardCard)
        {
            yield return base.OnAttackEnd(boardCard);
            boardCard.GetComponent<PieceOnBoard>().StartIncorporealHoverEffect();
        }
        
        private IEnumerator RemoveAndReapplyHoverEffectAfterFunction(BoardCard boardCard, IEnumerator function)
        {
            boardCard.GetComponent<PieceOnBoard>().StopIncorporealHoverEffect();
            yield return function;
            boardCard.GetComponent<PieceOnBoard>().StartIncorporealHoverEffect();
        }
    }
}