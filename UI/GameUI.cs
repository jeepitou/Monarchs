using System.Collections;
using System.Collections.Generic;
using Monarchs;
using Monarchs.Ability;
using Monarchs.Client;
using Monarchs.Logic;
using UnityEngine;
using UnityEngine.UI;
using TcgEngine.Client;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

namespace TcgEngine.UI
{
    /// <summary>
    /// Main UI script for all the game scene UI
    /// </summary>

    public class GameUI : MonoBehaviour
    {
        public Canvas game_canvas;
        public Canvas panel_canvas;
        public Canvas top_canvas;
        public UIPanel menu_panel;
        public UIPanel mana_panel;
        public UIPanel disconnect_panel;
        public Text quit_btn;

        [Header("Turn Area")]
        public Text round_count;
        public Text turn_timer;
        public Button end_turn_button;
        public Button end_AI_turn_button;
        public Animator timeout_animator;
        public AudioClip timeout_audio;
        public AudioClip your_turn_start_audio;
        public InitiativeManagerUI initiativeManagerUI;

        public GameObject mulligan;
        
        public GameObject trapPlayedIndicator;

        private float selector_timer = 0f;
        private int prev_time_val = 0;

        private static GameUI _instance;

        void Awake()
        {
            _instance = this;
            game_canvas.worldCamera = Camera.main;
            panel_canvas.worldCamera = Camera.main;
            top_canvas.worldCamera = Camera.main;
        }

        private void Start()
        {
            GameClient.Get().onGameStart += OnGameStart;
            GameClient.Get().onNewTurn += OnNextTurn;
            GameClient.Get().onCardPlayed += OnCardPlayed;
            GameClient.Get().onPlayerDisconnected += OnPlayerDisconnected;
            GameClient.Get().onPlayerReconnected += OnPlayerReconnected;
            LoadPanel.Get().Show(true);
            BlackPanel.Get().Show(true);
            BlackPanel.Get().Hide();
            
            if (quit_btn != null)
                quit_btn.text = GameClient.GameSettings.IsOnlinePlayer() ? "Resign" : "Quit";
        }

        private void OnPlayerDisconnected(int secondsUntilWin)
        {
            disconnect_panel.Show();
            disconnect_panel.GetComponentInChildren<GameTimer>().SetTimer(0, 0, secondsUntilWin);
        }
        
        private void OnPlayerReconnected()
        {
            disconnect_panel.Hide();
        }

        private void OnCardPlayed(Card card, Slot slot)
        {
            if (card?.CardData.cardType == CardType.Trap)
            {
                if (card?.playerID != GameClient.Get().GetPlayerID())
                {
                    trapPlayedIndicator.SetActive(true);
                    StartCoroutine(DisableTrapIndicator());
                }
                
                AudioTool.Get().PlaySFX("trap", AssetData.Get().trap_played_audio);
            }
        }

        private IEnumerator DisableTrapIndicator()
        {
            yield return new WaitForSeconds(4f);
            trapPlayedIndicator.SetActive(false);
        }

        void Update()
        {
            Game data = GameClient.GetGameData();
			bool is_connecting = data == null || data.State == GameState.Connecting;
            bool connection_lost = !is_connecting && !GameClient.Get().IsReady();
            ConnectionPanel.Get().SetVisible(connection_lost);

            //Menu
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                menu_panel.Toggle();
                SettingsPanel.Get().Hide();
            }
                

            if (!GameClient.Get().IsReady())
                return;

            if (!data.HasAIPlayer())
            {
                end_AI_turn_button.gameObject.SetActive(false);
            }
            
            bool yourturn = GameClient.Get().IsYourTurn();
            int player_id = GameClient.Get().GetPlayerID();

            LoadPanel.Get().SetVisible(is_connecting && !data.HasStarted());
            //end_turn_button.interactable = yourturn && data.state == GameState.Play && !initiativeManagerUI.isDoingNextTurnAnimation;
            SetNextTurnButton(yourturn && data.State == GameState.Play, data.HasAIPlayer());
            //end_turn_button.gameObject.SetActive(yourturn && data.state == GameState.Play && !initiativeManagerUI.isDoingNextTurnAnimation);
            end_AI_turn_button.interactable = !yourturn && data.State == GameState.Play;

