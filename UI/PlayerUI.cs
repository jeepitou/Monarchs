using System.Collections;
using System.Collections.Generic;
using Monarchs.Client;
using Monarchs.Logic;
using Monarchs.Tools;
using Monarchs.UI;
using UnityEngine;
using UnityEngine.UI;
using TcgEngine.Client;
using TcgEngine;

namespace TcgEngine.UI
{
    /// <summary>
    /// Main player UI inside the GameUI, inside the game scene
    /// there is one for each player
    /// </summary>

    public class PlayerUI : MonoBehaviour
    {
        public bool is_opponent;
        public Text pname;
        public AvatarUI avatar;
        public ManaBar mana_bar;
        public Text hp_txt;
        public Text hp_max_txt;

        public Animator[] secrets;

        public GameObject dead_fx;
        public AudioClip dead_audio;
        public Sprite avatar_dead;

        private bool killed = false;

        private static List<PlayerUI> ui_list = new List<PlayerUI>();

        private void Awake()
        {
            ui_list.Add(this);
        }

        private void OnDestroy()
        {
            ui_list.Remove(this);
        }

        void Start()
        {
            pname.text = "";
            hp_txt.text = "";
            hp_max_txt.text = "";
            
            GameClient.Get().onRefreshAll += OnRefreshAll;
        }
        
        void OnRefreshAll()
        {
            if (!GameClient.Get().IsReady())
                return;

            Player player = GetPlayer();

            if (player != null)
            {
                pname.text = player.username;
                mana_bar.UpdateMana(player.playerMana);

                if (player.king != null)
                {
                    hp_txt.text = player.king.GetHP().ToString();
                    hp_max_txt.text = "/" + player.king.GetHPMax().ToString();
                }

                AvatarData adata = AvatarData.Get(player.avatar);
                if (avatar != null && adata != null && !killed)
                    avatar.SetAvatar(adata);
            }
        }
        
        public void Kill()
        {
            killed = true;
            avatar.SetImage(avatar_dead);
            AudioTool.Get().PlaySFX("fx", dead_audio);
            StartCoroutine(PlayDeadFX());
        }
        
        private IEnumerator PlayDeadFX()
        {
            FXResult result = new FXResult();
            yield return FXTool.DoFX(dead_fx, avatar.transform.position, result);
            
            // Add destruction after a delay
            if (result.fxObject != null)
                Destroy(result.fxObject, 5f);
        }

        private void OnClickAvatar(AvatarData avatar)
        {
            Game gdata = GameClient.GetGameData();
            int player_id = GameClient.Get().GetPlayerID();
            if (gdata.selector == SelectorType.SelectTarget && player_id == gdata.selectorPlayer)
            {
                GameClient.Get().SelectPlayer(GetPlayer());
            }
        }

        public Player GetPlayer()
        {
            int player_id = is_opponent ? GameClient.Get().GetOpponentPlayerID() : GameClient.Get().GetPlayerID();
            Game data = GameClient.GetGameData();
            return data.GetPlayer(player_id);
        }

        public static PlayerUI Get(bool opponent)
        {
            foreach (PlayerUI ui in ui_list)
            {
                if (ui.is_opponent == opponent)
                    return ui;
            }
            return null;
        }

    }
}