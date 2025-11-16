using System.Collections.Generic;
using Monarchs.Client;
using Monarchs.Logic;

namespace Monarchs
{
    public class InitiativeCardHoveredByPlayerHighlights : SlotHighlight
    {
        private List<BoardSlot> _hoveredInitiativeCohortSlots = new List<BoardSlot>();
        
        public InitiativeCardHoveredByPlayerHighlights(SlotHighlightManager manager) : base(manager)
        {
            manager.onInitiativeCardHovered += OnInitiativeCardHover;
        }

        private void OnInitiativeCardHover(Card card)
        {
            SetStatus(_hoveredInitiativeCohortSlots, false);
            _hoveredInitiativeCohortSlots.Clear();
            
            if (card == null)
            {
                return;
            }

            List<Card> cohortCards = GameClient.GetGameData().GetBoardCardsOfCohort(card.CohortUid);

            foreach (var cohortCard in cohortCards)
            {
                if (cohortCard.slot == Slot.None)
                    continue; 
                _hoveredInitiativeCohortSlots.Add(BoardSlot.Get(cohortCard.slot));
            }
            
            SetStatus(_hoveredInitiativeCohortSlots, true);
        }

        public override void Unsubscribe()
        {
            manager.onInitiativeCardHovered -= OnInitiativeCardHover;
        }

        protected override void SetHighlightType()
        {
            highlightType = SlotHighlightTypes.HoveredInInitiativeCard;
        }
    }
}