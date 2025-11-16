using System.Collections;
using System.Collections.Generic;
using Monarchs.Board;
using Monarchs.Logic;
using TcgEngine;
using UnityEngine;

public class BoardTrap : BoardElement
{
    public override void SetCard(Card card)
    {
        _cardUID = card.uid;

        transform.position = GetTargetPos();

    }

    public override void UpdateUI(Card card, Game data, bool showFX=true)
    {
        
    }
    
}
