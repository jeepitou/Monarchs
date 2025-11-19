using System.Collections;
using DG.Tweening;
using Monarchs.Ability;
using Monarchs.Client;
using Monarchs.Logic;
using UnityEngine;

//TODO Rework GameBoardFX to simplify it and merge the TriggerAnimation that is currently in AnimationManager.cs with GameBoardFX
//TODO Figure out how to manage the position of the pieces when the Dying Wish ability is triggered
//TODO Validate if slot is available after killing a piece with Dying Wish ability, and move to fallback position if not
namespace Monarchs.FX.MovementFX
{
    public class MovementFX
    {
        private const float DYING_WISH_ANIMATION_DELAY = 2f;
        private const float DYING_WISH_ANIMATION_DURATION = 0.2f;
        private const float DYING_WISH_ANIMATION_OFFSET = 0.8f;
        protected KnightMovementFX _knightMovementFX;
        protected IncorporealMovementFX _incorporealMovementFX;
        
        
        public virtual MovementFX GetMovementFX(BoardCard boardCard)
        {
            if (boardCard.GetCard().CardData.HasTrait("incorporeal"))
            {
                return _incorporealMovementFX != null ? _incorporealMovementFX : (_incorporealMovementFX = new IncorporealMovementFX());
            }
            else if (boardCard.GetCard().CardData.GetPieceType() == PieceType.Knight)
            {
                return _knightMovementFX != null ? _knightMovementFX : (_knightMovementFX = new KnightMovementFX());
            }
            return this;
        }
        
        public IEnumerator DoMove(BoardCard boardCard, Slot slot)
        {
            int distanceTo = boardCard.GetCard().slot.GetDistanceTo(slot);
            Vector3 endPos = BoardSlot.Get(slot).transform.position;
            if (endPos == boardCard.transform.position)
                yield break;
            float duration = 0.3f + (0.1f * distanceTo);
            yield return DoSpecificMove(boardCard, endPos, duration);
        }
        
        
        public virtual IEnumerator ChargeInto(BoardCard boardCard, BoardCard target, bool killed = false)
        {
            if (target != null)
            {
                Vector3 dir = target.transform.position - boardCard.transform.position;
                Vector3 currentPos = boardCard.transform.position;

                if (killed && !target.GetCardData().HasAbility(AbilityTrigger.OnDeath))
                {
                    yield return DoAttackMoveWithKillAndNoDyingWish(boardCard, target, currentPos, dir);
                }
                else
                {
                    yield return DoAttackMove(boardCard, target, currentPos, dir);
                }
                
                if (!killed) // When the piece isn't killed, piece moves back to fallback square
                {
                    EnemyRecoilAfterAttack(boardCard, target);
                    
                    boardCard.StartCoroutine(DoFallBackMove(boardCard, target));
                }
                else if (target.GetCardData().HasAbility(AbilityTrigger.OnDeath))
                {
                    WaitForDyingWishAnimation(boardCard, target, dir);
                }
            }
        }
        
        public virtual IEnumerator OnAttackEnd(BoardCard boardCard)
        {
            if (boardCard.transform.position != BoardSlot.Get(boardCard.GetCard().slot).transform.position)
            {
                Vector3 fallbackPos = BoardSlot.Get(boardCard.GetCard().slot).transform.position;
                yield return boardCard.transform.DOMove(fallbackPos, 0.3f).SetEase(Ease.InOutSine).WaitForCompletion();
            }
        }
        
        protected virtual IEnumerator DoSpecificMove(BoardCard boardCard, Vector3 endPos, float duration)
        {
            yield return boardCard.transform.DOMove(endPos, duration).SetEase(Ease.InOutSine).WaitForCompletion();
        }

        protected virtual IEnumerator DoFallBackMove(BoardCard boardCard, BoardCard target)
        {
            Vector3 fallbackPos = BoardSlot.Get(boardCard.GetCard().slot).transform.position;
            yield return boardCard.transform.DOMove(fallbackPos, 0.3f);
        }
        
        protected virtual void WaitForDyingWishAnimation(BoardCard boardCard, BoardCard target, Vector3 dir)
        {
            boardCard.transform.DOMove(target.transform.position - dir.normalized * DYING_WISH_ANIMATION_OFFSET, DYING_WISH_ANIMATION_DURATION).WaitForCompletion();
        }
        
        protected virtual IEnumerator DoAttackMoveWithKillAndNoDyingWish(BoardCard boardCard, BoardCard target, Vector3 currentPos, Vector3 dir)
        {
            yield return boardCard.transform.DOMove(currentPos - dir.normalized * 0.5f, 0.3f).WaitForCompletion();
            yield return boardCard.transform.DOMove(target.transform.position, 0.1f).WaitForCompletion();
        }

        protected virtual IEnumerator DoAttackMove(BoardCard boardCard, BoardCard target, Vector3 currentPos, Vector3 dir)
        {
            yield return boardCard.transform.DOMove(currentPos - dir.normalized * 0.5f, 0.3f).WaitForCompletion();
            yield return boardCard.transform.DOMove(target.transform.position - dir.normalized*0.1f, 0.1f).WaitForCompletion();
            boardCard.transform.DOMove(target.transform.position - dir.normalized*0.2f, 0.05f);
        }
        
        protected void EnemyRecoilAfterAttack(BoardCard boardCard, BoardCard target)
        {
            Vector3 dir = target.transform.position - boardCard.transform.position;
            target.transform.DOMove(target.transform.position + dir.normalized * 0.2f, 0.1f).OnComplete(
                () => { target.transform.DOMove(target.transform.position - dir.normalized * 0.2f, 0.1f); }
            );
        }
        
        protected IEnumerator MoveToPosition(BoardCard boardCard, Vector3 position, float duration, float delay = 0f)
        {
            if (delay > 0f)
                yield return new WaitForSeconds(delay);
            yield return boardCard.transform.DOMove(position, duration).SetEase(Ease.InOutSine).WaitForCompletion();
        }
    }
}