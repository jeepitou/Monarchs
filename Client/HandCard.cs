using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Monarchs.Board;
using Monarchs.Logic;
using Sirenix.OdinInspector;
using TcgEngine;
using TcgEngine.FX;
using TcgEngine.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Monarchs.Client
{
    /// <summary>
    /// Represents the visual aspect of a card in hand.
    /// Will take the data from Card.cs and display it
    /// </summary>

    public class HandCard : MonoBehaviour
    {
        [Required] public Image cardGlow;
        [Required] public GameObject allVisuals;
        [Required] public GameObject cohortCardCopy;
        [Required] public InfoBoxList infoBoxList;
        [Required] public GameObject cardVisual;
        [Required] public RectTransform hitbox;
        [Required] public PieceBaseLink pieceBaseLink;
        public GameObject deckPositionGameObject;
        
        public float moveRotateSpeed = 4f;
        public float moveMaxRotate = 10f;
        
        public List<GameObject> cohortCardCopies = new ();
        
        [HideInInspector] public Vector2 deckPosition;
        [HideInInspector] public float deckAngle;
        [HideInInspector] public int handIndex;
        
        public static UnityAction<Card> OnChangeDraggedCard;
        public Color playableGlowColor;
        public Color aboutToBePlayedGlowColor;
        public bool puttingCardBackInHand;
        
        protected Card _card;
        protected string _cardUID = "";
        protected HandCardUIManager _cardUI;
        protected RectTransform _cardTransform;
        protected Vector3 _currentRotate;
        protected Vector2 _targetPosition;
        protected Vector3 _targetSize;
        
        private bool _cohortCopiesShown = true;
        private RectTransform _handTransform;
        private Vector3 _startScale;
        private float _currentAlpha;
        private Vector3 _targetRotate;
        private bool _playingSFX;
        private float _targetAlpha;
        private Vector3 _prevPosition;
        private bool _destroyed;
        private float _focusTimer;
        private bool _focus;
        private bool _drag;
        private bool _selected;
        private GameObject _temporaryPiece;
        private bool _drawingCard;
        
        private static Camera _mainCamera;
        private static readonly List<HandCard> CardList = new ();

        void Awake()
        {
            CardList.Add(this);
            _cardUI = GetComponent<HandCardUIManager>();
            _cardTransform = transform.GetComponent<RectTransform>();
            _handTransform = transform.parent.GetComponent<RectTransform>();
            _startScale = transform.localScale;
            cohortCardCopy.SetActive(false);
            if (_mainCamera == null)
                _mainCamera = Camera.main;
        }

        private void Start()
        {
            if (puttingCardBackInHand)
                return;
            allVisuals.SetActive(false);
            GameClient.Get().animationManager.AddToQueue(DrawCard(deckPositionGameObject), gameObject);
        }

        private void OnDestroy()
        {
            CardList.Remove(this);
        }

        private IEnumerator DrawCard(GameObject deck)
        {
            yield return null;
            allVisuals.SetActive(true);
            _drawingCard = true;
            
            _cardTransform.localPosition = deck.transform.localPosition;
            _cardTransform.localRotation = Quaternion.identity;
            _cardTransform.localScale = _targetSize;
            hitbox.gameObject.SetActive(false);
            
            _cardTransform.DOLocalMove(_targetPosition, 0.5f).SetEase(Ease.Flash).onComplete += () =>
            {
                _drawingCard = false;
                hitbox.gameObject.SetActive(true);
            };

            yield return new WaitForSeconds(0.2f);
        }

        protected virtual void Update()
        {
            if (!GameClient.Get().IsReady() || _drawingCard || puttingCardBackInHand)
                return;

            _focusTimer += Time.deltaTime;

            Card card = GetCard();
            Game game = GameClient.GetGameData();
            _targetPosition = deckPosition;
            _targetSize = _startScale;
            _targetAlpha = 1f;

            if (IsFocus())
            {
                SetFocusedProperties();
                SetOnTop();
                _cardUI.cardUI.ShowDescription();
                _cardUI.cardUI.ShowAOEPatterns();
                infoBoxList.Show();
            }
            else
            {
                ResetCohortCopiesPosition();
                _cardUI.cardUI.HideDescription();
                _cardUI.cardUI.HideAOEPatterns();
                infoBoxList.Hide();
            }

            if (IsDrag())
            {
                ResetCohortCopiesPosition();
                SetOnTop();
                BoardSlot currentlyHoveredSlot = BoardInputManager.Instance.GetLastHoveredSlot();
                bool mouseIsOnBoard = currentlyHoveredSlot != null;
                bool canBePlayed = mouseIsOnBoard && game.CanPlayCardOnSlot(card, currentlyHoveredSlot.GetSlot());
                if (mouseIsOnBoard)
                {
                    SetSFX(canBePlayed);
                    
                    if (card.CardData.IsBoardCard())
                    {
                        SetDraggedPieceOnBoardProperty();
                    }
                    else if (card.CardData.IsRequireTarget())
                    {
                        SetCardWithTargetDraggedOnBoardProperties();
                        SetOnTop();
                    }
                    else
                    {
                        if (canBePlayed)
                        {
                            cardGlow.color = aboutToBePlayedGlowColor;
                        }
                        
                        SetDraggedNotOnBoardProperties();
                        
                    }
                }
                else 
                {
                    SetSFX(false);
                    cardGlow.color = playableGlowColor;
                    SetDraggedNotOnBoardProperties();
                }
            }
            else
            {
                SetSFX(false);
                cardGlow.color = playableGlowColor;
            }
            
            if (!IsDrag() && !IsFocus())
            {
                ResetCohortCopiesPosition();
                _targetRotate = new Vector3(0f, 0f, deckAngle);
                _currentRotate = new Vector3(0f, 0f, deckAngle);
                ResetSortingOrder();
            }
            
            bool canPlay = game.CanPlayCardThisTurn(card);
            
            Vector2 offset = canPlay? new Vector3(0f, 30f) : Vector2.zero;
            _cardTransform.anchoredPosition = _targetPosition + offset;
            _cardTransform.localRotation = Quaternion.Euler(_currentRotate);
            _cardTransform.localScale = _targetSize;
            hitbox.localScale = _startScale/_cardTransform.localScale.x;
            hitbox.localRotation =
                Quaternion.Euler(new Vector3(0, 0, deckAngle - _cardTransform.localRotation.eulerAngles.z));
            
            hitbox.anchoredPosition = (deckPosition - _cardTransform.anchoredPosition + offset)/_cardTransform.localScale;
            
            
            cardGlow.enabled = canPlay;
            
            _currentAlpha = Mathf.MoveTowards(_currentAlpha, _targetAlpha, 2f * Time.deltaTime);
            _prevPosition = Vector3.Lerp(_prevPosition, _cardTransform.position, 1f * Time.deltaTime);
            
            if (!_drag && _selected && Input.GetMouseButtonDown(0))
                _selected = false;
        }

        public void SetInitialPosition()
        {
            _cardTransform.anchoredPosition = deckPosition;
            _cardTransform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, deckAngle));
            _cardTransform.localScale = _startScale;
            hitbox.localScale = _startScale/_cardTransform.localScale.x;
            hitbox.localRotation =
                Quaternion.Euler(new Vector3(0, 0, deckAngle - _cardTransform.localRotation.eulerAngles.z));
            
            hitbox.anchoredPosition = (deckPosition - _cardTransform.anchoredPosition)/_cardTransform.localScale;
        }

        private void SetSFX(bool newState)
        {
            if (newState != _playingSFX)
            {
                _playingSFX = newState;
                if (newState)
                    AudioTool.Get().PlaySFX("card_hover", AssetData.Get().card_about_to_be_played_audio, 0.3f, true, true);
                else
                    AudioTool.Get().StopSFX("card_hover");
            }
        }
        
        private void SetCardWithTargetDraggedOnBoardProperties()
        {
            _targetSize = _startScale * 1.2f;
            _targetPosition = new Vector2(deckPosition.x, 25f);
            _targetRotate = new Vector3(0f, 0f, 0f);
            _currentRotate = new Vector3(0f, 0f, 0f);
            cardGlow.color = aboutToBePlayedGlowColor;
        }

        protected void SetFocusedProperties()
        {
            _targetSize = _startScale * 3f;
            _targetPosition = new Vector2(deckPosition.x, 250f);
            _targetRotate = new Vector3(0f, 0f, 0f);
            _currentRotate = new Vector3(0f, 0f, 0f);
            if (!_cohortCopiesShown)
            {
                _cohortCopiesShown = true;
                ResetCohortCopiesPosition();
            }
            
            ShowCohortCopies();
        }

        private void SetDraggedNotOnBoardProperties()
        {
            _targetPosition = GetTargetPosition();
            ShowCard(_targetPosition);

            _targetSize = _startScale * 0.8f;
            Vector3 direction = _cardTransform.position - _prevPosition;
            Vector3 rotationToAdd = new Vector3(direction.y * 90f, -direction.x * 90f, 0f);
            _targetRotate += moveRotateSpeed * Time.deltaTime * rotationToAdd;
            _targetRotate = new Vector3(Mathf.Clamp(_targetRotate.x, -moveMaxRotate, moveMaxRotate), Mathf.Clamp(_targetRotate.y, -moveMaxRotate, moveMaxRotate), 0f);
            _currentRotate = Vector3.Lerp(_currentRotate, _targetRotate, moveRotateSpeed * Time.deltaTime);
            _targetAlpha = 0.8f;
        }

        private void SetDraggedPieceOnBoardProperty()
        {
            _targetPosition = BoardInputManager.Instance.GetMousePositionOnBoard();
            

            if (GetCard().CardData.IsBoardCard())
            {
                ShowTemporaryPiece(_targetPosition, GetCard());
    
                _temporaryPiece.transform.position = BoardInputManager.Instance.GetLastHoveredSlot().transform.position;
            }
        }

        protected void SetOnTop()
        {
            transform.SetSiblingIndex(transform.parent.childCount);
        }

        private void ResetSortingOrder()
        {
            transform.SetSiblingIndex(handIndex);
        }

        private Vector2 GetTargetPosition()
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_handTransform, Input.mousePosition, Camera.main, out Vector2 targetPosition);
            return targetPosition;
        }

        private void ShowCard(Vector3 targetPosition)
        {
            cardVisual.SetActive(true);
                    
            if (_temporaryPiece != null)
            {
                if (_temporaryPiece.activeSelf)
                {
                    _cardTransform.anchoredPosition = targetPosition;
                    _temporaryPiece.SetActive(false);
                }
            }
        }

        private void ShowTemporaryPiece(Vector3 targetPosition, Card card)
        {
            if (card.CardData.differentSpellPerMovement)
            {
                Card childCard =
                    card.GetCardCorrespondingWithPieceType(GameClient.Get().GetCurrentPieceTurn()[0].GetPieceType());
                if (childCard != null)
                {
                    ShowTemporaryPiece(targetPosition, childCard);
                }

                return;
            }
            if (_temporaryPiece == null)
            {
                InstantiateTemporaryPiece(targetPosition, card);
            }
                            
            _temporaryPiece.SetActive(true);
            cardVisual.SetActive(false);
        }

        private void InstantiateTemporaryPiece(Vector3 targetPosition, Card card)
        {
            _temporaryPiece = Instantiate(pieceBaseLink.GetPieceBase(card.GetPieceType()).piecePrefab, targetPosition, Quaternion.identity);
            PieceOnBoard pieceOnBoard = _temporaryPiece.GetComponent<PieceOnBoard>();
            BoardCard boardCard = _temporaryPiece.GetComponent<BoardCard>();
            
            pieceOnBoard.SetPiece(card);
            boardCard.SetCard(card);
            pieceOnBoard.ApplyTransparentMaterial();
            
            Destroy(boardCard);
            Destroy(_temporaryPiece.GetComponent<BoardCardFX>());
        }

        private void PutCardBackInHand()
        {
            _cardTransform.anchoredPosition = deckPosition;
            _cardTransform.localRotation = Quaternion.Euler(_currentRotate);
            _cardTransform.localScale = _startScale;
            ResetSortingOrder();
        }

        public void HideAdditionnalInfo()
        {
            ResetCohortCopiesPosition();
            _cardUI.cardUI.HideDescription();
            _cardUI.cardUI.HideAOEPatterns();
            infoBoxList.Hide();
        }
        
        public virtual void SetCard(Card card)
        {
            _cardUID = card.uid;
            
            _cardUI.SetCard(card);
            infoBoxList.SetCard(card);
            
            if (card.cohortSize > 1)
            {
                cohortCardCopies.Add(cohortCardCopy);
                cohortCardCopy.SetActive(true);
                cohortCardCopy.GetComponent<UnitCardUI>().SetCard(card);
                
                for (int i = 2; i < card.cohortSize; i++)
                {
                    GameObject copy = Instantiate(cohortCardCopy, cohortCardCopy.transform.position, Quaternion.identity, cohortCardCopy.transform.parent);
                    copy.transform.SetSiblingIndex(0);
                    
                    cohortCardCopies.Add(copy);
                }
            }
            else
            {
                Destroy(cohortCardCopy);
            }
        }
        
        private void ResetCohortCopiesPosition()
        {
            if (!_cohortCopiesShown)
                return;
            
            
            Debug.Log("Resetting cohort copies position");
            foreach (var cohortCopy in cohortCardCopies)
            {
                cohortCopy.SetActive(false);
                cohortCopy.transform.position = transform.position;
                cohortCopy.GetComponent<RectTransform>().localRotation = Quaternion.identity;
                DOTween.Kill(cohortCopy.transform);
                DOTween.Kill(cohortCopy.GetComponent<RectTransform>());
            }
            _cohortCopiesShown = false;
        }
        
        private void ShowCohortCopies()
        {
            int cohortSize = GetCard().cohortSize;
            if (cohortSize <= 1)
                return;
            
            _cohortCopiesShown = true;
            float currentMoveX = 0.5f;
            float currentRotate = 5f;
            foreach (var cohortCopy in cohortCardCopies)
            {
                cohortCopy.SetActive(true);
                cohortCopy.GetComponent<UnitCardUI>().SetCard(GetCard());
                cohortCopy.transform.DOMoveX(transform.position.x - currentMoveX, 0.5f);
                cohortCopy.GetComponent<RectTransform>().DOLocalRotate(Vector3.forward * currentRotate, 0.5f);
                
                currentMoveX += 0.5f;
                currentRotate += 5f;
            }
        }

        public void Kill()
        {
            if (!_destroyed)
            {
                _destroyed = true;
                Destroy(gameObject);
            }
        }

        private bool IsFocus()
        {
            if (GameTool.IsMobile())
                return _selected && !_drag;
            return _focus && !_drag && _focusTimer > 0f;
        }

        private bool IsDrag()
        {
            return _drag;
        }

        public virtual Card GetCard()
        {
            Game gdata = GameClient.GetGameData();
            return gdata.GetCard(_cardUID);
        }

        public CardData GetCardData()
        {
            Card card = GetCard();
            if (card != null)
                return CardData.Get(card.cardID);
            return null;
        }

        public string GetCardUID()
        {
            return _cardUID;
        }

        public void OnMouseEnterCard()
        {
            if (GameUI.IsUIOpened())
                return;

            _focus = true;
            GameClient.Get().HoverHandCard(GetCard());
        }

        public void OnMouseExitCard()
        {
            _focus = false;
            _focusTimer = 0.2f;

            if (GetFocus() == null)
            {
                GameClient.Get().HoverHandCard(null);
            }
        }

        public void OnMouseDownCard()
        {
            if (!GameUI.IsUIOpened())
            {
                OnChangeDraggedCard?.Invoke(GetCard());
                PlayerControls.Get().UnselectAll();
                _drag = true;
                _selected = true;
                AudioTool.Get().PlaySFX("hand_card", AssetData.Get().hand_card_click_audio);
            }
        }

        public void OnMouseUpCard()
        {
            SetSFX(false);
            if (_drag)
            {
                BoardSlot boardTargetSlot = BoardInputManager.Instance.GetLastHoveredSlot();
                Slot targetSlot = boardTargetSlot != null ? BoardInputManager.Instance.GetLastHoveredSlot().GetSlot(): Slot.None;
                if (CanPlayCard(targetSlot))
                {
                    PlayCard(targetSlot);
                    HandCardArea.Get().SortCards();
                }
                else 
                {
                    ShowCard(GetTargetPosition());
                    PutCardBackInHand();
                }
            }

            _drag = false;
            OnChangeDraggedCard?.Invoke(null);
            if (GetCard().CardData.differentSpellPerMovement)
            {
                _temporaryPiece = null;
            }
        }

        private bool CanPlayCard(Slot targetSlot)
        {
            if (GameClient.GetGameData().CanPlayCardOnSlot(GetCard(), targetSlot))
            {
                return true;
            }
            
            if (!GameClient.Get().IsYourTurn())
            {
                WarningText.ShowNotYourTurn();
                return false;
            }
            
            int playerID = GameClient.Get().GetPlayerID();
            Game gdata = GameClient.GetGameData();
            Player player = gdata.GetPlayer(playerID);
            Card cardToPlay = GetCard();
            
            List<Card> possibleCastersForCard = GameClient.GetGameData().GetPossibleCastersForCard(GetCard());
            
            if (possibleCastersForCard.FindAll(c => c.playedCardThisRound).Count >= possibleCastersForCard.Count)
            {
                WarningText.ShowAlreadyPlayedCard();
                return false;
            }
            
            //
            // PieceType currentPieceType = GameClient.Get().GetCurrentPieceTurn()[0].GetPieceType();
            // if (!cardToPlay.CardData.possibleCasters.HasFlag(currentPieceType))
            // {
            //     WarningText.ShowText(currentPieceType + " cannot play this card");
            //     return false;
            // }

            bool isBoundSpell =
                possibleCastersForCard.Find(c => c.CardData.boundSpells.Contains(cardToPlay.CardData)) != null;
            bool monarchCanCast =
                possibleCastersForCard.Find(c => c.GetPieceType() == PieceType.Monarch) != null;
            if (!player.CanPayMana(cardToPlay, (monarchCanCast || isBoundSpell)))
            {
                WarningText.ShowNoMana();
                return false;
            }
            
            if (!gdata.CanPlayCardOnSlot(cardToPlay, targetSlot, true))
            {
                WarningText.ShowInvalidTarget();
                
                return false;
            }
            
            Card targetCard = gdata.GetSlotCard(targetSlot);
            if (cardToPlay.CardData.IsRequireTarget() && targetCard != null && targetCard.HasStatus(StatusType.SpellImmunity))
            {
                WarningText.ShowSpellImmune();
                return false;
            }
            
            return true;
        }

        private void PlayCard(Slot slot)
        {
            GameClient.Get().PlayCard(GetCard(), slot);
            HandCardArea.Get().DelayRefresh(GetCard());
            if (_temporaryPiece != null)
            {
                Destroy(_temporaryPiece);
            }
            
            Destroy(gameObject);
            if (GameTool.IsMobile())
            {
                BoardElement.UnfocusAll();
            }
        }

        public static HandCard GetDrag()
        {
            foreach (HandCard card in CardList)
            {
                if (card.IsDrag())
                    return card;
            }
            return null;
        }

        public static HandCard GetFocus()
        {
            foreach (HandCard card in CardList)
            {
                if (card.IsFocus())
                    return card;
            }
            return null;
        }

        public static HandCard Get(string uid)
        {
            foreach (HandCard card in CardList)
            {
                if (card && card.GetCardUID() == uid)
                    return card;
            }
            return null;
        }

        public static void UnselectAll()
        {
            foreach (HandCard card in CardList)
            {
                if (card._drag)
                {
                    card.ShowCard(card.GetTargetPosition());
                    card.PutCardBackInHand();
                }
                card._drag = false;
                card._selected = false;
            }
                
        }

        public static List<HandCard> GetAll()
        {
            return CardList;
        }
    }
}
