using System.Collections.Generic;
using Monarchs.Client;
using Monarchs.Logic;
using TcgEngine;
using TcgEngine.Client;

namespace Monarchs
{
    public class DraggedHandCardHighlights : SlotHighlight {
        private List<BoardSlot> _lastHighlightedSlots = new List<BoardSlot>();
        
        public DraggedHandCardHighlights(SlotHighlightManager manager) : base(manager) {
            manager.onDraggedHandCardChanged += OnChangeDraggedHandCard;
        }

        private void OnChangeDraggedHandCard(Card card)
        {
            Game game = GameClient.GetGameData();
            
            SetStatus(_lastHighlightedSlots, false);
            _lastHighlightedSlots.Clear();
            
            if (card == null || !IsYourTurn)
            {
                return;
            }
            
            if (card.CardData.cardType != CardType.Character && !card.CardData.IsRequireTarget())
            {
                return;
            }

            _lastHighlightedSlots = BoardSlot.GetBoardSlotsFromSlots(game.GetLegalSlotsToPlayCard(card));
            SetStatus(_lastHighlightedSlots, true);
        }

        public override void Unsubscribe() {
            manager.onDraggedHandCardChanged -= OnChangeDraggedHandCard;
        }

        protected override void SetHighlightType()
        {
            highlightType = SlotHighlightTypes.IsDraggedHandCardPotentialChoice;
        }

    }
}