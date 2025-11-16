using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TcgEngine.UI
{
    /// <summary>
    /// Main script for the login menu scene
    /// </summary>

    public class LoginMenu : MonoBehaviour
    {
        [Header("Login")]
        public UIPanel login_panel;
        public InputField login_user;
        public InputField login_password;
        public Button login_button;
        public GameObject login_bottom;
        public Text error_msg;

        [Header("Register")]
        public UIPanel register_panel;
        public InputField register_username;
        public InputField register_email;
        public InputField register_password;
        public InputField register_password_confirm;
        public Button register_button;

        [Header("Other")]
        public GameObject test_area;

        public GameObject loggingInfoPannel;
        public TMP_Text loggingInfoText;

        [Header("Music")]
        public AudioClip musicIntro;
        public AudioClip music;

        private UnityAction<bool> connect_callback;
        private bool clicked = false;

        private static LoginMenu instance;

        void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            TcgNetwork.Get().onConnect += OnConnect;
            
            // Get the current DSP time
            double dspTime = AudioSettings.dspTime;
            AudioTool.Get().PlayMusic("music_intro", musicIntro, 0.3F, false, dspTime);
            double musicIntroEndTime = dspTime + (double)musicIntro.samples / musicIntro.frequency;
            
            AudioTool.Get().PlayMusic("music", music, 0.3F, true, musicIntroEndTime);
            //BlackPanel.Get().Show(true);
            error_msg.text = "";
            test_area.SetActive(Authenticator.Get().IsTest());

            string user = PlayerPrefs.GetString("tcg_last_user", "");
            login_user.text = user;

            if (Authenticator.Get().IsTest())
            {
                login_password.gameObject.SetActive(false);
                login_bottom.SetActive(false);
            }
            else if (!string.IsNullOrEmpty(user))
            {
                SelectField(login_password);
            }

            RefreshLogin();
        }
        
        private void OnDestroy()
        {
            TcgNetwork.Get().onConnect -= OnConnect;
        }

        void Update()
        {
            login_button.interactable = !clicked && !string.IsNullOrWhiteSpace(login_user.text);
            register_button.interactable = !clicked && !string.IsNullOrWhiteSpace(register_username.text) && !string.IsNullOrWhiteSpace(register_email.text)
                && !string.IsNullOrWhiteSpace(register_password.text) && register_password.text == register_password_confirm.text;

            if (login_panel.IsVisible())
            {
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    if (login_user.isFocused)
                        SelectField(login_password);
                    else
                        SelectField(login_user);
                }

                if (Input.GetKeyDown(KeyCode.Return))
                {
                    if (login_button.interactable)
                        OnClickLogin();
                }
            }

            if (register_panel.IsVisible())
            {
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    if (register_username.isFocused)
                        SelectField(register_email);
                    else if (register_email.isFocused)
                        SelectField(register_password);
                    else if (register_password.isFocused)
                        SelectField(register_password_confirm);
                    else
                        SelectField(register_username);
                }

                if (Input.GetKeyDown(KeyCode.Return))
                {
                    if (register_button.interactable)
                        OnClickRegister();
                }
            }

            if (Input.GetButtonDown("Cancel"))
            {
                OnClickSettings();
            }
        }

        public void OnClickSettings()
        {
            SettingsPanel.Get().Toggle();
        }

        private async void RefreshLogin()
        {
            bool success = await Authenticator.Get().RefreshLogin();
            if (success)
            {
                SceneNav.GoTo("DemoMenu");
            }
            else
            {
                login_panel.Show();
                //BlackPanel.Get().Hide();
            }
        }

        private async void Login(string user, string password)
        {
            loggingInfoPannel.SetActive(true);
            loggingInfoText.text = "Validating credentials...";
            clicked = true;
            error_msg.text = "";
            login_panel.Hide();
            

            bool success = await Authenticator.Get().Login(user, password);
            if (success)
            {
                loggingInfoText.text = "Connecting to server...";
                PlayerPrefs.SetString("tcg_last_user", login_user.text);
                //FadeToScene("Menu");

                if (!TcgNetwork.Get().IsConnected())
                {
                    Connect(NetworkData.Get().url, NetworkData.Get().port, connected =>
                    {
                        if (connected)
                        {
                            SendPlayerInfoToServer();
                            SceneManager.LoadSceneAsync("DemoMenu", LoadSceneMode.Single);
                        }
                        else
                        {
                            loggingInfoPannel.SetActive(false);
                            login_panel.Show();
                            clicked = false;
                            error_msg.text = Authenticator.Get().GetError();
                        }
                    });
                }
            }
            else
            {
                loggingInfoPannel.SetActive(false);
                login_panel.Show();
                clicked = false;
                error_msg.text = Authenticator.Get().GetError();
            }
        }
        
        public virtual async void SendPlayerInfoToServer()
        {
            try
            {
                await Task.Yield(); //Wait for initialization to finish

                if (!TcgNetwork.Get().IsActive())
                    return; //Not connected to server

                MsgPlayerConnect msgPlayerConnect = new MsgPlayerConnect
                {
                    user_id = Authenticator.Get().UserID,
                    username = Authenticator.Get().Username,
                    game_uid = "",
                    
                };

                Messaging.SendObject("send_player_info", ServerID, msgPlayerConnect, NetworkDelivery.Reliable);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error connecting to game: {e}");
            }
        }
        
        private void OnConnect()
        {
            Debug.Log("Connected to server!");
            connect_callback?.Invoke(true);
            connect_callback = null;
        }
        
        public void Connect(string url, ushort port, UnityAction<bool> callback=null)
        {
            //Must be logged in to API to connect
            if(!Authenticator.Get().IsSignedIn())
            {
                callback?.Invoke(false);
                return;
            }

            //Check if already connected
            if (IsConnected() || IsConnecting())
            {
                callback?.Invoke(IsConnected());
                return;
            }

            connect_callback = callback;
            TcgNetwork.Get().StartClient(url, port);
        }
        
        public bool IsConnected()
        {
            return TcgNetwork.Get().IsConnected();
        }

        public bool IsConnecting()
        {
            return TcgNetwork.Get().IsConnecting();
        }

        private async void Register(string email, string user, string password)
        {
            clicked = true;
            error_msg.text = "";

            bool success = await Authenticator.Get().Register(register_email.text, register_username.text, register_password.text);
            if (success)
            {
                login_user.text = register_username.text;
                login_password.text = register_password.text;
                login_panel.Show();
                register_panel.Hide();
            }
            else
            {
                error_msg.text = Authenticator.Get().GetError();
            }
            clicked = false;
        }

        public void OnClickLogin()
        {
            if (string.IsNullOrWhiteSpace(login_user.text))
                return;
            if (clicked)
                return;

            Login(login_user.text, login_password.text);
        }

        public void OnClickRegister()
        {
            if (string.IsNullOrWhiteSpace(register_username.text))
                return;
            if (string.IsNullOrWhiteSpace(register_email.text))
                return;

            if (register_password.text != register_password_confirm.text)
                return;

            if (clicked)
                return;

            Register(register_email.text, register_username.text, register_password.text);
        }

        public void OnClickSwitchLogin()
        {
            login_panel.Show();
            register_panel.Hide();
            login_user.text = "";
            login_password.text = "";
            error_msg.text = "";
            SelectField(login_user);
        }

        public void OnClickSwitchRegister()
        {
            login_panel.Hide();
            register_panel.Show();
            error_msg.text = "";
            SelectField(register_username);
        }

        public void OnClickSwitchReset()
        {
            RecoveryPanel.Get().Show();
        }

        public void OnClickGo()
        {
            FadeToScene("DemoMenu");
        }

        public void OnClickQuit()
        {
            Debug.Log("Quitting application");
            Application.Quit();
        }

        private void SelectField(InputField field)
        {
            if (!GameTool.IsMobile())
                field.Select();
        }

        public void FadeToScene(string scene)
        {
            StartCoroutine(FadeToRun(scene));
        }

        private IEnumerator FadeToRun(string scene)
        {
            //BlackPanel.Get().Show();
            AudioTool.Get().FadeOutMusic("music");
            yield return new WaitForSeconds(1f);
            SceneNav.GoTo(scene);
        }

        public static LoginMenu Get()
        {
            return instance;
        }
        
        public NetworkMessaging Messaging { get { return TcgNetwork.Get().Messaging; } }
        public ulong ServerID { get { return TcgNetwork.Get().ServerID; } }
    }
}