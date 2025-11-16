using System.Collections.Generic;
using Ability.Target;
using Monarchs.Board;
using Monarchs.Logic;
using TcgEngine;
using TcgEngine.FX;
using TcgEngine.UI;
using UnityEngine;

namespace Monarchs.Client
{
    /// <summary>
    /// Visual representation of a Slot.cs
    /// Will highlight when can be interacted with
    /// </summary>

    public class BoardSlot : MonoBehaviour
    {
        private static SlotHighlightManager _slotHighlightManager;
        private static readonly List<BoardSlot> SlotList = new ();
        private readonly SlotHighlightsStatus _slotHighlightsStatus = new ();
        public int x;
        public int y;
        public SpriteRenderer slotHighlight;
        public SpriteRenderer rangeAttackHighlight;
        public Color whiteHighlight;
        public Color greyHighlight;
        public Color redHighlight;
        public Color blueHighlight;
        public Material baseMaterial;
        public Material legalHoverMaterial;
        public Material canAttackMaterial;
        public GameObject rangeAttackIcon;
        public GameObject transparentTemporaryPiece;
        
        private MeshRenderer _meshRenderer;
        private static AbilityTargetExtractor _abilityTargetExtractor;
        private static bool _showBoardHighlightToOpponent = true;
        
        public Slot GetSlot()
        {
            return new Slot(x, y);
        }

        public Vector2S GetCoordinate()
        {
            return new Vector2S(x, y);
        }

        public static bool ShowBoardHighlightToOpponent()
        {
            if (HandCard.GetDrag() != null)
            {
                _showBoardHighlightToOpponent = false;
            }
            else if (!_showBoardHighlightToOpponent)
            {
                if (BoardElement.GetFocus() == null)
                {
                    _showBoardHighlightToOpponent = true;
                }
            }

            return _showBoardHighlightToOpponent;
        }
        
