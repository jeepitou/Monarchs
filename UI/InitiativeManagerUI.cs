using System;
using System.Collections.Generic;
using System.Linq;
using Monarchs;
using Monarchs.Ability;
using Monarchs.Client;
using Monarchs.Initiative;
using Monarchs.Logic;
using TcgEngine;
using TcgEngine.Client;
using UnityEngine;
using UnityEngine.UI;

public class InitiativeManagerUI : MonoBehaviour
{
    public float nextTurnAnimationDuration;
    public GameObject initiativePieceParent;
    public GameObject initiativePiecePrefab;
    private List<PieceInitiativeUI> _initiativeUIList = new List<PieceInitiativeUI>();
    private List<PieceInitiativeUI> _inactiveInitiativeUIList = new List<PieceInitiativeUI>();
    private PieceInitiativeUI _hoveredInitiativeUI;
    private PieceInitiativeUI _aboutToBePlayedTemporaryInitiativeUI;
    private GameObject _currentTurnGameObject;
    private Card _draggedCard;
    
    void Start()
    {
        if (GameplayData.Get().InitiativeType == InitiativeManager.InitiativeType.AllPiecesEveryTurn)
        {
            Destroy(gameObject);
            return;
        }
        
        GameClient.Get().onGameStart += CheckForUIDifferences;
        GameClient.Get().onRefreshAll += CheckForUIDifferences;
        GameClient.Get().onNewRound += SetAllActive;
        BoardInputManager.Instance.OnHover += OnSlotHover;
        HandCard.OnChangeDraggedCard += OnDraggedHandCardChanged;
        GameObject aboutToBePlayedObject = Instantiate(initiativePiecePrefab, initiativePieceParent.transform);
        _aboutToBePlayedTemporaryInitiativeUI = aboutToBePlayedObject.GetComponent<PieceInitiativeUI>();
        _aboutToBePlayedTemporaryInitiativeUI.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        GameClient.Get().onGameStart -= CheckForUIDifferences;
        GameClient.Get().onRefreshAll -= CheckForUIDifferences;
        GameClient.Get().onNewRound -= SetAllActive;
        BoardInputManager.Instance.OnHover -= OnSlotHover;
        HandCard.OnChangeDraggedCard -= OnDraggedHandCardChanged;
        
        foreach (var initiativeUI in _initiativeUIList)
        {
            Destroy(initiativeUI.gameObject);
        }
        
        _initiativeUIList.Clear();
        _inactiveInitiativeUIList.Clear();
    }

    private void OnDraggedHandCardChanged(Card draggedCard)
    {
        _draggedCard = draggedCard;
        if (_draggedCard == null)
        {
            _aboutToBePlayedTemporaryInitiativeUI._card = null;
            _aboutToBePlayedTemporaryInitiativeUI.gameObject.SetActive(false);
            return;
        }
        if (_draggedCard.CardData.cardType != CardType.Character)
        {
            _draggedCard = null;
            _aboutToBePlayedTemporaryInitiativeUI.gameObject.SetActive(false);
        }
    }

    private void OnSlotHover(BoardSlot slot, Card card)
    {
        ManageTemporaryInitiativeUI(slot);
        
        if (card == null)
        {
            _hoveredInitiativeUI?.OnPointerExitBoardCard();
            _hoveredInitiativeUI = null;
            return;
        }

        if (card.CohortUid != _hoveredInitiativeUI?._card.CohortUid)
        {
            _hoveredInitiativeUI?.OnPointerExitBoardCard();
            _hoveredInitiativeUI = _initiativeUIList.FirstOrDefault(x => x._card.CohortUid == card.CohortUid);
            if (_hoveredInitiativeUI != null)
            {
                _hoveredInitiativeUI.OnPointerEnterBoardCard();
            }
        }
    }
    
    private void ManageTemporaryInitiativeUI(BoardSlot hoveredSlot)
    {
        if (_draggedCard==null || hoveredSlot == null)
        {
            _aboutToBePlayedTemporaryInitiativeUI.gameObject.SetActive(false);
            return;
        }
        
        //Changed dragged card
        if (_draggedCard.CohortUid != _aboutToBePlayedTemporaryInitiativeUI?._card?.CohortUid)
        {
            int index = GameClient.GetGameData().initiativeManager.GetAboutToBePlayedCardPosition(_draggedCard, GameClient.GetGameData());
            _aboutToBePlayedTemporaryInitiativeUI.transform.SetSiblingIndex(index);
            _aboutToBePlayedTemporaryInitiativeUI.SetCard(_draggedCard);
            _aboutToBePlayedTemporaryInitiativeUI.UpdateUI(true);
            if (index >= GameClient.GetGameData().initiativeManager.GetCurrentTurnIndex() && !_draggedCard.HasAbility(id:"play_ambush"))
            {
                _aboutToBePlayedTemporaryInitiativeUI.SetAsTemporary(0.5f);
            }
            else
            {
                _aboutToBePlayedTemporaryInitiativeUI.SetAsTemporary(0.9f);
            }
        }

        if (_aboutToBePlayedTemporaryInitiativeUI._card != null && _aboutToBePlayedTemporaryInitiativeUI.gameObject.activeSelf == false)
        {
            _aboutToBePlayedTemporaryInitiativeUI.gameObject.SetActive(true);
        }
    }

