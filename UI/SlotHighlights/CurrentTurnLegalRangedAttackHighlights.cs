using System.Collections.Generic;
using Monarchs.Client;
using Monarchs.Logic;
using TcgEngine.Client;

namespace Monarchs
{
    public class CurrentTurnLegalRangedAttackHighlights : SlotHighlight
    {
        List<BoardSlot> _possibleRangeAttack = new List<BoardSlot>();
        
        public CurrentTurnLegalRangedAttackHighlights(SlotHighlightManager manager) : base(manager)
        {
            GameClient.Get().onRefreshAll += OnRefreshAll;
        }

        private void OnRefreshAll()
        {
            SetStatus(_possibleRangeAttack, false);
            _possibleRangeAttack.Clear();
            
            _possibleRangeAttack = BoardSlot.GetBoardSlotsFromCoordinates(GameClient.GetGameData().GetLegalRangedAttacks().ToArray());
            
            SetStatus(_possibleRangeAttack, true);
        }

        public override void Unsubscribe()
        {
            GameClient.Get().onRefreshAll -= OnRefreshAll;
        }

        protected override void SetHighlightType()
        {
            highlightType = SlotHighlightTypes.IsLegalRangedAttackOfCurrentTurn;
        }
    }
}