        void Awake()
        {
            SlotList.Add(this);
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        void Start()
        {
            if (x < Slot.xMin || x > Slot.xMax || y < Slot.yMin || y > Slot.yMax)
            {
                Debug.LogError("Board Slot X and Y value must be within the min and max set for those values, check Slot.cs script to change those min/max.");
            }

            BoardInputManager.Instance.OnClick += OnClick;

            _slotHighlightManager ??= new SlotHighlightManager();
        }

        private void UpdateMaterial()
        {
            if (GameUI.IsUIOpened() || GameClient.GetGameData().State == GameState.Mulligan)
            {
                return;
            }
            
            UpdateTemporaryPiece();
            SetCurrentTurnRangeAttackIcon();
            SetHoveredPieceRangeAttackHighlight();

            if (_slotHighlightsStatus.statusDict[SlotHighlightTypes.IsSelected] ||
                _slotHighlightsStatus.statusDict[SlotHighlightTypes.DraggedHandCardLegalMove])
            {
                slotHighlight.enabled = true;
                slotHighlight.color = this.blueHighlight;
                //SetMaterial(legalHoverMaterial);
                return;
            }

            if (_slotHighlightsStatus.statusDict[SlotHighlightTypes.HoveredByPlayer] &&
                IsLegalOffensiveTarget())
            {
                slotHighlight.enabled = true;
                slotHighlight.color = this.redHighlight;
                SetMaterial(canAttackMaterial);
                return;
            }
            
            if (IsLegalOffensiveTarget())
            {
                slotHighlight.enabled = true;
                slotHighlight.color = this.redHighlight;
                SetMaterial(baseMaterial);
                return;
            }
            
            if (_slotHighlightsStatus.statusDict[SlotHighlightTypes.IsFallBackSquare] || 
                _slotHighlightsStatus.statusDict[SlotHighlightTypes.RealDestination])
            {
                slotHighlight.enabled = true;
                slotHighlight.color = this.greyHighlight;
                SetMaterial(baseMaterial);
                return;
            }

            if (_slotHighlightsStatus.statusDict[SlotHighlightTypes.HoveredByPlayer] &&
                IsLegalTarget())
            {
                //SetMaterial(legalHoverMaterial);
                SetMaterial(baseMaterial);
                slotHighlight.enabled = true;
                slotHighlight.color = this.blueHighlight;
                return;
            }
            
            bool hasCardOrTrap = GameClient.GetGameData().GetSlotCard(GetSlot()) != null ||
                                 GameClient.GetGameData().GetSlotTrap(GetSlot()) != null;
            if ((_slotHighlightsStatus.statusDict[SlotHighlightTypes.HoveredByPlayer] && hasCardOrTrap) ||
                _slotHighlightsStatus.statusDict[SlotHighlightTypes.HoveredByOpponent] ||
                _slotHighlightsStatus.statusDict[SlotHighlightTypes.HoveredInInitiativeCard] ||
                IsLegalTarget())
            {
                slotHighlight.enabled = true;
                slotHighlight.color = this.whiteHighlight;
                SetMaterial(baseMaterial);
                return;
            }

            if (_slotHighlightsStatus.statusDict[SlotHighlightTypes.PieceDiedOnSlot])
            {
                slotHighlight.enabled = true;
                slotHighlight.color = Color.magenta;
                SetMaterial(baseMaterial);
                return;
            }
            
            if (_slotHighlightsStatus.statusDict[SlotHighlightTypes.HoveredCardLegalMove])
            {
                slotHighlight.enabled = true;
                slotHighlight.color = this.greyHighlight;
                SetMaterial(baseMaterial);
                return;
            }
            
            slotHighlight.enabled = false;
            SetMaterial(baseMaterial);
        }

        private void UpdateTemporaryPiece()
        {
            Card draggedCard = PlayerControls.Get().GetDragged()?.GetCard();
            bool hoveringLegalMove = _slotHighlightsStatus.statusDict[SlotHighlightTypes.HoveredByPlayer] &&
                                     IsLegalTarget() && draggedCard != null && GameClient.GetGameData().GetSlotCard(GetSlot()) == null;
            
            bool tempPieceShouldBeVisible = _slotHighlightsStatus.statusDict[SlotHighlightTypes.IsFallBackSquare] || 
                                            _slotHighlightsStatus.statusDict[SlotHighlightTypes.RealDestination] || 
                                            hoveringLegalMove;  
            
            
            if (tempPieceShouldBeVisible && transparentTemporaryPiece == null)
            {

                transparentTemporaryPiece = InstantiateTemporaryPiece(transform.position, draggedCard);
                transparentTemporaryPiece.SetActive(true);
            }
            else if (!tempPieceShouldBeVisible && transparentTemporaryPiece != null)
            {
                Destroy(transparentTemporaryPiece);
            }
        }
        
        private GameObject InstantiateTemporaryPiece(Vector3 targetPosition, Card card)
        {
            GameObject _temporaryPiece = Instantiate(PieceBaseLink.Instance.GetPieceBase(card.GetPieceType()).piecePrefab, targetPosition, Quaternion.identity);
            PieceOnBoard pieceOnBoard = _temporaryPiece.GetComponent<PieceOnBoard>();
            BoardCard boardCard = _temporaryPiece.GetComponent<BoardCard>();
            
            pieceOnBoard.SetPiece(card);
            boardCard.SetCard(card);
            pieceOnBoard.ApplyTransparentMaterial();
            
            Destroy(boardCard);
            Destroy(_temporaryPiece.GetComponent<BoardCardFX>());
            _temporaryPiece.transform.position = transform.position;
            return _temporaryPiece;
        }

        private bool IsLegalOffensiveTarget()
        {
            return _slotHighlightsStatus.statusDict[SlotHighlightTypes.IsInSpellAOE] ||
                   _slotHighlightsStatus.statusDict[SlotHighlightTypes.IsInSplashDamageArea] ||
                   _slotHighlightsStatus.statusDict[SlotHighlightTypes.IsLegalMeleeAttack] ||
                   _slotHighlightsStatus.statusDict[SlotHighlightTypes.IsInSelfSpell];
        }

        private bool IsLegalTarget()
        {
            return _slotHighlightsStatus.statusDict[SlotHighlightTypes.IsInSpellAOE] ||
                   _slotHighlightsStatus.statusDict[SlotHighlightTypes.IsInSplashDamageArea] ||
                   _slotHighlightsStatus.statusDict[SlotHighlightTypes.IsLegalMeleeAttack] ||
                   _slotHighlightsStatus.statusDict[SlotHighlightTypes.IsSelectorPotentialChoice] ||
                   _slotHighlightsStatus.statusDict[SlotHighlightTypes.IsDraggedHandCardPotentialChoice] ||
                   _slotHighlightsStatus.statusDict[SlotHighlightTypes.DraggedCardLegalMove];
        }

        private void SetHoveredPieceRangeAttackHighlight()
        {
            rangeAttackHighlight.enabled =
                _slotHighlightsStatus.statusDict[SlotHighlightTypes.IsLegalRangedAttackOfHoveredCard];
        }

        private void SetCurrentTurnRangeAttackIcon()
        {
            if (!GameplayData.Get().DragAndDropRangedUnitWillDoARangedAttack)
            {
                rangeAttackIcon.SetActive(_slotHighlightsStatus.statusDict[SlotHighlightTypes.IsLegalRangedAttackOfCurrentTurn]);
            }
        }
        
        public void SetHighlightStatus(SlotHighlightTypes type, bool status)
        {
            _slotHighlightsStatus.SetStatus(type, status);
            
            UpdateMaterial();
        }

        private void OnDestroy()
        {
            _slotHighlightManager?.Unsubscribe();
            _slotHighlightManager = null;
            SlotList.Remove(this);
            BoardInputManager.Instance.OnClick -= OnClick;
        }
        
        private void OnClick(BoardSlot boardSlot, Card card)
        {
            if (GameUI.IsUIOpened())
            {
                return;
            }
            
            if (boardSlot == this && card == null) // We only select a slot if there is no card.
            {
                Game game = GameClient.GetGameData();
                int playerID = GameClient.Get().GetPlayerID();
                
                bool isSelector = game.selector == SelectorType.SelectTarget || game.selector == SelectorType.SelectMultipleTarget;
                
                if (!isSelector || playerID != game.selectorPlayer)
                {
                    return;
                }
                
                GameClient.Get().SelectSlot(GetSlot());
            }
        }
        
        private void SetMaterial(Material material)
        {
            if (_meshRenderer == null)
            {
                _meshRenderer = GetComponent<MeshRenderer>();
            }
            
            _meshRenderer.material = material;
        }
        
        public static BoardSlot Get(Slot slot)
        {
            return Get(slot.x, slot.y);
        }
        
        public static BoardSlot Get(Vector2S coordinate)
        {
            return Get(coordinate.x, coordinate.y);
        }

        private static BoardSlot Get(int x, int y)
        {
            foreach (BoardSlot boardSlot in GetAll())
            {
                Slot slot = boardSlot.GetSlot();
                if (slot.x == x && slot.y == y)
                    return boardSlot;
            }
            return null;
        }

        private static List<BoardSlot> GetAll()
        {
            return SlotList;
        }

        public static List<BoardSlot> GetBoardSlotWithTheseCards(List<string> cardsUID, Game game)
        {
            List<BoardSlot> slots = new List<BoardSlot>();
            
            foreach (var uid in cardsUID)
            {
                Card card = game.GetBoardCard(uid);
                if (card != null)
                {
                    slots.Add(Get(card.GetCoordinates()));
                }
            }

            return slots;
        }
        
        public static List<BoardSlot> GetBoardSlotsFromSlots(List<Slot> slots)
        {
            List<BoardSlot> boardSlots = new List<BoardSlot>();
            foreach (var slot in slots)
            {
                boardSlots.Add(Get(slot));
            }

            return boardSlots;
        }

        public static List<BoardSlot> GetBoardSlotsFromCoordinates(Vector2S[] list)
        {
            if (list == null || list.Length == 0)
            {
                return new List<BoardSlot>();
            }
            
            List<BoardSlot> boardSlots = new List<BoardSlot>();
            foreach (var coordinate in list)
            {
                boardSlots.Add(Get(coordinate));
            }

            return boardSlots;
        }
    }
}