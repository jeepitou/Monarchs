using System.Collections.Generic;
using Monarchs.Client;
using Monarchs.Logic;
using UnityEngine;
using UnityEngine.UI;

namespace Monarchs
{
    
    public class HistoryManagerUI : MonoBehaviour
    {
        public GameObject historyLinePrefab;
        List<HistoryLineUI> historyLines = new List<HistoryLineUI>();
        int historyLineCounter = 0;
        
        void Start()
        {
            GameClient.Get().onRefreshAll += RefreshHistory;
        }

        private void RefreshHistory()
        {
            var history = GameClient.GetGameData().history.historyList;
            
            for (int i = historyLineCounter; i<history.Count; i++)
            {
                if (history[i].type == GameAction.Move || 
                    history[i].type == GameAction.Attack || 
                    history[i].type == GameAction.RangeAttack || 
                    history[i].type == GameAction.CardPlayed || 
                    history[i].type == GameAction.CastAbility)
                {
                   
                    var line = Instantiate(historyLinePrefab, transform).GetComponent<HistoryLineUI>();
                    line.transform.SetSiblingIndex(0);
                    line.SetText(history[i]);
                    line.index = i;
                    historyLines.Insert(0,line);
                }

                historyLineCounter++;
            }
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }
    }
}
