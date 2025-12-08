using System;
using System.Collections.Generic;
using Monarchs.Logic;

namespace Monarchs.Initiative
{
    [Serializable]
    public class InitiativeManager
    {
        protected InitiativeOrder InitiativeOrder {get; set;}
        [field: NonSerialized] protected InitiativeTurnUtil InitiativeTurnUtil {get; set;}
        [field: NonSerialized] protected InitiativeAmbush InitiativeAmbush {get; set;}
        
        public InitiativeManager()
        {
            InitiativeOrder = new InitiativeOrder(this);
            InitiativeTurnUtil = new InitiativeTurnUtil(this);
            InitiativeAmbush = new InitiativeAmbush(this, InitiativeOrder);
        }
        
        public InitiativeManager(InitiativeOrder initiativeOrder, InitiativeTurnUtil initiativeTurnUtil, InitiativeAmbush initiativeAmbush)
        {
            InitiativeOrder = initiativeOrder;
            InitiativeTurnUtil = initiativeTurnUtil;
            InitiativeAmbush = initiativeAmbush;
        }
        
        public bool LastPieceTurnDied => this.InitiativeTurnUtil.LastTurnPieceDied;
        public CardInitiativeId LastDied => this.InitiativeTurnUtil.LastDied;
        public virtual InitiativeOrder GetInitiativeOrderObject() => InitiativeOrder;
        public virtual InitiativeAmbush GetInitiativeAmbushObject() => InitiativeAmbush;
        public virtual InitiativeTurnUtil GetInitiativeTurnUtilObject() => InitiativeTurnUtil;
        
        public virtual void AddAmbushToCard(Card card) => InitiativeAmbush.AddAmbushToCard(card);
        public virtual void AddCard(Card card, bool canPlayThisTurn = false) => InitiativeOrder.AddCard(card, canPlayThisTurn);
        public virtual int GetAboutToBePlayedCardPosition(Card card, Game game) => InitiativeOrder.GetAboutToBePlayedCardPosition(card, game);
        public virtual int GetCardIndexInList(Card card) => InitiativeOrder.GetCardIndexInList(card);
        public virtual List<Card> GetCurrentCardTurn(Game game) => InitiativeTurnUtil.GetCurrentCardTurn(game);
        public virtual int GetCurrentTurnIndex() => InitiativeTurnUtil.GetCurrentTurnIndex();
        public virtual CardInitiativeId GetCurrentTurnInitiativeId() =>
            InitiativeOrder.GetInitiativeList()[InitiativeTurnUtil.GetCurrentTurnIndex()];
        public virtual List<CardInitiativeId> GetInitiativeOrder() => InitiativeOrder.GetInitiativeList();
        public virtual bool IsCardInInitiativeList(Card card) => InitiativeOrder.IsCardInInitiativeList(card);
        public virtual bool IsRoundOver() => InitiativeTurnUtil.IsRoundOver();
        public virtual bool IsTurnOfCard(Card card) => InitiativeTurnUtil.IsTurnOfCard(card);
        public virtual void NextRound(Game game) => InitiativeTurnUtil.NextRound(game);
        public virtual void NextTurn() => InitiativeTurnUtil.NextTurn();
        public virtual void OnCardAdded(CardInitiativeId cardInitiativeId, int index) => 
            InitiativeTurnUtil.OnCardAdded(cardInitiativeId, index);
        public virtual void OnCardRemoved(CardInitiativeId cardInitiativeId, int index, bool isStillOnBoard = false) => 
            InitiativeTurnUtil.OnCardRemoved(cardInitiativeId, index, isStillOnBoard);
        public virtual void ReplaceAmbushingCard(CardInitiativeId cardInitiativeId) => 
            InitiativeAmbush.ReplaceAmbushingCard(cardInitiativeId);
        public virtual void RemoveCard(Card card, Game game, bool forceRemove = false) => 
            InitiativeOrder.RemoveCard(card, game, forceRemove);
        public virtual void ResetCurrentTurnIndex() => InitiativeTurnUtil.ResetCurrentTurnIndex();

        public InitiativeManager Clone()
        {
            InitiativeManager initiativeManager = new InitiativeManager();
            initiativeManager.InitiativeOrder = InitiativeOrder.Clone(initiativeManager);
            initiativeManager.InitiativeTurnUtil = InitiativeTurnUtil.Clone(initiativeManager);
            initiativeManager.InitiativeAmbush = InitiativeAmbush;
            return initiativeManager;
        }
        
        public enum InitiativeType
        {
            Initiative,
            PlayedOrder,
            AllPiecesEveryTurn
        }
    }
}