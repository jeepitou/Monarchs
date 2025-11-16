using System.Collections.Generic;
using Monarchs.Client;
using Monarchs.Logic;
using TcgEngine;
using TcgEngine.Client;

namespace Monarchs
{
    public class SplashDamageHighlights : SlotHighlight
    {
        private List<BoardSlot> _lastHighlightedSlots = new List<BoardSlot>();
        
        public SplashDamageHighlights(SlotHighlightManager manager) : base(manager)
        {
            manager.onSlotHovered += OnHover;
        }
        
        private void OnHover(BoardSlot slot, Card card)
        {
            Game game = GameClient.GetGameData();
            
            SetStatus(_lastHighlightedSlots, false);
            _lastHighlightedSlots.Clear();

            if (manager.DraggedBoardCard != null || !IsYourTurn || slot == null)
            {
                return;
            }

            if (!game.GetLegalRangedAttacks().Contains(slot.GetCoordinate()))
            {
                return;
            }
            
            EffectSplashDamage splashDamage = game.GetCurrentCardTurn()[0].CardData.rangedSplashDamageForHighlights;
            if (splashDamage != null)
            {
                Vector2S[] list = splashDamage.splashDamagePattern.GetAllSquaresOnMovementScheme(slot.GetCoordinate(), splashDamage.range, game, GameClient.Get().GetPlayerID());
                _lastHighlightedSlots = BoardSlot.GetBoardSlotsFromCoordinates(list);
            }
            
            SetStatus(_lastHighlightedSlots, true);
        }

        public override void Unsubscribe()
        {
            manager.onSlotHovered -= OnHover;
        }

        protected override void SetHighlightType()
        {
            highlightType = SlotHighlightTypes.IsInSplashDamageArea;
        }
    }
}