    private void SetAllActive(Card card)
    {
        foreach (var initiative in _inactiveInitiativeUIList)
        {
            if (initiative != null)
            {
                initiative.SetActive(true);
            }
        }
        _inactiveInitiativeUIList.Clear();
        RefreshLayout();
    }

    private void Update()
    {
        RefreshLayout();
    }

    void CheckForUIDifferences()
    {
        if (!GameClient.Get().IsReady())
        {
            return;
        }

        if (GameClient.Get().GetCurrentPieceTurn().Count == 0)
        {
            return;
        }

        var initiativeList = GameClient.GetGameData().initiativeManager.GetInitiativeOrder();
        
        
        List<PieceInitiativeUI> toRemove = new List<PieceInitiativeUI>(_initiativeUIList);
        foreach (var pieceInitUI in _initiativeUIList)
        {
            foreach (var cardInitId in initiativeList)
            {
                if (cardInitId.cohortUid == pieceInitUI._card.CohortUid)
                {
                    toRemove.Remove(pieceInitUI);
                    break;
                }
            }
        }

        foreach (var remove in toRemove)
        {
            RemoveCardFromInitiativeUI(remove);
        }
        
        
        if (_initiativeUIList.Count < initiativeList.Count) // A card was added
        {
            var differences = initiativeList.
                Where(x => !_initiativeUIList.Any(y => y._card.CohortUid == x.cohortUid));
            foreach (var difference in differences)
            {
                AddNewCardToInitiativeUI(difference.cohortUid, initiativeList.IndexOf(difference), true, difference.active);
            }
        }

        foreach (var initiative in _initiativeUIList)
        {
            initiative.SetCurrentTurn(GameClient.Get().GetCurrentPieceTurn()[0].CohortUid);
        }

        if (initiativeList.Count > 10)
        {
            int currentTurnIndex = GameClient.GetGameData().initiativeManager.GetCurrentTurnIndex();
            if (_initiativeUIList.Count - currentTurnIndex <= 10)
            {
                for (int i=0; i < _initiativeUIList.Count-10; i++)
                {
                    _initiativeUIList[i].gameObject.SetActive(false);
                }
                for (int i=10; i < _initiativeUIList.Count; i++)
                {
                    _initiativeUIList[i].gameObject.SetActive(true);
                }
            }
            else
            {
                for (int i=currentTurnIndex; i < _initiativeUIList.Count-10; i++)
                {
                    _initiativeUIList[i].gameObject.SetActive(true);
                }
                for (int i = 0; i < currentTurnIndex; i++)
                {
                    _initiativeUIList[i].gameObject.SetActive(false);
                }
                for (int i = _initiativeUIList.Count-1; i >= currentTurnIndex + 10; i--)
                {
                    _initiativeUIList[i].gameObject.SetActive(false);
                }
                for (int i=9; i < currentTurnIndex+10; i++)
                {
                    _initiativeUIList[i].gameObject.SetActive(true);
                }
            }
            
        }
        RefreshLayout();
    }

    private void RemoveCardFromInitiativeUI(PieceInitiativeUI pieceToRemove)
    {
        _initiativeUIList.Remove(pieceToRemove);
        Destroy(pieceToRemove.gameObject);
        RefreshLayout();
    }

    private void AddNewCardToInitiativeUI(string differenceCohortUid, int indexOf, bool isNewCad, bool isActive)
    {
        var newCard = Instantiate(initiativePiecePrefab, initiativePieceParent.transform);
        var pieceInitiativeUI = newCard.GetComponent<PieceInitiativeUI>();
        pieceInitiativeUI.SetCard(differenceCohortUid);
        pieceInitiativeUI.UpdateUI(isNewCad);
        _initiativeUIList.Insert(indexOf, pieceInitiativeUI);
        newCard.transform.SetSiblingIndex(indexOf);
        pieceInitiativeUI.SetActive(isActive);
        if (!isActive)
        {
            _inactiveInitiativeUIList.Add(pieceInitiativeUI);
        }
        RefreshLayout();
    }
    
    private void RefreshLayout()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(initiativePieceParent.GetComponent<RectTransform>());
    }
}