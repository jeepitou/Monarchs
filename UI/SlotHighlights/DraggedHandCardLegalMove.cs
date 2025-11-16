using System.Collections.Generic;
using Monarchs.Client;
using Monarchs.Logic;
using TcgEngine;
using TcgEngine.Client;

namespace Monarchs
{
    public class DraggedHandCardLegalMove : SlotHighlight
    {
        private List<BoardSlot> _legalMoveSlots = new List<BoardSlot>();
        
        public DraggedHandCardLegalMove(SlotHighlightManager manager) : base(manager)
        {
            manager.onSlotHovered += OnSlotHover;
        }

        private void OnSlotHover(BoardSlot slot, Card card)
        {
            SetStatus(_legalMoveSlots, false);
            _legalMoveSlots.Clear();
            
            if (card != null || manager.DraggedHandCard == null || slot==null)
            {
                return;
            }
            
            Card draggedCard = manager.DraggedHandCard;
            if (draggedCard.CardData.cardType != CardType.Character)
            {
                return;
            }
            
            _legalMoveSlots = BoardSlot.GetBoardSlotsFromCoordinates(draggedCard.GetLegalMovesFromSlot(slot.GetSlot(),GameClient.GetGameData(), true));
            
            SetStatus(_legalMoveSlots, true);
        }
        
        public override void Unsubscribe()
        {
            manager.onSlotHovered -= OnSlotHover;
        }

        protected override void SetHighlightType()
        {
            highlightType = SlotHighlightTypes.DraggedHandCardLegalMove;
        }
    }
}