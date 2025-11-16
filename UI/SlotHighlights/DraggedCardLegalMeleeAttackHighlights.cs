using System.Collections.Generic;
using Monarchs.Client;
using Monarchs.Logic;
using TcgEngine;

namespace Monarchs
{
    public class DraggedCardLegalMeleeAttackHighlights : SlotHighlight
    {
        private List<BoardSlot> _legalMeleeAttackSlots = new List<BoardSlot>();
        
        public DraggedCardLegalMeleeAttackHighlights(SlotHighlightManager manager) : base(manager)
        {
            manager.onDraggedBoardCardChanged += OnDraggedBoardCardChanged;
        }

        private void OnDraggedBoardCardChanged(Card card)
        {
            SetStatus(_legalMeleeAttackSlots, false);
            _legalMeleeAttackSlots.Clear();
            
            if (card == null)
            {
                return;
            }
            
            Game game = GameClient.GetGameData();
            List<Vector2S> legalMeleeAttack =  game.GetLegalMeleeAttacks(card);
            
            foreach (Vector2S slot in legalMeleeAttack) // I validate with CanAttackTarget to take disarm, fear, etc. into account
            {
                if (game.CanAttackTarget(card, game.GetSlotCard(Slot.Get(slot))))
                {
                    _legalMeleeAttackSlots.Add(BoardSlot.Get(slot));
                }
            }
            
            if (GameplayData.Get().DragAndDropRangedUnitWillDoARangedAttack)
            {
                List<Vector2S> legalRangeAttack = game.GetLegalRangedAttacks(new List<Card>(){card});
                foreach (Vector2S slot in legalRangeAttack)
                {
                    _legalMeleeAttackSlots.Add(BoardSlot.Get(slot));
                }
            }
            
            SetStatus(_legalMeleeAttackSlots, true);
        }

        public override void Unsubscribe()
        {
            manager.onDraggedBoardCardChanged -= OnDraggedBoardCardChanged;
        }

        protected override void SetHighlightType()
        {
            highlightType = SlotHighlightTypes.IsLegalMeleeAttack;
        }
    }
}