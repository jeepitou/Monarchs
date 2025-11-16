using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Monarchs.Ability;
using Monarchs.Initiative;
using Monarchs.Logic;
using TcgEngine;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class InitiativeOrder
{
    private InitiativeManager _initiativeManager;
    private List<CardInitiativeId> _cardUidInOrder;
    
    public InitiativeOrder(InitiativeManager initiativeManager)
    {
        _initiativeManager = initiativeManager;
        _cardUidInOrder = new List<CardInitiativeId>();
    }
    
    public virtual List<CardInitiativeId> GetInitiativeList()
    {
        return _cardUidInOrder;
    }
    
    public virtual void AddCards(Card[] cards)
    {
        foreach (var card in cards)
        {
            AddCard(card);
        }
    }
    
    public virtual void AddCard(Card card, bool canPlayThisTurn = false)
    {
        if (IsThereCardOfSameCohortInInitiativeList(card))
        {
            return;
        }
        
        CardInitiativeId cardInitIdToAdd = new CardInitiativeId()
        {
            initiative = card.GetInitiative(),
            cohortUid = card.CohortUid,
            active = canPlayThisTurn,
            remainingMuster = card.remainingMuster
        };
        
        AddCardInitiativeId(cardInitIdToAdd);
    }

    public virtual int GetAboutToBePlayedCardPosition(Card card, Game game)
    {
        if (card.HasAbility(id:"play_ambush"))
        {
            if (card.GetInitiative() >= _initiativeManager.GetCurrentCardTurn(game)[0].GetInitiative())
            {
                return _initiativeManager.GetCurrentTurnIndex() + 1;
            }
        }
        CardInitiativeId previousCardInOrder = _cardUidInOrder.Find(cardId => card.GetInitiative() > cardId.initiative);
        
        if (previousCardInOrder == null)
        {
            return _cardUidInOrder.Count;
        }

        return _cardUidInOrder.IndexOf(previousCardInOrder);
    }

    public virtual void AddCardInitiativeId(CardInitiativeId card)
    {
        if (_cardUidInOrder.Count == 0)
        {
            _cardUidInOrder.Add(card);
            return;
        }
        
        CardInitiativeId previousCardInOrder = _cardUidInOrder.Find(cardId => card.initiative > cardId.initiative);
        
        if (previousCardInOrder == null)
        {
            _cardUidInOrder.Insert(_cardUidInOrder.Count, card);
            _initiativeManager.OnCardAdded(card, _cardUidInOrder.Count);
            return;
        }

        int index = _cardUidInOrder.IndexOf(previousCardInOrder);
        _cardUidInOrder.Insert(index, card);
        _initiativeManager.OnCardAdded(card, index);
    }
    
    public virtual void AddCardInitiativeIdToSpecificIndex(CardInitiativeId card, int index)
    {
        _cardUidInOrder.Insert(index, card);
        _initiativeManager.OnCardAdded(card, index);
    }

    private bool IsThereCardOfSameCohortInInitiativeList(Card card)
    {
        if (card.CohortUid == "")
        {
            return false;
        }
        
        return _cardUidInOrder.Any(cardInitId => cardInitId.cohortUid == card.CohortUid);
    }
    
    public virtual void RemoveCard(Card card, Game game, bool forceRemove=false)
    {
        if (game.GetBoardCardsOfCohort(card.CohortUid)?.Count > 0 && !forceRemove)
        {
            return;
        }
        
        CardInitiativeId cardId = _cardUidInOrder.Find(cardId => cardId.cohortUid == card.CohortUid);

        if (cardId != null)
        {
            int index = _cardUidInOrder.IndexOf(cardId);
            _cardUidInOrder.Remove(cardId);
            _initiativeManager.OnCardRemoved(cardId, index);
        }
    }
    
    public virtual void ForceRemoveCard(CardInitiativeId card, bool isStillOnBoard = false)
    {
        int index = _cardUidInOrder.IndexOf(card);
        _cardUidInOrder.Remove(card);
        _initiativeManager.OnCardRemoved(card, index, isStillOnBoard);
    }

    public virtual bool IsCardInInitiativeList(Card card)
    {
        return _cardUidInOrder.Any(cardInitId => cardInitId.cohortUid == card.CohortUid);
    }

    public virtual int GetCardIndexInList(Card card)
    {
        int index = _cardUidInOrder.FindIndex(cardInitId => cardInitId.cohortUid == card.CohortUid);
        return index != -1 ? index : -1;
    }
    
    public virtual string[] GetCohortUIDInOrder()
    {
        return _cardUidInOrder.Select(card => card.cohortUid).ToArray();
    }
    
    public virtual InitiativeOrder Clone(InitiativeManager initiativeManager)
    {
        InitiativeOrder dest = new InitiativeOrder(initiativeManager);
        dest._cardUidInOrder = new List<CardInitiativeId>();
        foreach (var cardInit in _cardUidInOrder)
        {
            dest._cardUidInOrder.Add(cardInit.Clone());
        }

        return dest;
    }
}
