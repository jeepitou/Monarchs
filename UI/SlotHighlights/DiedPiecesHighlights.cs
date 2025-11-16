using System.Collections.Generic;
using System.Linq;
using Monarchs.Ability;
using Monarchs.Client;
using Monarchs.Logic;
using Monarchs.Logic.AbilitySystem;
using TcgEngine.Client;

namespace Monarchs
{
    public class DiedPiecesHighlights : SlotHighlight
    {
        private List<BoardSlot> _lastHighlightedSlots = new List<BoardSlot>();
        private bool _activated = false;
        public DiedPiecesHighlights(SlotHighlightManager manager) : base(manager)
        {
            GameClient.Get().onNewTurn += OnNewTurn;
        }

        private void OnNewTurn(Card card)
        {
            SetStatus(_lastHighlightedSlots, false);
            _lastHighlightedSlots.Clear();
            _activated = false;
            
            AbilityData abilityThatNeedsDeathHighlights = null;
            foreach (var ability in card.CardData.GetAllCurrentAbilities())
            {
                if (ability.displayDeathHighlights)
                {
                    _activated = true;
                    abilityThatNeedsDeathHighlights = ability;
                    break;
                }
            }
            
            if (!_activated)
            {
                return;
            }
            
            Game game = GameClient.GetGameData();
            foreach (var player in game.players)
            {
                foreach (var discarded in player.cards_discard)
                {
                    if (discarded.wasOnBoard)
                    {
                        if (ValidateConditions(discarded, abilityThatNeedsDeathHighlights))
                        {
                            _lastHighlightedSlots.Add(BoardSlot.Get(discarded.slot));
                        }
                    }
                }
            }

            _lastHighlightedSlots = _lastHighlightedSlots.Distinct().ToList();
            SetStatus(_lastHighlightedSlots, true);
        }
        
        private bool ValidateConditions(Card card, AbilityData ability)
        {
            Game game = GameClient.GetGameData();
            foreach (var condition in ability.deathHighlightsConditions)
            {
                AbilityArgs args = new AbilityArgs();
                args.target = card;
                args.ability = ability;

                if (!condition.IsTargetConditionMet(game, args))
                {
                    return false;
                }
            }

            return true;
        }

        public override void Unsubscribe()
        {
            GameClient.Get().onNewTurn -= OnNewTurn;
        }

        protected override void SetHighlightType()
        {
            highlightType = SlotHighlightTypes.PieceDiedOnSlot;
        }
    }
}