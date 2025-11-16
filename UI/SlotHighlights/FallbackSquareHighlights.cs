using Monarchs.Client;
using Monarchs.Logic;
using TcgEngine;
using TcgEngine.Client;

namespace Monarchs
{
    public class FallbackSquareHighlights:SlotHighlight
    {
        private BoardSlot _lastFallBackSquare;
        private BoardSlot _lastHoveredSlot;
        
        public FallbackSquareHighlights(SlotHighlightManager manager) : base(manager)
        {
            manager.onSlotHovered += OnHover;
        }

        private void OnHover(BoardSlot slot, Card card)
        {
            SetStatus(_lastFallBackSquare, false);
            _lastHoveredSlot = slot;
            _lastFallBackSquare = null;
            Card draggedCard = manager.DraggedBoardCard;
            
            if (draggedCard == null || slot == null || card == null)
            {
                return;
            }

            Game game = GameClient.GetGameData();
            if (game.CanAttackTarget(draggedCard, card))
            {
                if (draggedCard.GetAttack() >= card.GetHP() + card.GetArmor())
                {
                    return;
                }

                if (GameplayData.Get().DragAndDropRangedUnitWillDoARangedAttack && game.CanRangeAttackTarget(draggedCard, card))
                {
                    return;
                }
                
                Vector2S fallbackSquare = draggedCard.GetCurrentMovementScheme()
                    .GetClosestAvailableSquaresOnMoveTrajectory(draggedCard.GetCoordinates(),
                        card.GetCoordinates(),game)[0];
                _lastFallBackSquare = BoardSlot.Get(fallbackSquare);
                SetStatus(_lastFallBackSquare, true);
            }
        }

        public override void Unsubscribe()
        {
            manager.onSlotHovered -= OnHover;
        }

        protected override void SetHighlightType()
        {
            highlightType = SlotHighlightTypes.IsFallBackSquare;
        }
    }
}