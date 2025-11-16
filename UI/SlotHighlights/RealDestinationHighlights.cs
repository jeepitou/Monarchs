using Monarchs.Client;
using Monarchs.Logic;
using TcgEngine.Client;

namespace Monarchs
{
    public class RealDestinationHighlights : SlotHighlight
    {
        private BoardSlot _lastHighlightedSlot;
        
        public RealDestinationHighlights(SlotHighlightManager manager) : base(manager)
        {
            manager.onSlotHovered += OnHover;
        }

        private void OnHover(BoardSlot slot, Card card)
        {
            SetStatus(_lastHighlightedSlot, false);

            Game game = GameClient.GetGameData();
            if (!game.GetCurrentCardTurn().Contains(manager.DraggedBoardCard) || !IsYourTurn)
            {
                return;
            }
            
            PieceMoves pieceMove = game.pieceMovesList[manager.DraggedBoardCard.uid];
            Vector2S realDestination = Vector2S.zero;
            
            if (slot != null && pieceMove.realDestination.ContainsKey(slot.GetCoordinate()))
            {
                realDestination = pieceMove.realDestination[slot.GetCoordinate()];
                _lastHighlightedSlot = BoardSlot.Get(realDestination);
                SetStatus(_lastHighlightedSlot, true);
            }
        }

        public override void Unsubscribe()
        {
            manager.onSlotHovered -= OnHover;
        }

        protected override void SetHighlightType()
        {
            highlightType = SlotHighlightTypes.RealDestination;
        }
    }
}