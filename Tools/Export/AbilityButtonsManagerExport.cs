using System.Collections;
using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using TcgEngine;
using TcgEngine.Client;
using TcgEngine.UI;
using UnityEngine;

namespace Monarchs
{
    public class AbilityButtonsManagerExport : AbilityButtonsManager
    {
        // Start is called before the first frame update
        protected override void Start()
        {

        }
        
        public  void SetupAbilityButtons(Card card)
        {
            //Ability buttons
            foreach (AbilityButton button in abilityButtons)
            {
                button.Hide();
            }
            
            int index = 0;
            CardData icard = CardData.Get(card.cardID);
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
}
