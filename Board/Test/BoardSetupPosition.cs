using System.Collections;
using System.Collections.Generic;
using Monarchs;
using TcgEngine;
using UnityEngine;

[CreateAssetMenu(fileName = "BoardSetupPosition", menuName = "ChessTCG/Test/BoardSetupPosition")]
public class BoardSetupPosition : ScriptableObject
{
    public PieceOnSquare[] piecesOnSquare;

    [System.Serializable]
    public struct PieceOnSquare
    {
        public int x;
        public int y;
        public CardData piece;
        public bool firstPlayer;
    }
}
