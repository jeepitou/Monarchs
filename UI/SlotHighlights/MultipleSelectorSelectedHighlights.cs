using System.Collections.Generic;
using System.Linq;
using Monarchs.Ability;
using Monarchs.Client;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using TcgEngine.Client;

namespace Monarchs
{
    public class MultipleSelectorSelectedHighlights : SlotHighlight
    {
        private List<BoardSlot> _lastHighlightedSlots = new List<BoardSlot>();
        public MultipleSelectorSelectedHighlights(SlotHighlightManager manager) : base(manager)
        {
            SetHighlightType();
            GameClient.Get().onRefreshAll += OnRefreshAll;
        }

        private void OnRefreshAll()
        {
            Game game = GameClient.GetGameData();
            SelectorType selector = game.selector;
            SetStatus(_lastHighlightedSlots, false);
            _lastHighlightedSlots.Clear();
            Player player = GameClient.Get().GetPlayer();

            if (selector == SelectorType.SelectMultipleTarget && player.playerID == game.selectorPlayer)
            {
                foreach (var selected in game.selectorTargets)
                {
                    if (selected != null)
                    {
                        _lastHighlightedSlots.Add(BoardSlot.Get(selected.GetSlot()));
                    }
                }
            }
            
            SetStatus(_lastHighlightedSlots, true);
        }

        protected override void SetHighlightType()
        {
            highlightType = SlotHighlightTypes.IsSelected;
        }
    
        public override void Unsubscribe()
        {
            GameClient.Get().onRefreshAll -= OnRefreshAll;
        }
    }
}