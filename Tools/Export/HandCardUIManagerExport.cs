using TcgEngine;

namespace Monarchs.Tools
{
    public class HandCardUIManagerExport: HandCardUIManager
    {
        public override void SetCard(ICard card, VariantData variant)
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
}