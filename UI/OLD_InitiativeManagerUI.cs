// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using DG.Tweening;
// using Monarchs.Logic;
// using TcgEngine;
// using TcgEngine.Client;
// using TcgEngine.UI;
// using UnityEngine;
// using UnityEngine.UI;
//
// public class InitiativeManagerUI : MonoBehaviour
// {
//     public float nextTurnAnimationDuration;
//     public GameObject initiativePieceParent;
//     public GameObject initiativePiecePrefab;
//     public GameObject initiativeCurrentTurnPrefab;
//     public GameObject roundMarkerPrefab;
//     private List<GameObject> _roundMarker = new List<GameObject>();
//     private int _currentTurnIndex;
//     private InitiativeOrderDisplayConverter _initiativeOrderDisplayConverter;
//     private List<CardInitiativeId> _initiativeListId;
//     private List<PieceInitiativeUI> _initiativeUIList = new List<PieceInitiativeUI>();
//     private GameObject _currentTurnGameObject;
//
//     public bool isDoingNextTurnAnimation = false;
//     public bool isDoingDeathAnimation = false;
//     void Start()
//     {
//         GameClient.Get().onGameStart += UpdateConverterAndUI;
//         //GameClient.Get().onNewTurn += DoNextTurnAnimations;
//         GameClient.Get().onRefreshAll += CheckForUIDifferences;
//         //GameClient.Get().onCardPlayed += CheckForUIDifferences;
//         //GameClient.Get().onCardSummoned += CheckForUIDifferences;
//         //GameClient.Get().onCardDiscarded += RemoveDiscardedCard;
//         
//         _initiativeOrderDisplayConverter = new InitiativeOrderDisplayConverter();
//     }
//
//     void DoNextTurnAnimations(Card card)
//     {
//         isDoingNextTurnAnimation = true;
//         
//         RectTransform rectTransform = initiativePieceParent.GetComponent<RectTransform>();
//         Vector3 initialParentPosition = rectTransform.anchoredPosition;
//         initiativePieceParent.GetComponent<LayoutElement>().ignoreLayout = true;
//         initiativePieceParent.GetComponent<RectTransform>().anchoredPosition = initialParentPosition;
//
//         Vector3 initialCurrentTurnPosition = _currentTurnGameObject.GetComponent<RectTransform>().anchoredPosition;
//         _currentTurnGameObject.GetComponent<LayoutElement>().ignoreLayout = true;
//         _currentTurnGameObject.GetComponent<RectTransform>().anchoredPosition = initialCurrentTurnPosition;
//
//         float initialPosition = initiativePieceParent.transform.localPosition.y;
//
//         float prefabHeight = initiativePiecePrefab.GetComponent<RectTransform>().rect.height * initiativePiecePrefab.transform.localScale.y;
//         
//         if (_currentTurnIndex == _initiativeUIList.Count - 1)
//         {
//             prefabHeight += roundMarkerPrefab.GetComponent <RectTransform>().rect.height * roundMarkerPrefab.transform.localScale.y;
//         }
//
//         UpdateInitiativeOrderConverter();
//         
//         string uidOfPieceThatJustEndedItsTurn = _initiativeUIList[_initiativeUIList.Count-1]._card.uid;
//         SetCurrentTurnNextCard();
//
//         initiativePieceParent.transform.DOLocalMoveY(initialPosition - prefabHeight,
//             nextTurnAnimationDuration, false).OnComplete(() =>
//         {
//             initiativePieceParent.GetComponent<RectTransform>().anchoredPosition = initialParentPosition;
//
//             RemovePieceDoubleForWhichItsHisTurn();
//
//             RemovePlayedCardMarker();
//             AddNewCardToInitiativeUI(uidOfPieceThatJustEndedItsTurn, true, false);
//             CheckForUIDifferences(true);
//             UpdatePieceIdList();
//             isDoingNextTurnAnimation = false;
//             
//         });
//         
//         _currentTurnGameObject.gameObject.GetComponent<Animator>().SetTrigger("StartNewTurn");
//     }
//
//     void RemoveDiscardedCard(Card card)
//     {
//         PieceInitiativeUI killedInitiativeUI = null;
//         foreach (var initiativeUI in _initiativeUIList)
//         {
//             if (initiativeUI._card.CohortUid == card.CohortUid)
//             {
//                 killedInitiativeUI = initiativeUI;
//                 break;
//             }
//         }
//
//         int remainingCohortSize = GameClient.GetGameData().GetBoardCardsOfCohort(card.CohortUid).Count - 1;
//         if (remainingCohortSize < 1)
//         {
//             if (_initiativeUIList[_initiativeUIList.Count - 1] != killedInitiativeUI)
//             {
//                 if (killedInitiativeUI != null)
//                 {
//                     killedInitiativeUI.StartDisappearAnimation(this);
//                     isDoingDeathAnimation = true;
//                 }
//             }
//         }
//         else
//         {
//             killedInitiativeUI.UpdateCohortIcon(remainingCohortSize);
//         }
//     }
//
//     public void RemoveUIElement(PieceInitiativeUI ui)
//     {
//         _initiativeUIList.Remove(ui);
//         isDoingDeathAnimation = false;
//     }
//
//     private void RemovePieceDoubleForWhichItsHisTurn()
//     {
//         var toRemove = _initiativeUIList[_initiativeUIList.Count-2];
//         toRemove.transform.SetParent(null);
//         _initiativeUIList.Remove(toRemove);
//         Destroy(toRemove.gameObject);
//     }
//
//     private void SetCurrentTurnNextCard()
//     {
//         var cardList = _initiativeOrderDisplayConverter.GetCardInitiativeIdInDisplayOrder();
//         _initiativeUIList[_initiativeUIList.Count-1].SetCard(cardList[cardList.Count-1].cohortUid);
//     }
//
//     void UpdateInitiativeOrderConverter()
//     {
//         var initiativeOrderManager = GameClient.GetGameData().initiativeManager;
//         _initiativeOrderDisplayConverter.UpdateInitiativeOrder(initiativeOrderManager);
//         _currentTurnIndex = initiativeOrderManager.GetCurrentTurnIndex();
//     }
//     
//     void UpdateConverterAndUI()
//     {
//         UpdateInitiativeOrderConverter();
//         ForceRefreshUI();
//     }
//
//     void UpdatePieceIdList()
//     {
//         _initiativeListId = _initiativeOrderDisplayConverter.GetCardInitiativeIdInDisplayOrder();
//     }
//
//     void CheckForUIDifferences()
//     {
//         //CheckForUIDifferences(false);
//         CheckForUIDifferences(true);
//     }
//     
//     void CheckForUIDifferences(Card card, Slot slot)
//     {
//         CheckForUIDifferences(true);
//     }
//     
//     void CheckForUIDifferences(Slot slot)
//     {
//         CheckForUIDifferences(true);
//     }
//
//     void CheckForUIDifferences(bool forceRefresh=false)
//     {
//         if (!GameClient.Get().IsReady() || _initiativeListId == null)
//         {
//             return;
//         }
//
//         if (GameClient.Get().GetCurrentPieceTurn().Count == 0)
//         {
//             return;
//         }
//
//         if (!forceRefresh)
//         {
//             if (isDoingNextTurnAnimation || isDoingDeathAnimation) 
//             {
//                 return; // We don't want to call refresh when new round is starting.
//             }
//
//             if (_initiativeUIList[_initiativeUIList.Count - 1]._card.CohortUid !=
//                 GameClient.Get().GetCurrentPieceTurn()[0].CohortUid )
//             {
//                 return;
//             }
//         }
//
//         RemovePlayedCardMarker();
//
//         UpdateInitiativeOrderConverter();
//         List<CardInitiativeId> initiativeList =
//             _initiativeOrderDisplayConverter.GetCardInitiativeIdInDisplayOrder();
//
//         if (_initiativeUIList.Count < initiativeList.Count) // A card was added
//         {
//             var differences = initiativeList.
//                 Where(x => !_initiativeUIList.Any(y => y._card.CohortUid == x.cohortUid));
//             foreach (var difference in differences)
//             {
//                 AddNewCardToInitiativeUI(difference.cohortUid, initiativeList.IndexOf(difference), true);
//             }
//         }
//
//         if (_initiativeUIList.Count > initiativeList.Count) // A card was removed
//         {
//             List<PieceInitiativeUI> toRemove = new List<PieceInitiativeUI>(_initiativeUIList);
//
//             foreach (var pieceInitUI in _initiativeUIList)
//             {
//                 foreach (var cardInitId in initiativeList)
//                 {
//                     if (cardInitId.cohortUid == pieceInitUI._card.CohortUid)
//                     {
//                         toRemove.Remove(pieceInitUI);
//                         break;
//                     }
//                 }
//             }
//
//             foreach (var remove in toRemove)
//             {
//                 RemoveCardFromInitiativeUI(remove, false);
//             }
//         }
//
//         if (_initiativeUIList.Count > initiativeList.Count) // If still too many cards, it means there is a double
//         {
//             List<PieceInitiativeUI> distinct = new List<PieceInitiativeUI>();
//             List<PieceInitiativeUI> doubles = new List<PieceInitiativeUI>();
//             foreach (var x in _initiativeUIList)
//             {
//                 foreach (var value in distinct)
//                 {
//                     if (value._card.CohortUid == x._card.CohortUid)
//                     {
//                         doubles.Add(value);
//                     }
//                 }
//                 distinct.Add(x);
//             }
//         
//             foreach (var remove in doubles)
//             {
//                 RemoveCardFromInitiativeUI(remove, false);
//             }
//         }
//         
//
//         _initiativeListId = initiativeList;
//         RefreshAllUI();
//         AddPlayedCardMarker();
//     }
//
//     void RefreshAllUI()
//     {
//         if (_initiativeUIList.Count != _initiativeListId.Count)
//         {
//             return;
//         }
//         
//         for (int i=0; i<_initiativeUIList.Count; i++) // Validate all indexes
//         {
//             if (_initiativeUIList[i]._card.uid != _initiativeListId[i].cohortUid)
//             {
//                 _initiativeUIList[i].SetCard(_initiativeListId[i].cohortUid);
//                 _initiativeUIList[i].UpdateUI(false);
//             }
//         }
//     }
//
//     void ForceRefreshUI()
//     {
//         _initiativeListId = _initiativeOrderDisplayConverter.GetCardInitiativeIdInDisplayOrder();
//         AdjustTextQuantity();
//         
//         for (int i = 0; i < _initiativeListId.Count; i++)
//         {
//             _initiativeUIList[i].SetCard(_initiativeOrderDisplayConverter.GetCardInitiativeIdInDisplayOrder()[i].cohortUid);
//             _initiativeUIList[i].UpdateUI(false);
//         }
//
//         AddPlayedCardMarker();
//     }
//
//     void AdjustTextQuantity()
//     {
//         if (_initiativeUIList.Count == 0)
//         {
//             _currentTurnGameObject = Instantiate(initiativeCurrentTurnPrefab, transform);
//             _initiativeUIList.Add(_currentTurnGameObject.GetComponent<PieceInitiativeUI>());
//         }
//
//         int countDifference = 0;
//         if (_roundMarker == null)
//         {
//             countDifference = _initiativeListId.Count - _initiativeUIList.Count;
//         }
//         else
//         {
//             countDifference = _initiativeListId.Count - _initiativeUIList.Count;
//         }
//         
//
//         if (countDifference > 0)
//         {
//             for (int i = 0; i < countDifference; i++)
//             {
//                 _initiativeUIList.Insert(0,Instantiate(initiativePiecePrefab, initiativePieceParent.transform).GetComponent<PieceInitiativeUI>());
//                 _initiativeUIList[0].transform.SetAsFirstSibling();
//                 _initiativeUIList[0].StartAppearAnimation();
//             }
//         }
//
//         if (countDifference < 0)
//         {
//             for (int i = 0; i > countDifference; i--)
//             {
//                 Destroy(_initiativeUIList[0].gameObject);
//                 _initiativeUIList.RemoveAt(0);
//             }
//         }
//     }
//
//     void AddNewCardToInitiativeUI(string cohortUID, int index, bool doAppearAnimation, bool isNewCard=true)
//     {
//         _initiativeUIList.Insert(index,Instantiate(initiativePiecePrefab, initiativePieceParent.transform).GetComponent<PieceInitiativeUI>());
//         _initiativeUIList[index].transform.SetSiblingIndex(index);
//         _initiativeUIList[index].SetCard(cohortUID);
//         _initiativeUIList[index].UpdateUI(isNewCard);
//         
//         if (doAppearAnimation)
//         {
//             _initiativeUIList[index].StartAppearAnimation();
//         }
//     }
//
//     void AddNewCardToInitiativeUI(string cohortUID, bool doAppearAnimation, bool isNewCard=true)
//     {
//         var initiativeListId = _initiativeOrderDisplayConverter.GetCardInitiativeIdInDisplayOrder();
//         foreach (var initiativeId in initiativeListId)
//         {
//             if (initiativeId.cohortUid == cohortUID)
//             {
//                 AddNewCardToInitiativeUI(cohortUID, initiativeListId.IndexOf(initiativeId), doAppearAnimation, isNewCard);
//             }
//         }
//     }
//     
//     void RemoveCardFromInitiativeUI(PieceInitiativeUI pieceInitiativeUI, bool doDisappearAnimation)
//     {
//         if (pieceInitiativeUI == _initiativeUIList[_initiativeUIList.Count - 1])
//         {
//             pieceInitiativeUI = _initiativeUIList[_initiativeUIList.Count - 2];
//         }
//         
//         if (doDisappearAnimation)
//         {
//             isDoingDeathAnimation = true;
//             pieceInitiativeUI.StartDisappearAnimation(this);
//         }
//         else
//         {
//             _initiativeUIList.Remove(pieceInitiativeUI);
//             Destroy(pieceInitiativeUI.gameObject);
//         }
//     }
//
//     void RemovePlayedCardMarker()
//     {
//         foreach (var roundMarker in _roundMarker)
//         {
//             roundMarker.transform.SetParent(null);
//             Destroy(roundMarker);
//             
//         }
//     }
//     
//     void AddPlayedCardMarker()
//     {
//         _roundMarker = new List<GameObject>();
//
//         
//         if (_initiativeOrderDisplayConverter.musterDelimitationIndexes.Count > 0)
//         {
//             for (int i = 0; i < _initiativeOrderDisplayConverter.musterDelimitationIndexes.Count; i++)
//             {
//                 _roundMarker.Add(Instantiate(roundMarkerPrefab, initiativePieceParent.transform));
//                 
//                 _roundMarker[i].transform.SetSiblingIndex(_initiativeOrderDisplayConverter.musterDelimitationIndexes[i]+i);
//                 _roundMarker[i].SetActive(true);
//             }
//         }
//
//         if (_initiativeOrderDisplayConverter.playNextTurnDelimitationIndex != 0)
//         {
//             var musterIndex = _initiativeOrderDisplayConverter.musterDelimitationIndexes;
//             if (musterIndex.Count == 0 || _initiativeOrderDisplayConverter.playNextTurnDelimitationIndex !=
//                 musterIndex[musterIndex.Count - 1])
//             {
//                 _roundMarker.Add(Instantiate(roundMarkerPrefab, initiativePieceParent.transform));
//                 _roundMarker[_roundMarker.Count-1].SetActive(true);
//                 _roundMarker[_roundMarker.Count - 1].name = "TEST";
//                 _roundMarker[_roundMarker.Count-1].transform.SetSiblingIndex(_initiativeOrderDisplayConverter.playNextTurnDelimitationIndex+musterIndex.Count);
//             }
//         }
//         
//         
//     }
// }