using System.Collections;
using System.Collections.Generic;
using Monarchs.Client;
using Monarchs.Logic;
using UnityEngine;
using UnityEngine.UI;
using TcgEngine.Client;
using TcgEngine;

namespace TcgEngine.UI
{
    /// <summary>
    /// Box that appears when using the SelectTarget ability target
    /// </summary>

    public class SelectTargetUI : UIPanel
    {
        public Text title;
        public Text desc;
        public GameObject skipButton;

        private static SelectTargetUI _instance;

        protected override void Awake()
        {
            _instance = this;
            base.Awake();
        }

        protected override void Update()
        {
            base.Update();

            Game game = GameClient.GetGameData();
            if (game != null && game.selector == SelectorType.None)
                Hide();
        }

        public void ShowMsg(string title, string desc, bool optional = false)
        {
            this.title.text = title;
            this.desc.text = desc;

            skipButton.SetActive(optional);
        }
        
        public void SkipAbilitySelection()
        {
            GameClient.Get().SkipSelection();
        }

        public void OnClickClose()
        {
            GameClient.Get().CancelSelection();
        }

        public static SelectTargetUI Get()
        {
            return _instance;
        }
    }
}