using System.Collections.Generic;
using Monarchs.Client;
using Monarchs.Logic;
using TcgEngine;
using TcgEngine.Client;

namespace Monarchs
{
    public class SpellAOEHighlights : SlotHighlight
    {
        private List<BoardSlot> _affectedBySpellSlotList = new List<BoardSlot>();
        
        public SpellAOEHighlights(SlotHighlightManager manager) : base(manager)
        {
            manager.onSlotHovered += OnHover;
        }

        private void OnHover(BoardSlot slot, Card card)
        {
            Game game = GameClient.GetGameData();
            Card handCardDragged = manager.DraggedHandCard;
            SetStatus(_affectedBySpellSlotList, false);
            _affectedBySpellSlotList.Clear();
            
            if (handCardDragged == null || !handCardDragged.CardData.IsRequireTarget() || !IsYourTurn || slot == null)
            {
                return;
            }

            var casters = game.GetPossibleCastersForCard(handCardDragged, slot.GetSlot());
            Card caster = casters.Count > 0 ? casters[0] : null;
            if (caster == null)
            {
                return;
            }
            
            _affectedBySpellSlotList = BoardSlot.GetBoardSlotsFromSlots(Slot.GetSlotsOfTargets(handCardDragged.GetAllCurrentAbilities()[0].GetTargets(game, caster, slot.GetSlot())));
            
            SetStatus(_affectedBySpellSlotList, true);
        }

        protected override void SetHighlightType()
        {
            highlightType = SlotHighlightTypes.IsInSpellAOE;
        }

        public override void Unsubscribe()
        {
            manager.onSlotHovered -= OnHover;
        }
    }
   
}