using Monarchs.Client;
using TMPro;
using UnityEngine;

namespace Monarchs
{
    public class BoardCoordinatesManager : MonoBehaviour
    {
        public GameObject rows;

        public GameObject columns;
        
        // Start is called before the first frame update
        void Start()
        {
            GameClient.Get().onGameStart += UpdateCoordinates;

        }

        void UpdateCoordinates()
        {
            if (!GameClient.Get().IsFirstPlayer())
            {
                InvertCoordinatesForSecondPlayer();
            }
        }

        void InvertCoordinatesForSecondPlayer()
        {
            char[] colums_chars = new []{'H', 'G', 'F', 'E', 'D', 'C', 'B', 'A'};
            char[] rows_chars = new []{'8', '7', '6', '5', '4', '3', '2', '1'};
            
            for (int i = 0; i < 8; i++)
            {
                columns.transform.GetChild(i).GetComponent<TMP_Text>().text = colums_chars[i].ToString();
                rows.transform.GetChild(i).GetComponent<TMP_Text>().text = rows_chars[i].ToString();
            }
        }
    }
}
