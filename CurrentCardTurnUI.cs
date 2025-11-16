using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Client;
using Monarchs.Logic;
using TcgEngine;
using TcgEngine.Client;
using TcgEngine.UI;
using UnityEngine;

public class CurrentCardTurnUI : MonoBehaviour
{
    public UnitCardUI cardUI;
    public AbilityButton[] abilityButtons;
    
    // Start is called before the first frame update
    void Start()
    {
        GameClient.Get().onNewTurn += OnNewTurn;
        //GameClient.Get().onGameStart += GameStart;
    }

    void GameStart()
    {
        cardUI.SetCard(GameClient.Get().GetCurrentPieceTurn()[0]);
    }
    
    void OnNewTurn(Card card)
    {
        cardUI.SetCard(card);
        SetupAbilityButton(card);
    }

    void SetupAbilityButton(Card card)
    {
        //Ability buttons
        foreach (AbilityButton button in abilityButtons)
        {
            button.Hide();
        }
        
        int index = 0;
        foreach (AbilityData iability in card.GetAllCurrentAbilities())
        {
            if (iability && iability.trigger == AbilityTrigger.Activate)
            {
                if (index < abilityButtons.Length)
                {
                    AbilityButton button = abilityButtons[index];
                    button.SetAbility(card, iability);
                }
                index++;
            }
        }
        
    }
}
