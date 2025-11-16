using System.Collections.Generic;
using System.Linq;
using Monarchs.Ability;
using Monarchs.Client;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using TcgEngine.Client;

namespace Monarchs
{
    public class SelectorHighlights : SlotHighlight
    {
        private List<BoardSlot> _lastHighlightedSlots = new List<BoardSlot>();
        public SelectorHighlights(SlotHighlightManager manager) : base(manager)
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
            
            if (selector == SelectorType.SelectCaster)
            {
                _lastHighlightedSlots = BoardSlot.GetBoardSlotWithTheseCards(game.selectorPotentialCasters.ToList(), game);
            }
            
            Player player = GameClient.Get().GetPlayer();
            if (selector == SelectorType.SelectTarget && player.playerID == game.selectorPlayer)
            {
                Card castedCard = game.GetCard(game.selectorCastedCardUID);
                Card caster = game.GetCard(game.selectorCasterUID);
                AbilityData ability = AbilityData.Get(game.selectorAbilityID);

                if (ability != null)
                {
                    List<Card> possibleCasters = new List<Card>();
                    if (caster != null)
                    {
                        possibleCasters.Add(caster);
                    }
                    else
                    {
                        possibleCasters = game.GetPossibleCastersForCard(castedCard);
                    }
                    _lastHighlightedSlots = BoardSlot.GetBoardSlotsFromSlots(ability.GetAllSlotsThatCanBeTargeted(possibleCasters,game, new AbilityArgs(){ability = ability, castedCard = castedCard}));
                }
            }

            if (selector == SelectorType.SelectMultipleTarget && player.playerID == game.selectorPlayer)
            {
                Card castedCard = game.GetCard(game.selectorCardUID);
                AbilityData ability = AbilityData.Get(game.selectorAbilityID);
                
                if (ability != null)
                {
                    _lastHighlightedSlots = BoardSlot.GetBoardSlotsFromSlots(ability.GetAllSlotsThatCanBeTargeted(game.GetPossibleCastersForCard(castedCard),game, new AbilityArgs(){ability = ability, castedCard = castedCard}, game.selectorTargets.Count));
                }
            }
            
            SetStatus(_lastHighlightedSlots, true);
        }

        protected override void SetHighlightType()
        {
            highlightType = SlotHighlightTypes.IsSelectorPotentialChoice;
        }
    
        public override void Unsubscribe()
        {
            GameClient.Get().onRefreshAll -= OnRefreshAll;
        }
    }
}