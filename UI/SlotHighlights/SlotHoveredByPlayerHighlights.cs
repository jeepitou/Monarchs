using Monarchs.Client;
using Monarchs.Logic;

namespace Monarchs.UI.SlotHighlights
{
    public class SlotHoveredByPlayerHighlights:SlotHighlight
    {
        private BoardSlot _lastHoveredSlot;
        
        private BoardSlot _lastHoveredSlotEventSent;
        private bool _showHoverToOpponent = true;
        
        public SlotHoveredByPlayerHighlights(SlotHighlightManager manager) : base(manager)
        {
            manager.onSlotHovered += OnHover;
        }
        
        public override void Unsubscribe()
        {
            manager.onSlotHovered -= OnHover;
        }
        
        private void OnHover(BoardSlot slot, Card card)
        {
            SetStatus(_lastHoveredSlot, false);
            SetStatus(slot, true);
            
            _lastHoveredSlot = slot;

            ShowHoverToOpponent(slot);
        }

        private void ShowHoverToOpponent(BoardSlot boardSlot)
        {
            if (boardSlot == null && manager.DraggedHandCard==null)
            {
                _showHoverToOpponent = true;
            }
            
            if (_lastHoveredSlotEventSent == boardSlot || !_showHoverToOpponent)
            {
                return;
            }

            Slot slot = Slot.None;
            if (boardSlot != null)
            {
                slot = boardSlot.GetSlot();
            }

            if (IsYourTurn)
            {
                if (BoardSlot.ShowBoardHighlightToOpponent())
                {
                    GameClient.Get().HoverSlot(slot);
                }
            }
            else if (_lastHoveredSlotEventSent != null)
            {
                GameClient.Get().HoverSlot(Slot.None);
            }
            
            _lastHoveredSlotEventSent = boardSlot;
        }
        
        protected override void SetHighlightType()
        {
            highlightType = SlotHighlightTypes.HoveredByPlayer;
        }
    }
}