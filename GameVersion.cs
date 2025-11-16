using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Monarchs
{
    public class GameVersion : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            GetComponent<TMP_Text>().text = "Game Version: " + Application.version;
        }
    }
}