            //Timer
            round_count.text = "Round " + data.roundCount.ToString();
            turn_timer.enabled = data.turnTimer > 0f;
            turn_timer.text = Mathf.RoundToInt(data.turnTimer).ToString();
            turn_timer.enabled = data.turnTimer < 999f;

            //Simulate timer
            if (data.State == GameState.Play && data.turnTimer > 0f)
                data.turnTimer -= Time.deltaTime;

            //Timer warning
            if (data.State == GameState.Play)
            {
                int val = Mathf.RoundToInt(data.turnTimer);
                int tick_val = 10;
                if (val < prev_time_val && val <= tick_val)
                    PulseFX();
                prev_time_val = val;
            }
            
            //Multiple Card target
            bool show_multiple_msg = data.selector == SelectorType.SelectMultipleTarget && data.selectorPlayer == player_id;
            if (show_multiple_msg)
            {
                AbilityData iability = AbilityData.Get(data.selectorAbilityID);
                Card playedCard = data.GetCard(data.selectorCastedCardUID);
                if (iability.id == "cohort_size")
                {
                    SelectTargetUI.Get().ShowMsg(playedCard.CardData.name+"'s cohort", "Select where to summon the next cohort member");
                }
                else if (iability.targetSpecifications.Length > data.selectorTargets.Count)
                {
                    SelectTargetUI.Get().ShowMsg(iability.title, iability.targetSpecifications[data.selectorTargets.Count].promptText, iability.targetSpecifications[data.selectorTargets.Count].optional);
                }
            }

            //Card target
            bool show_msg = data.selector == SelectorType.SelectTarget && data.selectorPlayer == player_id;
            if (show_msg)
            {
                AbilityData iability = AbilityData.Get(data.selectorAbilityID);
                SelectTargetUI.Get().ShowMsg(iability.title, iability.desc);
            }
            
            //Caster selector
            bool show_caster_msg = data.selector == SelectorType.SelectCaster && data.selectorPlayer == player_id;
            if (show_caster_msg)
            {
                AbilityData iability = AbilityData.Get(data.selectorAbilityID);
                Card card = GameClient.GetGameData().GetCard(data.selectorCardUID);
                string title = "";
                if (iability != null)
                {
                    title = iability.title;
                }
                else if (card != null)
                {
                    title = card.cardID;
                }
   
                SelectTargetUI.Get().ShowMsg(title, "Choose the piece to do this ability");
            }
            
            //RangerAttacker selector
            bool show_rangeAttacker_msg = data.selector == SelectorType.SelectRangeAttacker && data.selectorPlayer == player_id;
            
            if (show_rangeAttacker_msg)
            {
                string title = "";
                SelectTargetUI.Get().ShowMsg(title, "Choose the piece to do this attack");
            }
            
            bool mana_selector = data.selector == SelectorType.SelectManaTypeToGenerate && data.selectorPlayer == player_id;
            bool manaToSpendSelector = data.selector == SelectorType.SelectManaTypeToSpend && data.selectorPlayer == player_id;

            mana_panel.SetVisible(mana_selector || manaToSpendSelector);
            SelectTargetUI.Get().SetVisible(show_rangeAttacker_msg || show_caster_msg || show_msg || show_multiple_msg);

            if (!CardSelector.Get().IsVisible())
                selector_timer += Time.deltaTime;

            //Card Selector
            bool show_selector = data.selector == SelectorType.SelectorCard && data.selectorPlayer == player_id && selector_timer > 1f;
            if (show_selector && !CardSelector.Get().IsVisible())
            {
                AbilityData iability = AbilityData.Get(data.selectorAbilityID);
                Card caster = data.GetCard(data.selectorCasterUID);
                selector_timer = 0f;

                if (iability != null)
                    CardSelector.Get().Show(iability, caster);
            }

            //Choice selector
            bool show_choice_selector = data.selector == SelectorType.SelectorChoice && data.selectorPlayer == player_id;
            if (show_choice_selector && !ChoiceSelector.Get().IsVisible() && selector_timer > 1f)
            {
                AbilityData iability = AbilityData.Get(data.selectorAbilityID);
                ChoiceSelector.Get().Show(iability);
                selector_timer = 0f;
            }

