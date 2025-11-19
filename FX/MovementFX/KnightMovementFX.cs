using System.Collections;
using DG.Tweening;
using Monarchs.Client;
using UnityEngine;

namespace Monarchs.FX.MovementFX
{
    public class KnightMovementFX : MovementFX
    {
        protected override IEnumerator DoSpecificMove(BoardCard boardCard, Vector3 endPos, float duration)
        {
            float distanceTo = (boardCard.transform.position - endPos).magnitude;
            float jumpHeight = 0.5f + 0.2f * distanceTo;
            
            yield return boardCard.transform.DOJump(endPos, jumpHeight, 1, duration).SetEase(Ease.InOutSine).SetAutoKill(false).WaitForCompletion();
        }
        
        protected override IEnumerator DoFallBackMove(BoardCard boardCard, BoardCard target)
        {
            Vector2S fallbackSquare = boardCard.GetCard().GetCurrentMovementScheme()
                .GetClosestAvailableSquaresOnMoveTrajectory(boardCard.GetCard().GetCoordinates(),
                    target.GetCard().GetCoordinates(), GameClient.GetGameData())[0];
            Vector3 fallbackPos = BoardSlot.Get(fallbackSquare).transform.position;
            
            yield return boardCard.transform.DOMove(fallbackPos, 0.3f);
        }
        
        protected override IEnumerator DoAttackMove(BoardCard boardCard, BoardCard target, Vector3 currentPos, Vector3 dir)
        {
            yield return boardCard.transform
                .DOJump(target.transform.position - dir.normalized * 0.1f, 1.5f, 1, 0.3f).WaitForCompletion();
        }

        protected override IEnumerator DoAttackMoveWithKillAndNoDyingWish(BoardCard boardCard, BoardCard target, Vector3 currentPos, Vector3 dir)
        {
            yield return boardCard.transform
                .DOJump(target.transform.position, 1.5f, 1, 0.3f).WaitForCompletion();
        }
    }
}