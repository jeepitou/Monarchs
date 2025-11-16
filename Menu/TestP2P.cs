using System.Collections;
using System.Collections.Generic;
using Monarchs.Client;
using Monarchs.Logic;
using UnityEngine;
using UnityEngine.UI;
using TcgEngine.Client;
using TcgEngine.UI;

namespace TcgEngine
{
    /// <summary>
    /// Main script for the P2P menu scene
    /// </summary>

    public class TestP2P : MonoBehaviour
    {
        public UIPanel deck_panel;
        public UIPanel join_panel;
        public InputField username;
        public InputField password;
        public DeckSelector deck_selector;
        public DeckDisplay deck_preview;
        public InputField join_ip;
        public Text error;

        private bool starting = false;

        void Start()
        {
            GameClient.GameSettings = GameSettings.Default;
            GameClient.PlayerSettings = PlayerSettings.Default;
            GameClient.GameSettings.game_uid = "test_p2p";

            deck_selector.onChange += OnChangeDeck;
            error.text = "";
        }

        void Update()
        {

        }

        private async void Login()
        {
            error.text = "";
            bool success = await Authenticator.Get().Login(username.text, password.text);
            if (success)
            {
                UserData udata = await Authenticator.Get().LoadUserData();
                GameClient.PlayerSettings.avatar = udata.GetAvatar();
                GameClient.PlayerSettings.cardback = udata.GetCardback();
                deck_panel.Show();
                RefreshDeckList();
            }
            else
            {
                error.text = Authenticator.Get().GetError();
            }
        }

        public void StartGame()
        {
            if (!starting)
            {
                starting = true;
                SceneNav.GoTo(GameClient.GameSettings.GetScene());
            }
        }

        public void RefreshDeckList()
        {
            deck_selector.RefreshDeckList();
            deck_selector.SelectDeck(GameClient.PlayerSettings.deck.id);
            RefreshDeck(deck_selector.GetDeckID());
        }

        private void RefreshDeck(string tid)
        {
            UserData user = Authenticator.Get().UserData;
            UserDeckData udeck = user.GetDeck(tid);
            DeckData ddeck = DeckData.Get(tid);
            if (udeck != null)
                deck_preview.SetDeck(udeck);
            else if (ddeck != null)
                deck_preview.SetDeck(ddeck);
            else
                deck_preview.Clear();
        }

        public void OnChangeDeck(string tid)
        {
            GameClient.PlayerSettings.deck.id = tid;
            PlayerPrefs.SetString("tcg_deck", tid);
            RefreshDeck(tid);
        }

        public void OnClickLogin()
        {
            if (username.text.Length == 0)
                return;

            Login();
        }

        public void OnClickHost()
        {
            GameClient.GameSettings.game_type = GameType.HostP2P;
            GameClient.GameSettings.server_url = "127.0.0.1";
            GameClient.PlayerSettings.deck.id = deck_selector.GetDeckID();
            StartGame();
        }

        public void OnClickGoJoin()
        {
            GameClient.PlayerSettings.deck.id = deck_selector.GetDeckID();
            deck_panel.Hide();
            join_panel.Show();
        }

        public void OnClickJoin()
        {
            if (join_ip.text.Length == 0)
                return;

            GameClient.GameSettings.game_type = GameType.Multiplayer;
            GameClient.GameSettings.server_url = join_ip.text;
            StartGame();
        }

        public void OnClickBack()
        {
            if (join_panel.IsVisible())
            {
                join_panel.Hide();
                deck_panel.Show();
            }
            else
            {
                deck_panel.Hide();
            }
        }
    }
}
