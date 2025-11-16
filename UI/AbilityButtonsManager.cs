using Monarchs.Ability;
using Monarchs.Client;
using Monarchs.Initiative;
using Monarchs.Logic;
using TcgEngine;
using TcgEngine.UI;
using UnityEngine;
using UnityEngine.UI;

public class AbilityButtonsManager : MonoBehaviour
{
    public AbilityButton[] abilityButtons;
    public Image currentPieceImage;

    public ColorSO allyColor;
    public ColorSO enemyColor;
    
    public GameObject leftOutline;
    public GameObject rightOutline;
    public GameObject midOutline;
    public GameObject bigOutline;
    public bool isPlayer;

    private Card _card;
    
    // Start is called before the first frame update
    protected virtual void Start()
    {
        if (GameplayData.Get().InitiativeType == InitiativeManager.InitiativeType.AllPiecesEveryTurn)
        {
            PlayerControls.OnSelectBoardCard += SetupAbilityButton;
            GameClient.Get().onNewTurn += OnNewTurn;
        }
        else
        {
            GameClient.Get().onNewTurn += SetupAbilityButton;
            GameClient.Get().onGameStart += GameStart;
        }
        gameObject.SetActive(false);
    }

    private void OnNewTurn(Card arg0)
    {
        
        gameObject.SetActive(false);
    }

    void GameStart()
    {
        gameObject.SetActive(false);
    }
    
    protected virtual void SetupAbilityButton(Card card)
    {
        _card = card;
        if (card == null)
        {
            gameObject.SetActive(false);
            return;
        }
        
        //Ability buttons
        foreach (AbilityButton button in abilityButtons)
        {
            button.Hide();
        }
        
        gameObject.SetActive(true);
        
        if (currentPieceImage)
        {
            currentPieceImage.sprite = card.CardData.artFull;
        }
        
        int abilityCount = System.Array.FindAll(card.GetAllCurrentAbilities(), a => a && a.trigger == AbilityTrigger.Activate).Length;
        if (abilityCount == 0)
        {
            leftOutline.SetActive(false);
            rightOutline.SetActive(false);
            midOutline.SetActive(false);
        }
        if (abilityCount == 1)
        {
            ShowOneAbility(card);
        }
        else if (abilityCount == 2)
        {
            ShowTwoAbilities(card);
        }

        ValidateExhaust();
        
        ValidateIfOpponentCard();
    }

    private void ValidateIfOpponentCard()
    {
        if (_card.playerID == GameClient.Get().GetPlayerID())
        {
            SetOutlineColor(allyColor.color);
        }
        else
        {
            SetOutlineColor(enemyColor.color);
        }
    }
    
    private void SetOutlineColor(Color color)
    {
        bigOutline.GetComponent<RawImage>().color = color;
        leftOutline.GetComponent<RawImage>().color = color;
        rightOutline.GetComponent<RawImage>().color = color;
        midOutline.GetComponent<RawImage>().color = color;
    }

    private void ShowOneAbility(Card card)
    {
        leftOutline.SetActive(false);
        rightOutline.SetActive(false);
        midOutline.SetActive(true);
        
        int index = 2;
        foreach (AbilityData iability in card.GetAllCurrentAbilities())
        {
            if (iability && iability.trigger == AbilityTrigger.Activate)
            {
                if (index < abilityButtons.Length)
                {
                    AbilityButton button = abilityButtons[index];
                    button.SetAbility(card, iability);
                    return;
                }
            }
        }
    }
    
    private void ShowTwoAbilities(Card card)
    {
        leftOutline.SetActive(true);
        rightOutline.SetActive(true);
        midOutline.SetActive(false);
        
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

    private void ValidateExhaust()
    {
        bool isOpponentCard = _card.playerID != GameClient.Get().GetPlayerID();
        if (_card != null && (_card.exhausted || !GameClient.GetGameData().GetCurrentCardTurn().Contains(_card)) && !isOpponentCard)
        {
            leftOutline.SetActive(false);
            rightOutline.SetActive(false);
            midOutline.SetActive(false);
            bigOutline.SetActive(false);
            currentPieceImage.color = new Color(1, 1, 1, 0.5f);
        }
        else
        {
            bigOutline.SetActive(true);
            currentPieceImage.color = new Color(1, 1, 1, 1);
        }
    }
}
