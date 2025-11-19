using System;
using System.Collections;
using System.Collections.Generic;
using Monarchs.Client;
using Monarchs.Logic;
using Monarchs.UI;
using Sirenix.OdinInspector;
using TcgEngine;
using TcgEngine.UI;
using UnityEngine;

namespace Monarchs.Board
{
    /// <summary>
    /// Singleton that manages the board. Might be replaced by the Game class of the TCG engine when I will have cleaned it. TODO
    /// It initializes everything the board need, and has helper functions.
    /// </summary>
    public class BoardManager : Singleton<BoardManager>
    {
        [Required] public PieceBaseLink pieceBaseLink;
        [Required] public GameObject trapPrefab;
        [Required] public GameObject slotPrefab;
        private BoardSlot[,] _slots;

        [Header("Board Data")] 
        [SerializeField] private float tileSize;
    
        private bool _gameEnded;
    

        private void Start()
        {
            GameClient.Get().onRefreshAll += OnRefreshAll;
            GameClient.Get().onConnectGame += OnConnectGame;
            GameClient.Get().onTrapTrigger += OnTrapTrigger;
            GameClient.Get().onCardDiscarded += OnCardDiscarded;
        }

        private void OnCardDiscarded(Card card)
        {
            BoardElement boardCard = BoardElement.Get(card.uid);
            if (boardCard != null && card.CardData.cardType == CardType.Character)
            {
                if (!boardCard.GetCard().HasAbility(trigger:Ability.AbilityTrigger.OnDeath))
                {
                    boardCard.Kill(true);
                }
            }
        }

        private void OnTrapTrigger(Card trap, Card triggerer)
        {
            Destroy(BoardElement.Get(trap.uid).gameObject);
        }

        private void OnRefreshAll()
        {
            Game data = GameClient.GetGameData();
        
            SyncBoardState(data);
            CheckWin(data);
        }
    
        private void OnConnectGame()
        {
            bool firstPlayer = GameClient.Get().IsFirstPlayer();
            _slots = new SlotGenerator().GenerateAllSlots(GameplayData.Get().boardSizeX, GameplayData.Get().boardSizeY, transform, tileSize, slotPrefab, firstPlayer);
            Debug.Log("BoardManager: Board initialized", this);
        }

        // This sync every board slot with the Game Data to display the correct pieces with the correct info
        private void SyncBoardState(Game data)
        {
            //Add missing cards and validate promotion
            foreach (Player p in data.players)
            {
                foreach (Card card in p.cards_board)
                {
                    BoardElement boardCard = BoardElement.Get(card.uid);
                    if (boardCard == null)
                        SpawnNewPiece(card);
                    else if (card.promoted && !boardCard.promoted)
                    {
                        Destroy(boardCard.gameObject);
                        SpawnNewPiece(card, true);
                    }
                }
            }
        
            //Show owned trap
            foreach (var trap in GameClient.Get().GetPlayer().cards_trap)
            {
                BoardElement boardCard = BoardElement.Get(trap.uid);
                if (boardCard == null)
                    SpawnNewTrap(trap);
            }
        
            foreach (var status in GameClient.GetGameData().slotStatusList)
            {
                BoardSlotStatus boardCard = BoardSlotStatus.Get(status.uid);
                if (boardCard == null && status.showOnBoard)
                    SpawnNewSlotStatus(status);
                if (boardCard != null && status.slot != boardCard.slot)
                {
                    MoveSlotStatus(status);
                }
            }


            List<BoardElement> cards = BoardElement.GetAll();
        
            //Vanish removed cards
            for (int i = cards.Count - 1; i >= 0; i--)
            {
                BoardElement card = cards[i];
                if (card && data.GetBoardCard(card.GetCard()?.uid) == null && !card.IsDead())
                {
                    if (data.GetHandCard(card.GetCard().uid) != null)
                    {
                        ((BoardCard)card).GetComponent<BoardCardTrapFx>().SendBackToHand();
                    }
                    else
                    {
                        card.UpdateUI(card.GetCard(), data);
                        card.Kill(card.GetCard().CardData.cardType != CardType.Trap);
                    }
                }
            }

            List<BoardSlotStatus> slotStatusList = BoardSlotStatus.allSlotStatus;
            //Vanish removed tileStatus
            for (int i = slotStatusList.Count - 1; i >= 0; i--)
            {
                BoardSlotStatus slotStatus = slotStatusList[i];
                if (slotStatus && data.GetSlotStatus(slotStatus.uid) == null)
                {
                    Destroy(slotStatus.gameObject);
                }
            }
        }
    
