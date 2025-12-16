using Monarchs;
using Monarchs.Board;
using Monarchs.Client;
using Monarchs.Logic;
using TcgEngine;
using TcgEngine.Client;
using UnityEngine;

// CEST UNE CLASSE JUSTE POUR TESTER, Sa spawn la pieceToSpawn quand on clique sur une case vide
public class PieceSpawnerTest : MonoBehaviour
{
    public CardData pieceToSpawn;
    
    // Start is called before the first frame update
    void Start()
    {
        BoardInputManager.Instance.OnClick += OnClick;
    }

    public void OnClick(BoardSlot slot, Card card = null)
    {
        if (card == null && slot != null)
        {
            Vector3 position = BoardManager.Instance.GetPositionFromCoordinate(slot.GetCoordinate());
            GameObject g = Instantiate(pieceToSpawn.pieceModels[0].blackPrefab, position, Quaternion.identity, transform);
        }
    }
}
