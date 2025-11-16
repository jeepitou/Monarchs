using System;
using System.Collections;
using System.Collections.Generic;
using Monarchs.Client;
using TcgEngine;
using TcgEngine.Client;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurrentTurnPieceInitiativeUI : PieceInitiativeUI
{
    [SerializeField] private Image _nextTurnImage;
    [SerializeField] private Image _nextTurnGradiant;

    public void UpdateNextTurnImage()
    {
        _nextTurnImage.sprite = _card.CardData.artBoard;
    }

    public void UpdateNextTurnGradiant()
    {
        if (_card.playerID == GameClient.GetGameData().firstPlayer)
        {
            _nextTurnGradiant.color = whiteGradientColor;
        }
        else
        {
            _nextTurnGradiant.color = blackGradientColor;
        }
    }

}
