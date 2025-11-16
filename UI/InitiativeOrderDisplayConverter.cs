using System;
using System.Collections.Generic;
using Monarchs.Initiative;
using TcgEngine.Client;
using UnityEngine;

namespace TcgEngine.UI
{
    public class InitiativeOrderDisplayConverter
    {
        private List<CardInitiativeId> _cardUIDInDisplayOrder;
        public List<int> musterDelimitationIndexes = new List<int>();
        public int playNextTurnDelimitationIndex;

        public List<CardInitiativeId> GetCardInitiativeIdInDisplayOrder()
        {
            return _cardUIDInDisplayOrder;
        }

        public void UpdateInitiativeOrder(InitiativeManager initiativeManager)
        {
            _cardUIDInDisplayOrder = new List<CardInitiativeId>();
            musterDelimitationIndexes = new List<int>();
            
            if (initiativeManager == null || initiativeManager.GetInitiativeOrder() == null)
            {
                Debug.LogError("Cannot update InitiativeOrderDisplayConverter with a null InitiativeOrderManager");
                return;
            }
            
            var initiativeList = initiativeManager.GetInitiativeOrder();
            int currentTurnIndex = initiativeManager.GetCurrentTurnIndex();
            
            if (initiativeList.Count == 0)
            {
                Debug.LogError("InitiativeOrderManager list is empty.");
                return;
            }

            if (currentTurnIndex < 0 || currentTurnIndex >= initiativeList.Count)
            {
                Debug.LogError("Invalid currentTurnIndex in InitiativeOrderManager");
                return;
            }

            playNextTurnDelimitationIndex = currentTurnIndex;
            musterDelimitationIndexes = new List<int>();

            if (currentTurnIndex > 0) //Add cards that have already played
            {
                for (int i = currentTurnIndex - 1; i >=0; i--)
                { 
                    if (initiativeList[i].remainingMuster > 0)
                    {
                        AddCardAtCorrectPlace(initiativeList[i], false);
                    }
                    else
                    {
                        _cardUIDInDisplayOrder.Add(initiativeList[i]);
                    }
                }
            }

            if (currentTurnIndex != initiativeList.Count - 1) //Add cards that play after the currentTurnCard
            {
                for (int i = initiativeList.Count - 1; i >currentTurnIndex; i--)
                {
                    if (!initiativeList[i].active)
                    {
                        AddCardAtCorrectPlace(initiativeList[i], true);
                    }
                    else
                    {
                        _cardUIDInDisplayOrder.Add(initiativeList[i]);
                    }
                }
            }
            
            _cardUIDInDisplayOrder.Add(initiativeList[currentTurnIndex]);
            musterDelimitationIndexes.Reverse();
        }

        /// <summary>
        /// This is used only to place card with muster time, or card with summoning sickness that have a lower initiative
        /// than the current card turn.
        /// </summary>
        /// <param name="cardInitiativeId"></param>
        private void AddCardAtCorrectPlace(CardInitiativeId cardInitiativeId, bool adjustDelimitations)
        {
            

            int maxIndex = _cardUIDInDisplayOrder.Count - 1 < playNextTurnDelimitationIndex
                ? _cardUIDInDisplayOrder.Count
                : playNextTurnDelimitationIndex;
            
            if (maxIndex == 0)
            {
                _cardUIDInDisplayOrder.Insert(0, cardInitiativeId);
            }
            
            for (int i = 0; i < maxIndex; i++)
            {
                if (cardInitiativeId.remainingMuster > _cardUIDInDisplayOrder[i].remainingMuster)
                {
                    _cardUIDInDisplayOrder.Insert(i, cardInitiativeId);
                    break;
                }

                if (cardInitiativeId.remainingMuster == _cardUIDInDisplayOrder[i].remainingMuster)
                {
                    if (cardInitiativeId.initiative <= _cardUIDInDisplayOrder[i].initiative)
                    {
                        _cardUIDInDisplayOrder.Insert(i, cardInitiativeId);
                        break;
                    }
                }

                if (i == maxIndex - 1)
                {
                    _cardUIDInDisplayOrder.Insert(i+1, cardInitiativeId);
                }
            }

            if (adjustDelimitations)
            {
                playNextTurnDelimitationIndex++;
            }
            
            if (cardInitiativeId.remainingMuster > 0)
            {
                for (int i = 1; i <= cardInitiativeId.remainingMuster; i++)
                {
                    if (musterDelimitationIndexes.Count < i)
                    {
                        musterDelimitationIndexes.Add(1);
                    }
                    else
                    {
                        musterDelimitationIndexes[i-1] = musterDelimitationIndexes[i-1] + 1;
                    }
                }
            }
        }
    }
}