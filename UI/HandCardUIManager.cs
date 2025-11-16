using System.Collections;
using System.Collections.Generic;
using Monarchs.Logic;
using Sirenix.OdinInspector;
using TcgEngine;
using TcgEngine.Client;
using TcgEngine.UI;
using UnityEngine;

public class HandCardUIManager : MonoBehaviour
{
    [HideInInspector]public CardUI cardUI;
    
    [SerializeField] protected UnitCardUI _unitCardUI;
    [SerializeField] protected InterventionCardUI _interventionCardUI;

    private CardData _card;

    public void Hide()
    {
        _unitCardUI.Hide();
        _interventionCardUI.Hide();
    }

    public void SetCard(Card card)
    {
        if (card == null)
            return;
        
        SetCard(card, VariantData.GetDefault());
    }
    
    public virtual void SetCard(ICard card, VariantData variant)
    {
        if (card == null)
            return;

        CardType cardType = card.GetCardData().cardType;
        
        if (cardType == CardType.Character)
        {
            _unitCardUI.SetCard(card, variant);
            _interventionCardUI.Hide();
            cardUI = _unitCardUI;
        }

        if (cardType == CardType.Spell || cardType == CardType.Trap)
        {
            _interventionCardUI.SetCard(card, variant);
            _unitCardUI.Hide();
            cardUI = _interventionCardUI;
        }
    }
}