            //Hide
            if (!show_selector && CardSelector.Get().IsAbility())
                CardSelector.Get().Hide();
            if (!show_choice_selector && ChoiceSelector.Get().IsVisible())
                ChoiceSelector.Get().Hide();

        }

        

        private void PulseFX()
        {
            timeout_animator?.SetTrigger("pulse");
            AudioTool.Get().PlaySFX("time", timeout_audio, 1f);
        }

        private void OnGameStart()
        {
            Game data = GameClient.GetGameData();
            int player_id = GameClient.Get().GetPlayerID();
            Player player = data.GetPlayer(player_id);
        }
        
        private void SetNextTurnButton(bool active, bool hasAiPlayer)
        {
            if (active == end_turn_button.gameObject.activeSelf)
            {
                return;
            }
            
            end_turn_button.gameObject.SetActive(active);
            if (hasAiPlayer)
            {
                end_AI_turn_button.gameObject.SetActive(!active);
            }

            if (active)
            {
                AudioTool.Get().PlaySFX("ui", your_turn_start_audio);
            }
        }

        private void OnNextTurn(Card card)
        {
            CardSelector.Get().Hide();
            SelectTargetUI.Get().Hide();
        }

        public void OnClickDeck()
        {
            //GameClient.Get().DrawCard();
        }

        public void OnClickNextTurn()
        {
            GameClient.Get().EndTurn();
        }

        public void OnClickRestart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void OnClickSettings()
        {
            SettingsPanel.Get().Toggle();
        }

        public void OnClickBack()
        {
            menu_panel.Hide();
        }

        public void OnClickQuit()
        {
            bool online = GameClient.GameSettings.IsOnlinePlayer();
            bool ended = GameClient.Get().HasEnded();
            if (online && !ended)
                GameClient.Get().Resign();
            else
                StartCoroutine(QuitRoutine("DemoMenu"));
            menu_panel.Hide();
        }

        private IEnumerator QuitRoutine(string scene)
        {
            BlackPanel.Get().Show();
            AudioTool.Get().FadeOutMusic("music");
            AudioTool.Get().FadeOutSFX("ambience");
            AudioTool.Get().FadeOutSFX("ending_sfx");

            yield return new WaitForSeconds(1f);

            GameClient.Get().Disconnect();
            SceneNav.GoTo(scene);
        }

        public void OnClickSwapObserve()
        {
            int other = GameClient.Get().GetPlayerID() == 0 ? 1 : 0;
            GameClient.Get().SetObserverMode(other);
        }

        public static bool IsUIOpened()
        {
            return CardSelector.Get().IsVisible() || EndGamePanel.Get().IsVisible() || ChoiceSelector.Get().IsVisible() || _instance.mulligan != null;
        }

        public static bool IsOverUI()
        {
            //return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            return results.Count > 0;
        }

        public static bool IsOverUILayer(string sorting_layer)
        {
            return IsOverUILayer(SortingLayer.NameToID(sorting_layer));
        }

        public static bool IsOverUILayer(int sorting_layer)
        {
            //return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            int count = 0;
            foreach (RaycastResult result in results)
            {
                if (result.sortingLayer == sorting_layer)
                    count++;
            }
            return count > 0;
        }

        public static Vector2 ScreenToRectPos(Canvas canvas, RectTransform rect, Vector2 screen_pos)
        {
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay && canvas.worldCamera != null)
            {
                Vector2 anchor_pos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screen_pos, canvas.worldCamera, out anchor_pos);
                return anchor_pos;
            }
            else
            {
                Vector2 anchor_pos = screen_pos - new Vector2(rect.position.x, rect.position.y);
                anchor_pos = new Vector2(anchor_pos.x / rect.lossyScale.x, anchor_pos.y / rect.lossyScale.y);
                return anchor_pos;
            }
        }

        public static Vector3 MouseToWorld(Vector2 mouse_pos)
        {
            Camera cam = GameCamera.Get() != null ? GameCamera.GetCamera() : Camera.main;
            Vector3 wpos = cam.ScreenToWorldPoint(mouse_pos);
            wpos.z = 0f;
            return wpos;
        }

        public static string FormatNumber(int value)
        {
            return string.Format("{0:#,0}", value);
        }

        public static GameUI Get()
        {
            return _instance;
        }
    }
}
