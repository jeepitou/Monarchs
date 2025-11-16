using System.Collections.Generic;
using Monarchs.Client;
using Monarchs.Logic;
using TcgEngine.Client;

namespace Monarchs
{
    public class DraggedCardLegalMoveHighlights : SlotHighlight
    {
        private List<BoardSlot> _legalMoveSlots = new List<BoardSlot>();
        
        public DraggedCardLegalMoveHighlights(SlotHighlightManager manager) : base(manager)
        {
            manager.onDraggedBoardCardChanged += OnDraggedBoardCardChanged;
        }

        private void OnDraggedBoardCardChanged(Card card)
        {
            SetStatus(_legalMoveSlots, false);
            _legalMoveSlots.Clear();
            
            if (card == null || !IsYourTurn)
            {
                return;
            }

            if (!GameClient.GetGameData().CardCanMoveThisTurn(card))
            {
                return;
            }
            
            _legalMoveSlots = BoardSlot.GetBoardSlotsFromCoordinates(GameClient.GetGameData().GetCardLegalMove(card));
            
            SetStatus(_legalMoveSlots, true);
            
        }

        public override void Unsubscribe()
        {
            manager.onDraggedBoardCardChanged -= OnDraggedBoardCardChanged;
        }

        protected override void SetHighlightType()
        {
            highlightType = SlotHighlightTypes.DraggedCardLegalMove;
        }
    }
}