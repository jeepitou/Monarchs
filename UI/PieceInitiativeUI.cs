using System.Collections.Generic;
using DG.Tweening;
using Monarchs.Client;
using Monarchs.Logic;
using TcgEngine;
using TcgEngine.Client;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PieceInitiativeUI : MonoBehaviour
{
    public Color whiteGradientColor;
    public Color blackGradientColor;
    public static List<PieceInitiativeUI> allUIPieceInitiatives;

    [SerializeField] private TextMeshProUGUI _initiativeText;
    [SerializeField] private Image _pieceImage;
    [SerializeField] private GameObject horizontal_offset;
    [SerializeField] private GameObject _whitePieceFrame;
    [SerializeField] private GameObject _blackPieceFrame;
    [SerializeField] private GameObject _highlight;
    [SerializeField] private Image _gradientImage;
    [SerializeField] private Image _cohortImage;
    public Card _card;
    public CanvasGroup canvasGroup;
    public bool exporting = false;
    private float _horizontalOffsetInitialScale;
    private const float HOVER_SCALE_FACTOR = 1.1f;
    private const float CURRENT_TURN_SCALE_FACTOR = 1.2f;
    private const float INACTIVE_SCALE_FACTOR = 0.8f;
    private const float CURRENT_TURN_X_OFFSET = 10f;
    private const float INACTIVE_ALPHA = 0.5f;
    private const float TEMPORARY_SCALE_FACTOR = 0.9f;
    private float _initialScale;
    private bool disappearing = false;
    private bool active = false;
    private bool isCurrentTurn = false;
    private bool hovered = false;
    private bool pieceHoveredOnBoard = false;
    private InitiativeManagerUI _initiativeManagerUI;

    private void Awake()
    {
        if (allUIPieceInitiatives == null)
        {
            allUIPieceInitiatives = new List<PieceInitiativeUI>();
        }
        _highlight.SetActive(false);
        allUIPieceInitiatives.Add(this);
        _initialScale = transform.localScale.x;
        _horizontalOffsetInitialScale = horizontal_offset.transform.localScale.x;
    }

    private void OnDestroy()
    {
        allUIPieceInitiatives.Remove(this);
    }

    public void SetCard(string cohortUID)
    {
        _card = GameClient.GetGameData().GetBoardCardsOfCohort(cohortUID)?[0];

        if (_card == null)
        {
            Debug.LogError("Tried to set card with invalid UID in UIPieceInitiative.");
            return;
        }
    }

    public void SetCard(Card card)
    {
        _card = card;
    }

    public void SetCurrentTurn(string cohortUID)
    {
        if (_card.CohortUid == cohortUID)
        {
            isCurrentTurn = true;
            transform.DOScale(_initialScale * CURRENT_TURN_SCALE_FACTOR, 0.2f);
            _highlight.SetActive(true);
            SetHighlightColor(Color.white);
            //transform.position += new Vector3(CURRENT_TURN_X_OFFSET, 0, 0);
        }
        else if (active)
        {
            isCurrentTurn = false;
            transform.DOScale(_initialScale, 0.2f);
            _highlight.SetActive(false);
            //transform.position -= new Vector3(CURRENT_TURN_X_OFFSET, 0, 0);
        }
    }

    public void UpdateUI()
    {
        UpdateUI(false);
    }

    public void UpdateUI(bool newCard)
    {
        UpdateInitiative();
        UpdatePieceArt();
        UpdateGradiantColor();

        int cohortSize;
        if (newCard)
        {
            cohortSize = _card.cohortSize;
        }
        else
        {
            cohortSize = GameClient.GetGameData().GetBoardCardsOfCohort(_card.CohortUid).Count;
        }

        UpdateCohortIcon(cohortSize);
    }

    public void UpdateInitiative()
    {
        _initiativeText.text = _card.GetInitiative().ToString();
    }

    public void UpdatePieceArt()
    {
        _pieceImage.sprite = _card.CardData.artBoard;
    }

    public void UpdateGradiantColor()
    {
        if (exporting || _card.playerID == GameClient.GetGameData().firstPlayer)
        {
            _gradientImage.color = whiteGradientColor;
            _whitePieceFrame.SetActive(true);
            _blackPieceFrame.SetActive(false);
        }
        else
        {
            _gradientImage.color = blackGradientColor;
            _whitePieceFrame.SetActive(false);
            _blackPieceFrame.SetActive(true);
        }
    }

    public void SetActive(bool active)
    {
        canvasGroup.alpha = active ? 1 : INACTIVE_ALPHA;

        if (this.active == active && active)
        {
            return;
        }

        this.active = active;
        if (!active)
        {
            transform.localScale = (_initialScale * INACTIVE_SCALE_FACTOR) * Vector3.one;
        }
        else
        {
            transform.DOScale(_initialScale, 0.2f);
        }
    }

    public void SetAsTemporary(float alpha)
    {
        horizontal_offset.transform.localScale = new Vector3(_horizontalOffsetInitialScale, horizontal_offset.transform.localScale.y, horizontal_offset.transform.localScale.z);
        horizontal_offset.transform.DOKill();
        horizontal_offset.SetActive(true);
        horizontal_offset.transform.DOScaleX(horizontal_offset.transform.localScale.x * 2.5f, 0.4f).SetLoops(-1, LoopType.Yoyo);
        canvasGroup.alpha = alpha;
        transform.localScale = (_initialScale * TEMPORARY_SCALE_FACTOR) * Vector3.one;
        _highlight.SetActive(true);
        SetHighlightColor(Color.green);
    }

    public void UpdateCohortIcon(int cohortSize)
    {
        if (cohortSize > 1)
        {
            _cohortImage.sprite = GameplayData.Get().CohortIconLink.GetCohortIcon(cohortSize);
            _cohortImage.gameObject.SetActive(true);
            _cohortImage.enabled = true;
            return;
        }

        _cohortImage.enabled = false;
    }

    public void OnPointerEnter()
    {
        hovered = true;
        BoardInputManager.Instance.SetHoverInitiativeCard(_card);

        if (!isCurrentTurn)
        {
            transform.DOScale(_initialScale * HOVER_SCALE_FACTOR, 0.2f);
            _highlight.SetActive(true);
            SetHighlightColor(Color.gray);
        }
    }

    public void OnPointerExit()
    {
        hovered = false;
        if (BoardInputManager.Instance.hoveredInitiativeCard == _card)
        {
            BoardInputManager.Instance.SetHoverInitiativeCard(null);
        }

        if (!isCurrentTurn)
        {
            transform.DOScale(_initialScale, 0.2f);
            _highlight.SetActive(false);
            SetHighlightColor(Color.white);
        }
    }

    public void OnPointerEnterBoardCard()
    {
        pieceHoveredOnBoard = true;
        
        if (!isCurrentTurn)
        {
            _highlight.SetActive(true);
            SetHighlightColor(Color.gray);
        }
    }
    
    public void OnPointerExitBoardCard()
    {
        pieceHoveredOnBoard = false;
        
        if (!isCurrentTurn)
        {
            _highlight.SetActive(false);
            SetHighlightColor(Color.white);
        }
    }

    private void SetHighlightColor(Color color)
    {
        for (int i = 0; i < _highlight.transform.childCount; i++)
        {
            _highlight.transform.GetChild(i).GetComponent<Image>().color = color;
        }
    }
}