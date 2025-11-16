using System.Collections;
using System.Collections.Generic;
using Monarchs.Client;
using Monarchs.Logic;
using TMPro;
using UnityEngine;

namespace Monarchs
{
    public class MulliganUIManager : MonoBehaviour
    {
        public GameObject mulliganCardPrefab;
        public GameObject mulliganCardParent;
        public GameObject mulliganButton;
        public GameObject waitingOnNextPlayerText;
        public GameObject deckPosition;
        public TMP_Text mulliganTimerText;
        public float discardDuration = 1f;
        public float delay = 0.5f;
        public float drawDuration = 1f;
        private bool isResolvingMulligan = false;
        bool updatedDiscardCards = false;
        private bool destroyAfterMulligan = false;
        [HideInInspector] List<MulliganCard> mulliganCardObjects;
        [HideInInspector] List<Card> mulliganCards;
        
        public void Start()
        {
            GameClient.Get().onMulligan += onMulligan;
        }

        private void onMulligan(int playerId)
        {
            Game game = GameClient.GetGameData();

            if (game.State == GameState.Mulligan && (playerId == GameClient.Get().GetPlayerID() || playerId < 0)) //-1 is called when mulligan starts
            {
                if (updatedDiscardCards)
                {
                    return;
                }
                
                if (game.mulliganTimer <= 0 && !GameClient.Get().GetPlayer().submittedMulligan)
                {
                    OnMulliganButtonClicked();
                }
                
                SetMulliganCardFromGame(game, GameClient.Get().GetPlayerID());
            }
            
            if (game.State == GameState.Play || game.BothPlayersSubmittedMulligan() || playerId == -2) //-2 means timer expired
            {
                ResumeGame();
            }
        }
        
        public void Update()
        {
            if (!GameClient.Get().IsReady()) return;
            
            Game game = GameClient.GetGameData();
            if (game.State == GameState.Mulligan)
            {
                mulliganTimerText.text = Mathf.RoundToInt(game.mulliganTimer).ToString();
                game.mulliganTimer -= Time.deltaTime;
            }
            
            if (game.State != GameState.Mulligan || game.mulliganTimer <= 0 || game.BothPlayersSubmittedMulligan())
            {
                ResumeGame();
            }

        }

        public void ResumeGame()
        {
            if (isResolvingMulligan)
            {
                destroyAfterMulligan = true;
                return;
            }
            Destroy(gameObject);
        }
        
        public void OnMulliganButtonClicked()
        {
            isResolvingMulligan = true;
            mulliganButton.SetActive(false);
            waitingOnNextPlayerText.SetActive(true);
            List<Card> mulliganedCards = new List<Card>();
            foreach (var mulliganCard in mulliganCardObjects)
            {
                if (mulliganCard.isToBeDiscarded)
                {
                    mulliganedCards.Add(mulliganCard.card);
                }
            }
            GameClient.Get().MulliganCards(mulliganedCards);
        }

        public void SetMulliganCardFromGame(Game game, int playerId)
        {
            mulliganCards = game.players[playerId].cards_hand;
            UpdateMulliganCards();
        }
        
        private void UpdateMulliganCards()
        {
            if (mulliganCardObjects == null)
            {
                SetCardsBeforeMulligan();
            }
            else
            {
                StartCoroutine(SetCardsAfterMulligan());
            }
        }
        
        private void SetCardsBeforeMulligan()
        {
            mulliganCardObjects = new List<MulliganCard>();
            
            foreach (var card in mulliganCards)
            {
                var cardObject = Instantiate(mulliganCardPrefab, mulliganCardParent.transform);
                var mulliganCard = cardObject.GetComponent<MulliganCard>();
                mulliganCard.SetCard(card);
                mulliganCardObjects.Add(mulliganCard);
            }
            
        }

        private IEnumerator SetCardsAfterMulligan()
        {
            isResolvingMulligan = true;
            for (int i = 0; i < mulliganCardObjects.Count; i++)
            {
                if (mulliganCardObjects[i].isToBeDiscarded)
                {
                    StartCoroutine(mulliganCardObjects[i].DiscardAndSetNewCard(mulliganCards[i], deckPosition, discardDuration, delay, drawDuration));
                    yield return new WaitForSeconds(0.3f);
                }
                else
                {
                    mulliganCardObjects[i].currentStatusIcon.SetActive(false);
                    mulliganCardObjects[i].glow.gameObject.SetActive(false);
                }
            }
            updatedDiscardCards = true;
            
            yield return new WaitForSeconds(3f);
            
            isResolvingMulligan = false;
            if (destroyAfterMulligan)
            {
                Destroy(gameObject);
            }
        }
    }
    
}
