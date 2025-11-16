using System;
using System.Collections.Generic;
using Monarchs.Logic;

namespace Monarchs.Initiative
{
    [Serializable]
    public class InitiativeTurnUtil
    {
        protected virtual InitiativeManager InitiativeManager { get; set; }
        public virtual int CurrentIndex { get; protected set; }
        public virtual bool RoundOver { get; protected set; } = false;
        public virtual bool LastTurnPieceDied {get; protected set;} = false;
        public virtual CardInitiativeId LastDied {get; protected set;}
        
        public InitiativeTurnUtil(InitiativeManager initiativeManager)
        {
            InitiativeManager = initiativeManager;
            CurrentIndex = 0;
        }

        public virtual void OnCardAdded(CardInitiativeId card, int index)
        {
            if (index <= CurrentIndex && InitiativeManager.GetInitiativeOrder().Count > 1)
            {
                CurrentIndex++;
            }
        }

        public virtual void OnCardRemoved(CardInitiativeId card, int index, bool isStillOnBoard=false)
        {
            LastDied = card;
                
            if (CurrentIndex > index)
            {
                CurrentIndex -= 1;
            }

            if (CurrentIndex == index && !isStillOnBoard)
            {
                LastTurnPieceDied = true;
            }
        }
        
        public virtual List<Card> GetCurrentCardTurn(Game game)
        {
            return game.GetBoardCardsOfCohort(GetCurrentCohortTurn());
        }
        
        public virtual string GetCurrentCohortTurn()
        {
            var cardInitiativeIds = InitiativeManager.GetInitiativeOrder();
            
            if (CurrentIndex >= cardInitiativeIds.Count && LastTurnPieceDied)
            {
                return LastDied.cohortUid;
            }
            
            if (CurrentIndex >= cardInitiativeIds.Count)
            {
                return "";
            }
        
            return cardInitiativeIds[CurrentIndex].cohortUid;
        }

        public virtual int GetCurrentTurnIndex()
        {
            return CurrentIndex;
        }

        public virtual bool IsTurnOfCard(Card card)
        {
            return card?.CohortUid == GetCurrentCohortTurn();
        }
        
        public virtual void ResetCurrentTurnIndex()
        {
            CurrentIndex = 0;
        }

        public virtual void SetCurrentTurnIndex(int index)
        {
            CurrentIndex = index;
        }

        public virtual void SetRoundOver(bool state)
        {
            RoundOver = state;
        }
        
        public virtual void NextTurn()
        {
            var cardUidInOrder = InitiativeManager.GetInitiativeOrder();
            
            if (LastTurnPieceDied)
            {
                LastTurnPieceDied = false;
            }
            else
            {
                if (cardUidInOrder[CurrentIndex].isAmbushing)
                {
                    cardUidInOrder[CurrentIndex].isAmbushing = false;
                    InitiativeManager.ReplaceAmbushingCard(cardUidInOrder[CurrentIndex]);
                    
                }
                else
                {
                    CurrentIndex++;
                }
            }
            
            if (CurrentIndex >= cardUidInOrder.Count)
            {
                RoundOver = true;
                return;
            }
    
            if (!cardUidInOrder[CurrentIndex].active)
            {
                NextTurn();
            }
        }
    
        public virtual void NextRound(Game game)
        {
            RoundOver = false;
            CurrentIndex = 0;
    
            UpdatePiecesInMuster(game); 
        }
        
        public virtual bool IsRoundOver()
        {
            return RoundOver;
        }
        
        private void UpdatePiecesInMuster(Game game)
        {
            foreach (var cardInitiativeId in InitiativeManager.GetInitiativeOrder())
            {
                cardInitiativeId.ReduceMuster(game);
            }
        }

        public virtual InitiativeTurnUtil Clone(InitiativeManager initiativeManager)
        {
            InitiativeTurnUtil initiativeTurnUtil = new InitiativeTurnUtil(initiativeManager);
            initiativeTurnUtil.CurrentIndex = CurrentIndex;
            initiativeTurnUtil.RoundOver = RoundOver;
            initiativeTurnUtil.LastTurnPieceDied = LastTurnPieceDied;
            initiativeTurnUtil.LastDied = LastDied;
            return initiativeTurnUtil;
        }
    }
}