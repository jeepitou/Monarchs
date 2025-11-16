using System.Collections.Generic;
using Monarchs.Client;
using Monarchs.Logic;
using TcgEngine;
using TcgEngine.Client;

namespace Monarchs
{
    public class HoveredCardLegalMoveHighlights : SlotHighlight
    {
        private List<BoardSlot> _legalMoveSlots = new List<BoardSlot>();
        private const bool SHOW_LEGAL_MOVE_OF_HOVERED_INITIATIVE_CARD = true;
        
        public HoveredCardLegalMoveHighlights(SlotHighlightManager manager) : base(manager)
        {
            manager.onSlotHovered += OnSlotHover;
            
            if (SHOW_LEGAL_MOVE_OF_HOVERED_INITIATIVE_CARD)
            {
                manager.onInitiativeCardHovered += OnInitiativeCardHover;
            }
        }

        private void OnSlotHover(BoardSlot slot, Card card)
        {
            SetStatus(_legalMoveSlots, false);
            _legalMoveSlots.Clear();
            
            if (card == null || manager.DraggedHandCard != null || manager.DraggedBoardCard != null)
            {
                return;
            }
            
            _legalMoveSlots = BoardSlot.GetBoardSlotsFromCoordinates(GameClient.GetGameData().GetCardLegalMove(card));
            
            SetStatus(_legalMoveSlots, true);
        }
        
        private void OnInitiativeCardHover(Card card)
        {
            SetStatus(_legalMoveSlots, false);
            _legalMoveSlots.Clear();
            
            if (SHOW_LEGAL_MOVE_OF_HOVERED_INITIATIVE_CARD && card != null)
            {
                List<Card> cohortCards = GameClient.GetGameData().GetBoardCardsOfCohort(card.CohortUid);

                foreach (var cohortCard in cohortCards)
                {
                    _legalMoveSlots.AddRange(BoardSlot.GetBoardSlotsFromCoordinates(GameClient.GetGameData().GetCardLegalMove(cohortCard)));
                }
            
                SetStatus(_legalMoveSlots, true);
            }
        }

        public override void Unsubscribe()
        {
            manager.onSlotHovered -= OnSlotHover;
            if (SHOW_LEGAL_MOVE_OF_HOVERED_INITIATIVE_CARD)
            {
                manager.onInitiativeCardHovered -= OnInitiativeCardHover;
            }
        }

        protected override void SetHighlightType()
        {
            highlightType = SlotHighlightTypes.HoveredCardLegalMove;
        }
    }
}