using Monarchs.Client;
using Monarchs.Logic;
using TcgEngine;
using TcgEngine.Client;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// This shoots event when a tile is hovered, clicked or a click is released
/// </summary>
public class BoardInputManager : Singleton<BoardInputManager>
{
    //Events
    public UnityAction<BoardSlot, Card> OnHover;
    public UnityAction<BoardSlot, Card> OnClick;
    public UnityAction<BoardSlot, Card> OnClickRelease;
    public UnityAction<Card> OnInitiativeCardHover;

    public bool Dragging = false;

    public Card hoveredInitiativeCard = null;
    
    private Camera _currentCamera;
    private BoardSlot _lastSlotHovered;
    private BoardSlot _lastSlotClicked;
    private BoardSlot _lastSlotReleased;
    private Vector3 _hoverCoordinate;

    public void SetHoverInitiativeCard(Card card)
    {
        hoveredInitiativeCard = card;
        OnInitiativeCardHover?.Invoke(card);
    }
    
    public Vector3 GetMousePositionOnBoard()
    {
        return _hoverCoordinate;
    }

    public BoardSlot GetLastHoveredSlot()
    {
        return _lastSlotHovered;
    }
    
    
    public BoardSlot GetLastClickedSlot()
    {
        return _lastSlotClicked;
    }
    
    private void Update()
    {
        if (!_currentCamera)
        {
            _currentCamera = Camera.main;
            return;
        }
        
        CheckHover();
        CheckClick();
    }

    private void CheckHover()
    {
        RaycastHit info;
        Ray ray = _currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile")))
        {
            _hoverCoordinate = info.point;
            BoardSlot hoveredSlot = GetSlotFromGameObject(info.transform.gameObject);

            //If we're hovering a new tile
            if (_lastSlotHovered != hoveredSlot)
            {
                _lastSlotHovered = hoveredSlot;
                OnHover?.Invoke(_lastSlotHovered, GetLastHoveredPiece());
            }
        }
        else
        {
            //If we went from hovering a tile to hovering no tile
            if (_lastSlotHovered != null)
            {
                _lastSlotHovered = null;
                OnHover?.Invoke(null, null);
            }
        }
    }

    private void CheckClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (_lastSlotHovered != null)
            {
                OnClick?.Invoke(_lastSlotHovered, GetLastHoveredPiece());
                _lastSlotClicked = _lastSlotHovered;
                Dragging = true;
            }
            else
            {
                OnClick?.Invoke(null, null);
            }
        }
        
        if (Input.GetMouseButtonUp(0))
        {
            if (_lastSlotHovered != null)
            {
                OnClickRelease?.Invoke(_lastSlotHovered, GetLastHoveredPiece());
                _lastSlotReleased = _lastSlotHovered;
                Dragging = false;
            }
            else
            {
                OnClickRelease?.Invoke(null, null);
            }
        }
    }
    
    private BoardSlot GetSlotFromGameObject(GameObject hitInfo)
    {
        return hitInfo.GetComponent<BoardSlot>();
    }

    private Card GetLastHoveredPiece()
    {
        if (_lastSlotHovered != null)
        {
            return GameClient.GetGameData()?.GetSlotCard(_lastSlotHovered.GetSlot());
        }

        return null;
    }
}
