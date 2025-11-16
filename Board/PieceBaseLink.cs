using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This class is to link each movement scheme with its piece base, to facilitate spawning the correct base under the piece.
/// </summary>
[CreateAssetMenu(fileName = "PieceBaseLink", menuName = "ChessTCG/Misc/PieceBaseLink")]
public class PieceBaseLink : ScriptableSingleton<PieceBaseLink>
{
    [SerializeField]private List<PieceBasePrefabLink> pieceBasePrefabLinks;
    public PieceBasePrefabLink GetPieceBase(PieceType pieceType)
    {
        foreach (var baseLinked in pieceBasePrefabLinks)
        {
            if (baseLinked.pieceType == pieceType)
            {
                return baseLinked;
            }
        }
        Debug.LogError($"Can't find piece base for movementScheme: {pieceType}");
        return pieceBasePrefabLinks[0];
    }
    
    [System.Serializable]
    public struct PieceBasePrefabLink
    {
        public PieceType pieceType;
        public GameObject piecePrefab;
        public Material whiteMaterial;
        public Material blackMaterial;
    } 
}
