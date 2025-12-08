using System.Collections.Generic;
using Monarchs.Client;
using Monarchs.Logic;
using TcgEngine;
using TcgEngine.Client;

namespace Monarchs
{
    public class HoveredCardLegalRangedAttackHighlights : SlotHighlight
    {
        private List<BoardSlot> _possibleRangeAttack = new List<BoardSlot>();
        private const bool SHOW_LEGAL_RANGED_ATTACK_OF_HOVERED_INITIATIVE_CARD = true;
        private const bool SHOW_LEGAL_RANGED_ATTACK_OF_DRAGGED_HAND_CARD = true;
        
        public HoveredCardLegalRangedAttackHighlights(SlotHighlightManager manager) : base(manager)
        {
            manager.onSlotHovered += OnSlotHovered;
            if (SHOW_LEGAL_RANGED_ATTACK_OF_HOVERED_INITIATIVE_CARD)
            {
                manager.onInitiativeCardHovered += OnInitiativeCardHover;
            }
        }

        private void OnSlotHovered(BoardSlot slot, Card card)
        {
            SetStatus(_possibleRangeAttack, false);
            _possibleRangeAttack.Clear();
            
            if (card == null && manager.DraggedHandCard != null && SHOW_LEGAL_RANGED_ATTACK_OF_DRAGGED_HAND_CARD && slot != null)
            {
                OnDraggedHandCardHover(slot.GetSlot(), manager.DraggedHandCard);
                return;
            }

            if (card != null)
            {
                _possibleRangeAttack = BoardSlot.GetBoardSlotsFromCoordinates(
                    GameClient.GetGameData().GetLegalRangedAttacks(
                        new List<Card>(){card}, true).ToArray());
            }
            
            SetStatus(_possibleRangeAttack, true);
        }
        
        private void OnDraggedHandCardHover(Slot slot, Card card)
        {
            SetStatus(_possibleRangeAttack, false);
            _possibleRangeAttack.Clear();
            
            Card draggedCard = manager.DraggedHandCard;
            if (draggedCard.CardData.cardType != CardType.Character)
            {
                return;
            }
            
            _possibleRangeAttack = BoardSlot.GetBoardSlotsFromCoordinates(draggedCard.GetCurrentMovementScheme().GetLegalRangedAttack(slot.GetCoordinate(),
                draggedCard.CardData.minAttackRange, draggedCard.GetMaxAttackRange(),
                draggedCard.playerID, GameClient.GetGameData(), true, true, true).ToArray());
            
            SetStatus(_possibleRangeAttack, true);
        }
        
        private void OnInitiativeCardHover(Card card)
        {
            SetStatus(_possibleRangeAttack, false);
            _possibleRangeAttack.Clear();
            
            if (SHOW_LEGAL_RANGED_ATTACK_OF_HOVERED_INITIATIVE_CARD && card != null)
            {
                List<Card> cohortCards = GameClient.GetGameData().GetBoardCardsOfCohort(card.CohortUid);

                _possibleRangeAttack = BoardSlot.GetBoardSlotsFromCoordinates(GameClient.GetGameData().GetLegalRangedAttacks(cohortCards, true).ToArray());
                
                SetStatus(_possibleRangeAttack, true);
            }
        }

        public override void Unsubscribe()
        {
            manager.onSlotHovered -= OnSlotHovered;
            if (SHOW_LEGAL_RANGED_ATTACK_OF_HOVERED_INITIATIVE_CARD)
            {
                manager.onInitiativeCardHovered -= OnInitiativeCardHover;
            }
        }

        protected override void SetHighlightType()
        {
            highlightType = SlotHighlightTypes.IsLegalRangedAttackOfHoveredCard;
        }
    }
}