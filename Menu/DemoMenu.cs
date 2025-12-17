using System.Collections;
using System.Threading.Tasks;
using Monarchs.Api;
using Monarchs.Client;
using Monarchs.Logic;
using Monarchs.Tools;
using TcgEngine;
using TcgEngine.UI;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Monarchs.Menu
{
    /// <summary>
    /// Main script for the main menu scene
    /// </summary>

    public class DemoMenu : MonoBehaviour
    {
        public AudioClip music;
        public AudioClip ambience;

        [Header("Player UI")]
        public TMP_Text usernameText;

        [Header("UI")]
        public Text versionText;

        public GameObject playPanel;
        public GameObject topPanel;
        public GameObject attemptingToRecconnectPanel;
        
        public TMP_InputField customDeckImportField;
        public TMP_Text customDeckInfoText;
        public GameObject queueWithCustomDeckButton;

        private UserDeckData _importedDeck;
        private bool _starting;

        private static DemoMenu _instance;

        void Awake()
        {
            _instance = this;

            //Set default settings
            Application.targetFrameRate = 120;
            GameClient.GameSettings = GameSettings.Default;
        }

        private void Start()
        {
            AudioTool.Get().PlayMusic("music", music);
            AudioTool.Get().PlaySFX("ambience", ambience, 0.5f, true, true);

            usernameText.text = "";
            versionText.text = "Version " + Application.version;

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
            playPanel.GetComponent<UIPanel>().Show();
            topPanel.GetComponent<UIPanel>().Show();

            //Events
            GameClientMatchmaker matchmaker = GameClientMatchmaker.Get();
            matchmaker.onMatchingComplete += OnMatchmakingDone;
            matchmaker.onMatchList += OnReceiveObserver;
            
            //UserData
            RefreshUserData();
        }

        public void CreateDeck()
        {
            CreateDeckWithExportCode(customDeckImportField.text);
        }

        public async void CreateDeckWithExportCode(string exportCode)
        {
            UserDeckData deck = DeckImporter.ImportDeck(exportCode, "Imported Deck");

            bool success = false;
            if (deck != null)
            {
                success = await SaveDeckAPI(Authenticator.Get().UserData, deck);
            }
            
            if (success)
            {
                await Authenticator.Get().LoadUserData();
                _importedDeck = Authenticator.Get().UserData.GetDeck(deck.tid);
                customDeckInfoText.color = Color.black;
                customDeckInfoText.text = $"Monarch: {_importedDeck.monarch}\nChampion: {_importedDeck.champion}\nCards: {string.Join(", ", _importedDeck.cards)}";
                queueWithCustomDeckButton.SetActive(true);
            }
            else
            {
                customDeckInfoText.color = Color.red;
                customDeckInfoText.text = "Failed to create deck from export code.";
                queueWithCustomDeckButton.SetActive(false);
            }
        }
        
        public void QueueWithCustomDeck()
        {
            if (_importedDeck != null)
            {
                PlayerDeckSettings playerDeck = new PlayerDeckSettings(_importedDeck);
                StartMatchmaking(playerDeck);
            }
        }
        
        private async Task<bool> SaveDeckAPI(UserData udata, UserDeckData udeck)
        {
            string url = ApiClient.ServerURL + "/users/deck/" + udeck.tid;
            string jdata = ApiTool.ToJson(udeck);
            WebResponse res = await ApiClient.Get().SendPostRequest(url, jdata);
            UserDeckData[] decks = ApiTool.JsonToArray<UserDeckData>(res.data);

            if (res.success && decks != null)
            {
                Debug.Log("Deck saved: " + udeck.tid);
                udata.decks = decks;
                await Authenticator.Get().SaveUserData();
            }
            else
            {
                Debug.LogError("Failed to save deck: " + res.error);
            }
            
            return res.success;
        }
        
        public async void RefreshUserData()
        {
            UserData user = await Authenticator.Get().LoadUserData();
            if (user != null)
            {
                usernameText.text = user.username;
                
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

        public void StartGame(GameType type, string gameUID, string serverURL = "")
        {
            GameClient.GameSettings.game_type = type;
            StartGame(gameUID, serverURL);
        }

        public void StartGame(string gameUID, string serverURL = "")
        {
            if (!_starting)
            {
                _starting = true;
                GameClient.GameSettings.server_url = serverURL; //Empty server_url will use the default one in NetworkData
                GameClient.GameSettings.game_uid = gameUID;
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
            
            GameClient.GameSettings.game_type = GameType.Multiplayer;
            GameClient.GameSettings.game_mode = GameMode.Casual;
            GameClient.PlayerSettings.deck = playerDeck;
            GameClientMatchmaker.Get().StartMatchmaking("", GameClient.GameSettings.nb_players);
        }
        
        public void StartMatchmaking(PlayerDeckSettings playerDeck)
        {
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
            return _instance;
        }
        
        public NetworkMessaging Messaging => TcgNetwork.Get()?.Messaging;
    }
}
