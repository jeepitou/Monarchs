using System.Collections;
using System.Collections.Generic;
using TcgEngine;
using UnityEngine;
using UnityEngine.UI;

public class HandCardPossibleCasters : MonoBehaviour
{
    public PieceTypeGameObjectLink[] pieceTabs;

    // Update is called once per frame
    public void UpdateCurrentType(PieceType type)
    {
        foreach (var tab in pieceTabs)
        {
            if (type.HasFlag(tab.pieceType))
            {
                tab.gameObject.transform.SetSiblingIndex(tab.gameObject.transform.parent.childCount-1);
                tab.gameObject.GetComponent<Image>().color = new Color(0, 0, 0, 1);
                tab.gameObject.GetComponentInChildren<Image>().color = new Color(0, 0, 0, 1);
            }
            else
            {
                tab.gameObject.GetComponent<Image>().color = new Color(0, 0, 0, 0.6f);
                tab.gameObject.GetComponentInChildren<Image>().color = new Color(0, 0, 0, 0.6f);
            }
        }
    }

    public void SetPossibleCasters(PieceType possibleCaster)
    {
        foreach (var tab in pieceTabs)
        {
            tab.gameObject.SetActive(possibleCaster.HasFlag(tab.pieceType));
        }
    }
    
    [System.Serializable]
    public struct PieceTypeGameObjectLink
    {
        public PieceType pieceType;
        public GameObject gameObject;
    }
}
