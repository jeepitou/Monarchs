using Monarchs.Logic;
using TcgEngine.UI;
using UnityEngine;

namespace Monarchs.Client
{
    public class HandCardExporting : HandCard
    {
        protected override void Update(){}

        public override void SetCard(Card card)
        {
            SetCardForExport(card);
        }
        
        public override Card GetCard()
        {
            return _card;
        }
        
        public void SetFocus()
        {
            SetFocusedProperties();
            SetOnTop();
            infoBoxList.Show();
            _cardUI.cardUI.ShowDescription();
            _cardUI.cardUI.ShowAOEPatterns();
            _cardTransform.anchoredPosition = _targetPosition;
            _cardTransform.localRotation = Quaternion.Euler(_currentRotate);
            _cardTransform.localScale = _targetSize;
        }
        
        private void SetCardForExport(Card card)
        {
            _card = card;
            _cardUID = card.uid;
            _cardUI.SetCard(card);
            infoBoxList.SetCard(card);
            
            cohortCardCopies.Remove(cohortCardCopy);
            ClearCohortCardCopiesForExport();
            
            if (card.cohortSize > 1)
            {
                cohortCardCopies.Add(cohortCardCopy);
                cohortCardCopy.SetActive(true);
                cohortCardCopy.GetComponent<UnitCardUI>().SetCard(card);
                
                for (int i = 2; i < card.cohortSize; i++)
                {
                    GameObject copy = Instantiate(cohortCardCopy, cohortCardCopy.transform.position, Quaternion.identity, cohortCardCopy.transform.parent);
                    copy.transform.SetSiblingIndex(0);
                    
                    cohortCardCopies.Add(copy);
                }
            }
            else
            {
                cohortCardCopy.SetActive(false);
            }
        }
        
        private void ClearCohortCardCopiesForExport()
        {
            foreach (var cohortCopy in cohortCardCopies)
            {
                Destroy(cohortCopy);
            }
            cohortCardCopies.Clear();
        }
    }
}