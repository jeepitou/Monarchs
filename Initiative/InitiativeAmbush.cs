using System;
using System.Collections.Generic;
using Monarchs.Logic;

namespace Monarchs.Initiative
{
    [Serializable]
    public class InitiativeAmbush
    {
        private InitiativeManager _initiativeManager;
        private InitiativeOrder _initiativeOrder;
        
        public InitiativeAmbush(InitiativeManager initiativeManager, InitiativeOrder initiativeOrder)
        {
            _initiativeManager = initiativeManager;
            _initiativeOrder = initiativeOrder;
        }
        
        public virtual void AddAmbushToCard(Card card)
        {
            List<CardInitiativeId> cardInitiativeIds = _initiativeManager.GetInitiativeOrder();
            CardInitiativeId cardInitiativeId = cardInitiativeIds.Find(cardId => cardId.cohortUid == card.CohortUid);
            
            if (cardInitiativeId == null)
            {
                return;
            }
            
            cardInitiativeId.active = true;

            if (_initiativeManager.GetCurrentTurnInitiativeId().initiative <= cardInitiativeId.initiative)
            {
                cardInitiativeId.isAmbushing = true;
                _initiativeOrder.ForceRemoveCard(cardInitiativeId, true);
                _initiativeOrder.AddCardInitiativeIdToSpecificIndex(cardInitiativeId, _initiativeManager.GetCurrentTurnIndex()+1);
            }
              
        }
    
        public virtual void ReplaceAmbushingCard(CardInitiativeId card)
        {
            card.isAmbushing = false;
            _initiativeOrder.ForceRemoveCard(card, true);
            _initiativeOrder.AddCardInitiativeId(card);
        }
    }
}