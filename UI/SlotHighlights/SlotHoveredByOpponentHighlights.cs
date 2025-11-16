using Monarchs.Client;
using Monarchs.Logic;
using TcgEngine.Client;

namespace Monarchs
{
    public class SlotHoveredByOpponentHighlights:SlotHighlight
    {
        private BoardSlot _lastHoveredSlotByOpponent;
        public SlotHoveredByOpponentHighlights(SlotHighlightManager manager) : base(manager)
        {
            GameClient.Get().onBoardSlotHoveredByOpponent += OnBoardSlotHoveredByOpponent;
        }
        
        public override void Unsubscribe()
        {
            GameClient.Get().onBoardSlotHoveredByOpponent -= OnBoardSlotHoveredByOpponent;
        }

        protected override void SetHighlightType()
        {
            highlightType = SlotHighlightTypes.HoveredByOpponent;
        }

        private void OnBoardSlotHoveredByOpponent(Slot slot)
        {
            SetStatus(_lastHoveredSlotByOpponent, false);
            SetStatus(BoardSlot.Get(slot), true);
            
            _lastHoveredSlotByOpponent = BoardSlot.Get(slot);
        }
    }
}