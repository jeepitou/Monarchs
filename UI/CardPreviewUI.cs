using Monarchs;
using Monarchs.Board;
using Monarchs.Client;
using Monarchs.Logic;
using UnityEngine;
using TcgEngine.Client;

namespace TcgEngine.UI
{
    /// <summary>
    /// In the game scene, the CardPreviewUI is what shows the card in big with extra info when hovering a card
    /// </summary>

    public class CardPreviewUI : MonoBehaviour
    {
        public UIPanel uiPanel;
        public HandCardUIManager cardUIManager;
        public float hoverDelayBoard = 0.7f;
        public float hoverDelayMobile = 0.1f;
        public InfoBoxList infoBoxList;

        public RectTransform[] sideRows;
        public bool isCardExporter = false;
        

        private float _previewTimer = 0f;
        private Vector2[] _startPosition;
        private string _lastCardUID;
        bool shown = false;

        private void Start()
        {
            _startPosition = new Vector2[sideRows.Length];
            for (int i = 0; i < sideRows.Length; i++)
            {
                _startPosition[i] = sideRows[i].anchoredPosition;
            }
        }

        void Update()
        {
            if (isCardExporter)
                return;
            
            if (!GameClient.Get().IsReady() || GameClient.GetGameData().State == GameState.Mulligan)
                return;
            
            //HandCard hcard = HandCard.GetFocus();
            BoardElement bcard = BoardElement.GetFocus();
            //HeroUI hero_ui = HeroUI.GetFocus();

            PlayerControls controls = PlayerControls.Get();

            float delay = hoverDelayBoard;
            if (GameTool.IsMobile())
                delay = hoverDelayMobile;

            Card pcard =bcard?.GetCard();

            bool hoverOnly = !Input.GetMouseButton(0) && !HandCardArea.Get().IsDragging();
            bool shouldShowPreview = hoverOnly && !GameUI.IsUIOpened() && pcard != null;

            if (pcard != null && _lastCardUID != pcard.uid)
                shouldShowPreview = false;

            if (shouldShowPreview)
                _previewTimer += Time.deltaTime;
            else
                _previewTimer = 0f;

            bool showPreview = shouldShowPreview && _previewTimer >= delay;

            if (!showPreview)
            {
                cardUIManager.Hide();
                infoBoxList.Hide();
                shown = false;
            }
            
            uiPanel.SetVisible(showPreview);
            _lastCardUID = pcard != null ? pcard.uid : "";

            if (showPreview && !shown)
            {
                shown = true;
                Debug.Log("Showing preview");
                CardData icard = pcard.CardData;
                infoBoxList.SetCard(pcard);
                infoBoxList.Show();
                cardUIManager.SetCard(icard, pcard.VariantData);
            }
        }
        
        //Used in the card exporter
        public void SetCard(Card card)
        {
            Card pcard = card;
            uiPanel.SetVisible(true);
            _lastCardUID = pcard != null ? pcard.uid : "";
            CardData icard = pcard.CardData;
            cardUIManager.SetCard(icard, pcard.VariantData);
            infoBoxList.SetCard(pcard);
            infoBoxList.Show();
        }
    }
}
