using System.Collections;
using System.Collections.Generic;
using Monarchs.Client;
using TcgEngine.Client;
using UnityEngine;

public class AiTurnButton : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void OnClick()
    {
        GameClient.Get().EndTurn();
    }
}
