using System.Collections.Generic;
using Monarchs;
using Monarchs.Client;
using Monarchs.Logic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UIElements;

public class PieceOnBoard : MonoBehaviour
{
    [Required]public GameObject baseModel;
    [Required]public GameObject pieceImage;
    [Required]public GameObject hpGemBaseModel;
    [Required]public GameObject attackGemBaseModel;
    [Required]public Material whiteTransparentMaterial;
    [Required]public Material blackTransparentMaterial;
    public bool isExported = false;
    private Card _card;
    private Material _normalMaterial;
    private IncorporealBoardEffect _incorporealEffect;

    private static List<PieceOnBoard> piece_list = new List<PieceOnBoard>();

    void Awake()
    {
        if (isExported)
            return;
        piece_list.Add(this);
        GameClient.Get().onCardTransformed += OnCardTransform;
        GameClient.Get().onRefreshAll += OnRefreshAll;
    }

    private void OnRefreshAll()
    {
        SetIncorporealEffect();
    }

    void OnDestroy()
    {
        if (isExported)
            return;
        piece_list.Remove(this);
        GameClient.Get().onCardTransformed -= OnCardTransform;
        _incorporealEffect?.OnDestroy();
    }
    
    private void OnCardTransform(Card card)
    {
        if (card.uid == _card.uid)
        {
            SetPiece(card);
        }
    }
    
    public void SetPiece(Card card)
    {
        _card = card;
        pieceImage.GetComponent<MeshRenderer>().material.mainTexture = _card.CardData.artBoard.texture;
        pieceImage.SetActive(true);
        SetPieceBaseMaterialAndMesh(_card.OwnedByFirstPlayer(GameClient.GetGameData()));
    }
    
    public void SetPieceForExport(Card card)
    {
        _card = card;
       
        pieceImage.GetComponent<MeshRenderer>().material.mainTexture = _card.CardData.artBoard.texture;
        pieceImage.SetActive(true);

        SetPieceBaseMaterialAndMesh(true);
        
    }

    private void SetPieceBaseMaterialAndMesh(bool firstPlayer)
    {
        if (firstPlayer)
        {
            SetPieceWhite();
        }
        else
        {
            SetPieceBlack();
        }
        SetIncorporealEffect();
    }

    public void SwapColor()
    {
        if (_card.OwnedByFirstPlayer(GameClient.GetGameData()))
        {
            SetPieceBlack();
        }
        else
        {
            SetPieceWhite();
        }
    }

    public void SetPieceWhite()
    {
        PieceBaseLink.PieceBasePrefabLink? baseLinkedInfo = PieceBaseLink.Instance.GetPieceBase(_card.GetPieceType());

        baseModel.GetComponent<MeshRenderer>().material = baseLinkedInfo?.whiteMaterial;
        attackGemBaseModel.GetComponent<MeshRenderer>().material = baseLinkedInfo?.whiteMaterial;
        hpGemBaseModel.GetComponent<MeshRenderer>().material = baseLinkedInfo?.whiteMaterial;
    }

    public void SetPieceBlack()
    {
        PieceBaseLink.PieceBasePrefabLink? baseLinkedInfo = PieceBaseLink.Instance.GetPieceBase(_card.GetPieceType());
        
        baseModel.GetComponent<MeshRenderer>().material = baseLinkedInfo?.blackMaterial;
        attackGemBaseModel.GetComponent<MeshRenderer>().material = baseLinkedInfo?.blackMaterial;
        hpGemBaseModel.GetComponent<MeshRenderer>().material = baseLinkedInfo?.blackMaterial;
    }
    
    private void SetIncorporealEffect()
    {
        if (_card.HasTrait("incorporeal") && _incorporealEffect == null)
        {
            _incorporealEffect = new IncorporealBoardEffect();
            _incorporealEffect.ApplyIncorporealEffect(transform, pieceImage, baseModel, _card.OwnedByFirstPlayer(GameClient.GetGameData()));
        }
        else if (!_card.HasTrait("incorporeal") && _incorporealEffect != null)
        {
            _incorporealEffect.RemoveIncorporealEffect();
            _incorporealEffect = null;
        }
    }

    public void ApplyTransparentMaterial()
    {
        if (_card == null)
        {
            Debug.LogError("Tried to change material on a piece, without a card assigned");
            return;
        }
        RecursiveTransparent.SetTransparency(0.5f, gameObject);
    }

    public void ApplyNormalMaterial()
    {
        if (_normalMaterial == null)
        {
            Debug.LogError("Tried to change material without _baseMaterial assigned");
            return;
        }
        baseModel.GetComponent<MeshRenderer>().material = _normalMaterial;
        attackGemBaseModel.GetComponent<MeshRenderer>().material = _normalMaterial;
        hpGemBaseModel.GetComponent<MeshRenderer>().material = _normalMaterial;
    }

    public void StartIncorporealHoverEffect()
    {
        _incorporealEffect?.StartHoverEffect(transform);
    }
    
    public void StopIncorporealHoverEffect()
    {
        _incorporealEffect?.StopHoverEffect();
    }

    public string GetCardUID()
    {
        Debug.Log(_card.uid);
        return _card.uid;
    }
    
    public Card GetCard()
    {
        return _card;
    }
    
    public static PieceOnBoard Get(string uid)
    {
        foreach (PieceOnBoard piece in piece_list)
        {
            if (piece.GetCardUID() == uid)
                return piece;
        }
        return null;
    }

    public static List<PieceOnBoard> GetAll()
    {
        return piece_list;
    }
}
