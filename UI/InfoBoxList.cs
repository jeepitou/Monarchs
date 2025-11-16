using System.Collections.Generic;
using Monarchs.Ability;
using Monarchs.Logic;
using TcgEngine;
using TcgEngine.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Monarchs
{
    public class InfoBoxList : MonoBehaviour
    {
        public GameObject cardInfoBoxPrefab;
        public GameObject statusLineParent;
        public GameObject abilityLineParent;
        public CanvasGroup canvasGroup;
        
        private Card _card;
        private List<CardInfoBox> _statusLines = new List<CardInfoBox>();
        private List<CardInfoBox> _abilityLines = new List<CardInfoBox>();
        private bool _showing = false;

        void Update()
        {
            if (_showing)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(statusLineParent.transform as RectTransform);
                LayoutRebuilder.ForceRebuildLayoutImmediate(abilityLineParent.transform as RectTransform);
            }
        }
        
        public void SetCard(Card card)
        {
            _card = card;
            DeleteLines();
            
            CreateActiveAbilities(card);
            CreatePassiveAbilities(card);
            abilityLineParent.SetActive(_abilityLines.Count != 0);
            CreateStatus(card);
        }
        
        public void Show()
        {
            _showing = true;
            canvasGroup.alpha = 1;
        }

        public void Hide()
        {
            _showing = false;
            canvasGroup.alpha = 0;
        }
        
        void CreateActiveAbilities(Card card)
        {
            foreach (AbilityData ability in card.GetAllCurrentAbilities())
            {
                if (ability.trigger == AbilityTrigger.Activate)
                {
                    if (!string.IsNullOrWhiteSpace(ability.desc) && !ability.DontShowOnPreview)
                    {
                        GameObject g = Instantiate(cardInfoBoxPrefab, abilityLineParent.transform);
                        _abilityLines.Add(g.GetComponent<CardInfoBox>());
                        g.GetComponent<CardInfoBox>().SetInfoBox($"<b>{ability.GetTitle()}</b>: {ability.GetDesc(card.CardData)}", "Active");
                    }
                }
            }
        }
        
        void CreatePassiveAbilities(Card card)
        {
            foreach (AbilityData ability in card.GetAllCurrentAbilities())
            {
                if (ability.trigger != AbilityTrigger.Activate)
                {
                    if (!string.IsNullOrWhiteSpace(ability.desc) && !ability.DontShowOnPreview)
                    {
                        GameObject g = Instantiate(cardInfoBoxPrefab, abilityLineParent.transform);
                        _abilityLines.Add(g.GetComponent<CardInfoBox>());
                        g.GetComponent<CardInfoBox>().SetInfoBox($"<b>{ability.GetTitle()}</b>: {ability.GetDesc(card.CardData)}");
                    }
                }
            }
        }

        

        void CreateStatus(Card card)
        {
            foreach (CardStatus status in card.status)
            {
                GameObject g = Instantiate(cardInfoBoxPrefab, statusLineParent.transform);
                _statusLines.Add(g.GetComponent<CardInfoBox>());
                    
                    
                StatusData istatus = StatusData.Get(status.type);
                if (istatus != null && !string.IsNullOrWhiteSpace(istatus.desc))
                {
                    int ival = Mathf.Max(status.value, Mathf.CeilToInt(status.duration / 2f));

                    string description = $"<b>{istatus.GetTitle()}</b>: {istatus.GetDesc(ival)} ({status.duration} rounds)";
                        
                    g.GetComponent<CardInfoBox>().SetInfoBox(description, "Status");
                }
                    
            }
        }
        
        void DeleteLines()
        {
            foreach (CardInfoBox line in _statusLines)
                Destroy(line.gameObject);
            _statusLines.Clear();
            
            foreach (CardInfoBox line in _abilityLines)
                Destroy(line.gameObject);
            _abilityLines.Clear();
        }
    }
}
