using System;
using System.Collections;
using Monarchs.Client;
using Monarchs.Logic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

namespace TcgEngine.UI
{
    /// <summary>
    /// Main script for the main menu scene
    /// </summary>

    public class DemoMenu : MonoBehaviour
    {
        public AudioClip music;
        public AudioClip ambience;

        [Header("Player UI")]
        public TMP_Text username_txt;

        [Header("UI")]
        public Text version_text;

        public GameObject playPanel;
        public GameObject topPanel;
        public GameObject attemptingToRecconnectPanel;

        private bool starting = false;

        private static DemoMenu instance;

        void Awake()
        {
            instance = this;

            //Set default settings
            Application.targetFrameRate = 120;
            GameClient.GameSettings = GameSettings.Default;
        }

        private void Start()
        {
            //BlackPanel.Get().Show(true);
            //BlackPanel.Get().Hide();
            
            AudioTool.Get().PlayMusic("music", music);
            AudioTool.Get().PlaySFX("ambience", ambience, 0.5f, true, true);

            username_txt.text = "";
            version_text.text = "Version " + Application.version;

            if (Authenticator.Get().IsConnected())
                AfterLogin();
            else
                RefreshLogin();
            
            Messaging.ListenMsg("reconnected", OnReconnect);
        }

        private void OnDestroy()
        {
            Messaging?.UnListenMsg("matchmaking");
        }

        private void OnReconnect(ulong clientID, FastBufferReader reader)
        {
            reader.ReadNetworkSerializable(out MsgPlayerConnect msg);

            if (!string.IsNullOrWhiteSpace(msg.game_uid))
            {
                attemptingToRecconnectPanel.SetActive(true);
                StartCoroutine(Reconnect(msg.game_uid));
            }
        }
        
        private IEnumerator Reconnect(string gameUID)
        {
            yield return new WaitForSeconds(2f);
            attemptingToRecconnectPanel.SetActive(false);
            StartGame(gameUID);
        }

        void Update()
        {
            UserData udata = Authenticator.Get().UserData;

            bool matchmaking = GameClientMatchmaker.Get().IsMatchmaking();
            
            if (MatchmakingPanel.Get().IsVisible() != matchmaking)
                MatchmakingPanel.Get().SetVisible(matchmaking);

            if (Input.GetButtonDown("Cancel"))
            {
                OnClickSettings();
            }
        }

        private async void RefreshLogin()
        {
            bool success = await Authenticator.Get().RefreshLogin();
            if (success)
                AfterLogin();
            else
                SceneNav.GoTo("LoginMenu");
        }

        private void AfterLogin()
        {
            //BlackPanel.Get().Hide();
            playPanel.GetComponent<UIPanel>().Show();
            topPanel.GetComponent<UIPanel>().Show();

            //Events
            GameClientMatchmaker matchmaker = GameClientMatchmaker.Get();
            matchmaker.onMatchingComplete += OnMatchmakingDone;
            matchmaker.onMatchList += OnReceiveObserver;
            
            //UserData
            RefreshUserData();
        }

        public async void RefreshUserData()
        {
            UserData user = await Authenticator.Get().LoadUserData();
            if (user != null)
            {
                username_txt.text = user.username;
                
                AvatarData avatar = AvatarData.Get(user.avatar);
            }
        }

        private void OnMatchmakingDone(MatchmakingResult result)
        {
            if (result == null)
                return;

            if (result.success)
            {
                Debug.Log("Matchmaking found: " + result.success + " " + result.server_url + "/" + result.game_uid);
                StartGame(GameType.Multiplayer, result.game_uid, result.server_url);
            }
            else
            {
                MatchmakingPanel.Get().SetCount(result.players);
            }
        }

        private void OnReceiveObserver(MatchList list)
        {
            MatchListItem target = null;
            foreach (MatchListItem item in list.items)
            {
                if (item.username == GameClient.ObserveUser)
                    target = item;
            }

            if (target != null)
            {
                StartGame(GameType.Observer, target.game_uid, target.game_url);
            }
        }

        public void StartGame(GameType type, GameMode mode)
        {
            string uid = GameTool.GenerateRandomID();
            GameClient.GameSettings.game_type = type;
            GameClient.GameSettings.game_mode = mode;
            StartGame(uid); 
        }

        public void StartGame(GameType type, string game_uid, string server_url = "")
        {
            GameClient.GameSettings.game_type = type;
            StartGame(game_uid, server_url);
        }

        public void StartGame(string game_uid, string server_url = "")
        {
            if (!starting)
            {
                starting = true;
                GameClient.GameSettings.server_url = server_url; //Empty server_url will use the default one in NetworkData
                GameClient.GameSettings.game_uid = game_uid;
                GameClient.GameSettings.scene = GameplayData.Get().GetRandomArena();
                GameClientMatchmaker.Get().Disconnect();
                FadeToScene(GameClient.GameSettings.GetScene());
            }
        }

        public void StartObserve(string user)
        {
            GameClient.ObserveUser = user;
            GameClientMatchmaker.Get().StopMatchmaking();
            GameClientMatchmaker.Get().RefreshMatchList(user);
        }

        public void StartMatchmaking(DeckData deckData)
        {
            PlayerDeckSettings playerDeck = new PlayerDeckSettings(deckData);
            if (playerDeck != null)
            {
                GameClient.GameSettings.game_type = GameType.Multiplayer;
                GameClient.GameSettings.game_mode = GameMode.Casual;
                GameClient.PlayerSettings.deck = playerDeck;
                GameClientMatchmaker.Get().StartMatchmaking("", GameClient.GameSettings.nb_players);
            }
        }
        
        public void FadeToScene(string scene, bool fadeoutMusic = true)
        {
            StartCoroutine(FadeToRun(scene, fadeoutMusic));
        }

        private IEnumerator FadeToRun(string scene, bool fadeoutMusic = true)
        {
            //BlackPanel.Get().Show();
            playPanel.GetComponent<UIPanel>().Hide();
            topPanel.GetComponent<UIPanel>().Hide();
            if (fadeoutMusic)
            {
                AudioTool.Get().FadeOutMusic("music");
            }
            yield return new WaitForSeconds(0.5f);
            SceneNav.GoTo(scene);
        }

        public void OnClickLogout()
        {
            
            TcgNetwork.Get().Disconnect();
            Authenticator.Get().Logout();
            FadeToScene("LoginMenu", false);
        }

        public void OnClickQuit()
        {
            Application.Quit();
        }
        
        public void OnClickSettings()
        {
            SettingsPanel.Get().Toggle();
        }

        public static DemoMenu Get()
        {
            return instance;
        }
        
        public NetworkMessaging Messaging { get { return TcgNetwork.Get()?.Messaging; } }
    }
}
