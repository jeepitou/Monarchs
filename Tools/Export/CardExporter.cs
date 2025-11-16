using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Monarchs;
using Monarchs.Client;
using Monarchs.Logic;
using Sirenix.OdinInspector;
using TcgEngine.Client;
using TcgEngine.FX;
using UnityEngine;
using UnityEngine.UI;
using TcgEngine.UI;

namespace TcgEngine
{
    /// <summary>
    /// Use this export all your cards to png images (so they have the stats/ui on top of the card)
    /// </summary>

    public class CardExporter : MonoBehaviour
    {
        public PieceBaseLink PieceBaseLink;
        public string export_path = "C:/CardsExport";
        public int width = 856;
        public int height = 1200;
        public VariantData variant;

        [Header("References")]
        public Camera render_cam;
        public HandCardArea handCard;
        public HandCardArea focusedCard;
        public PieceInitiativeUI pieceInitiativeUI;
        public AbilityButtonsManagerExport abilityButtonsManager;
        public CardPreviewUI cardPreviewUI;
        
        public bool exportPieces = false;
        public bool exportInterventions = false;

        public CardData cardToShow;

        private GameObject previousPiece;
        private RenderTexture texture;
        private Texture2D export_texture;

        void Start()
        {
            if (variant == null)
                variant = VariantData.GetDefault();

            //GenerateAll();
        }

        [Button]
        private async void GenerateAll()
        {
            QualitySettings.SetQualityLevel(QualitySettings.names.Length -1); //Set Max Quality level

            texture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
            export_texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
            texture.filterMode = FilterMode.Point;
            export_texture.filterMode = FilterMode.Point;
            render_cam.targetTexture = texture;
            //render_cam.orthographicSize = height / 2;

            List<CardData> cards = CardData.GetAll();
            for (int i = 0; i < cards.Count; i++)
            {
                CardData card = cards[i];
                if (!card.deckBuilding)
                    continue;
                
                if (card.IsBoardCard() && exportPieces)
                {
                    ShowText("Exporting: " + card.id);
                    abilityButtonsManager.gameObject.SetActive(true);
                    pieceInitiativeUI.gameObject.SetActive(true);
                    cardPreviewUI.gameObject.SetActive(true);
                    GenerateCard(card);
                    SpawnNewPiece(card);
                    
                    
                    await Task.Delay(2000);
                    ExportCard(card);
                    await Task.Delay(2);
                }

                if (!card.IsBoardCard() && exportInterventions)
                {
                    ShowText("Exporting: " + card.id);
                    if (previousPiece != null)
                    {
                        Destroy(previousPiece);
                    }
                    pieceInitiativeUI.gameObject.SetActive(false);
                    abilityButtonsManager.gameObject.SetActive(false);
                    cardPreviewUI.gameObject.SetActive(false);
                    Card _card = Card.Create(card, variant, 0);
                    handCard.SetCardForExport(_card);
                    focusedCard.SetCardForExport(_card);
                    
                    await Task.Delay(2000);
                    ExportCard(card);
                    await Task.Delay(2);
                }
                
            }

            ShowText("Completed!");
        }

        [Button]
        public void ShowOneCard()
        {
            render_cam.targetTexture = null;
            Card card = Card.Create(cardToShow, variant, 0);
            
            if (cardToShow.IsBoardCard() && exportPieces)
            {
                abilityButtonsManager.gameObject.SetActive(true);
                pieceInitiativeUI.gameObject.SetActive(true);
                cardPreviewUI.gameObject.SetActive(true);
                GenerateCard(cardToShow);
                if (cardToShow.IsBoardCard())
                {
                    SpawnNewPiece(cardToShow);
                }
            }

            if (!cardToShow.IsBoardCard() && exportInterventions)
            {
                if (previousPiece != null)
                {
                    Destroy(previousPiece);
                }
                pieceInitiativeUI.gameObject.SetActive(false);
                abilityButtonsManager.gameObject.SetActive(false);
                cardPreviewUI.gameObject.SetActive(false);
                Card _card = Card.Create(cardToShow, variant, 0);
                handCard.SetCardForExport(_card);
                focusedCard.SetCardForExport(_card);
            }
        }

        private void GenerateCard(CardData cardData)
        {
            Card card = Card.Create(cardData, variant, 0);
            handCard.SetCardForExport(card);
            focusedCard.SetCardForExport(card);
            pieceInitiativeUI.SetCard(card);
            pieceInitiativeUI.UpdateUI(true);
            abilityButtonsManager.SetupAbilityButtons(card);
            cardPreviewUI.SetCard(card);
            render_cam.Render();
        }

        private void ExportCard(CardData card)
        {
            RenderTexture.active = texture;
            export_texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            byte[] bytes = export_texture.EncodeToPNG();
            string file = card.id + ".png";
            File.WriteAllBytes(export_path + "/" + file, bytes);
            RenderTexture.active = null;
        }

        private void ShowText(string txt)
        {
            Debug.Log(txt);
        }
        
        private void SpawnNewPiece(CardData cardData)
        {
            if (previousPiece != null)
            {
                Destroy(previousPiece);
            }
            
            Card card = Card.Create(cardData, VariantData.GetDefault(), 0);
            Vector3 position = Vector3.zero;
            GameObject piece_obj = PieceBaseLink.GetPieceBase(card.GetPieceType()).piecePrefab;
           
            
            piece_obj.GetComponent<PieceOnBoard>().isExported = true;
            piece_obj.GetComponent<BoardCard>().isExported = true;
            piece_obj.GetComponent<BoardCardFX>().enabled = false;
            piece_obj.SetActive(true);
            GameObject piece = Instantiate(PieceBaseLink.GetPieceBase(card.GetPieceType()).piecePrefab, position, Quaternion.identity);
            piece.transform.localScale = new Vector3(300, 300, 300);
            piece.transform.position = new Vector3(-850, -113, 0);
            piece.transform.rotation = Quaternion.Euler(-90, 0, 0);
            piece.GetComponent<PieceOnBoard>().SetPieceForExport(card);
            piece.GetComponent<BoardCard>().SetUIForExport(card);
            piece.GetComponent<PieceOnBoard>().enabled = false;
            piece.GetComponent<BoardCard>().enabled = false;
            
            piece_obj.GetComponent<PieceOnBoard>().isExported = false;
            piece_obj.GetComponent<BoardCard>().isExported = false;
            piece_obj.GetComponent<BoardCardFX>().enabled = true;
            
            previousPiece = piece;
        }
    }
}
