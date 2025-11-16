using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Client;
using Monarchs.Logic;
using TcgEngine;
using TcgEngine.Client;

namespace Monarchs
{
    public class SpellSelfHighlights : SlotHighlight
    {
        private List<BoardSlot> _affectedBySpellSlotList = new List<BoardSlot>();
        
        public SpellSelfHighlights(SlotHighlightManager manager) : base(manager)
        {
            manager.onSlotHovered += OnHover;
        }

        private void OnHover(BoardSlot slot, Card card)
        {
            Game game = GameClient.GetGameData();
            Card handCardDragged = manager.DraggedHandCard;
            SetStatus(_affectedBySpellSlotList, false);
            _affectedBySpellSlotList.Clear();
            
            if (handCardDragged == null || !IsYourTurn || slot == null) return;
            
            bool hasSpellThatTargetSelf = handCardDragged.CardData.HasAbility(AbilityTrigger.OnPlay, AbilityTargetType.Self);
            
            if (!hasSpellThatTargetSelf)
            {
                return;
            }

            Card caster = game.GetCurrentCardTurn()[0];
            CardData spell = handCardDragged.CardData;
            _affectedBySpellSlotList = BoardSlot.GetBoardSlotsFromSlots(spell.GetAllCurrentAbilities()[0].GetSlotTargets(game, caster));
            
            SetStatus(_affectedBySpellSlotList, true);
        }

        protected override void SetHighlightType()
        {
            highlightType = SlotHighlightTypes.IsInSelfSpell;
        }

        public override void Unsubscribe()
        {
            manager.onSlotHovered -= OnHover;
        }
    }
   
}