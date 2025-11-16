using System.Collections;
using DG.Tweening;
using Monarchs.Ability;
using Monarchs.Client;
using Monarchs.Logic;
using TMPro;
using UnityEngine;
using Image = UnityEngine.UI.Image;

namespace Monarchs.UI
{
    public class PlayedAbilityUI : MonoBehaviour
    {
        public Image abilityIcon;
        public TMP_Text abilityName;
        public TMP_Text abilityDescription;
        
        public void Start()
        {
            GameClient.Get().onAbilityEnd += OnAbilityEnd;
        }

        private void OnAbilityEnd(AbilityData ability, Card caster)
        {
            if (caster == null || caster.playerID == GameClient.Get().GetPlayerID()) return;
            if (ability.trigger == AbilityTrigger.Activate && !ability.DontShowToEnemyWhenPlayed)
            {
                GameClient.Get().animationManager.AddToQueue(ShowAbilityCoroutine(ability), gameObject);
            }
        }
        
        private IEnumerator ShowAbilityCoroutine(AbilityData ability)
        {
            SetAbility(ability);
            CanvasGroup canvas = GetComponent<CanvasGroup>();
            canvas.DOFade(1, 0.5f);
            yield return new WaitForSeconds(1.5f);
            canvas.DOFade(0, 2f);
        }

        public void SetAbility(AbilityData ability)
        {
            abilityIcon.sprite = ability.icon;
            abilityName.text = ability.GetTitle();
            abilityDescription.text = ability.GetDesc();
        }
    }
}