        private void SpawnNewPiece(Card card, bool promoted=false)
        {
            Vector3 position = GetPositionFromCoordinate(card.GetCoordinates());
            GameObject pieceGameObject = Instantiate(pieceBaseLink.GetPieceBase(card.GetPieceType()).piecePrefab, position, Quaternion.identity);
            pieceGameObject.SetActive(true);
            pieceGameObject.GetComponent<PieceOnBoard>().SetPiece(card);
            pieceGameObject.GetComponent<BoardCard>().SetCard(card);
            pieceGameObject.GetComponent<BoardCard>().promoted = promoted;
        }
    
        private void SpawnNewTrap(Card card)
        {
            Vector3 position = GetPositionFromCoordinate(card.GetCoordinates());
            GameObject trap = card.CardData.TrapOnGroundFX;
            if (trap == null)
            {
                trap = trapPrefab;
            }
        
            GameObject pieceGameObject = Instantiate(trap, position, Quaternion.identity);
            pieceGameObject.SetActive(true);
            pieceGameObject.GetComponent<BoardTrap>().SetCard(card);
        }
    
        private void SpawnNewSlotStatus(SlotStatus slotStatus)
        {
            Vector3 position = GetPositionFromCoordinate(slotStatus.slot.GetCoordinate());
            GameObject pieceGameObject = Instantiate(slotStatus.SlotStatusData.VFX, position, Quaternion.identity);
            pieceGameObject.SetActive(true);
            pieceGameObject.GetComponent<BoardSlotStatus>().SetSlotStatus(slotStatus);
        }
    
        private void MoveSlotStatus(SlotStatus slotStatus)
        {
            Vector3 position = GetPositionFromCoordinate(slotStatus.slot.GetCoordinate());
            BoardSlotStatus boardSlotStatus = BoardSlotStatus.Get(slotStatus.uid);
            if (boardSlotStatus != null)
            {
                boardSlotStatus.transform.position = position;
                boardSlotStatus.slot = slotStatus.slot;
            }
        }

        public Vector3 GetPositionFromCoordinate(Vector2S coordinate)
        {
            return _slots[coordinate.x, coordinate.y].transform.position;
        }

        public BoardSlot GetTileFromCoordinates(Vector2S coordinate)
        {
            Debug.Log(coordinate);
            return _slots[coordinate.x, coordinate.y];
        }

        private void CheckWin(Game data)
        {
            //--- End Game ----
            if (!_gameEnded && data.State == GameState.GameEnded)
            {
                _gameEnded = true;
                EndGame();
            }
        }
    
        private void EndGame()
        {
            StartCoroutine(EndGameRun());
        }

        private IEnumerator EndGameRun()
        {
            Game data = GameClient.GetGameData();
            if (data.winnerPlayer == -1)
            {
                yield return null;
            }
            Player winnerPlayer = data.GetPlayer(data.winnerPlayer);
            Player player = GameClient.Get().GetPlayer();
            bool win = winnerPlayer != null && player.playerID == winnerPlayer.playerID;
            bool tied = winnerPlayer == null;

            AudioTool.Get().FadeOutMusic("music");

            yield return new WaitForSeconds(1f);

            if (win)
                PlayerUI.Get(true).Kill();
            if (!win && !tied)
                PlayerUI.Get(false).Kill();

            if (win && AssetData.Get().win_fx != null)
                Instantiate(AssetData.Get().win_fx, Vector3.zero, Quaternion.identity);
            else if (tied && AssetData.Get().tied_fx != null)
                Instantiate(AssetData.Get().tied_fx, Vector3.zero, Quaternion.identity);
            else if (tied && AssetData.Get().lose_fx != null)
                Instantiate(AssetData.Get().lose_fx, Vector3.zero, Quaternion.identity);

            AudioClip audioToPlay = win ? AssetData.Get().win_audio : AssetData.Get().defeat_audio;
            AudioClip musicToPlay = win ? AssetData.Get().win_music : AssetData.Get().defeat_music;
            
            AudioTool.Get().PlaySFX("ending_sfx", audioToPlay);
            AudioTool.Get().PlayMusic("music", musicToPlay, 0.4f, false);

            yield return new WaitForSeconds(2f);

            EndGamePanel.Get().ShowWinner(data.winnerPlayer);
        }
    }
}
