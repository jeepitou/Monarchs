using System.Collections;
using System.Collections.Generic;
using TcgEngine;
using UnityEngine;

[CreateAssetMenu(fileName = "MovementPieceTypeLink", menuName = "ChessTCG/Misc/MovementIconLink")]
public class MovementIconLink : ScriptableObject
{
    [SerializeField]
    private MovementLinkedToIcon[] _spriteLinkedToMovement = null;

    private Dictionary<Sprite, PieceType> movementDictionary;
    private Dictionary<PieceType, Sprite> spriteDictionary;
    
    public PieceType GetPieceType(Sprite sprite)
    {
        if (movementDictionary == null)
        {
            GenerateDictionary();
        }
        return movementDictionary[sprite];
    }

    public Sprite GetSprite(PieceType pieceType)
    {
        if (spriteDictionary == null)
        {
            GenerateDictionary();
        }
        return spriteDictionary[pieceType];
    }

    public void GenerateDictionary()
    {
        movementDictionary = new Dictionary<Sprite, PieceType>();
        spriteDictionary = new Dictionary<PieceType, Sprite>();
        
        foreach (var spriteLinked in _spriteLinkedToMovement)
        {
            movementDictionary[spriteLinked.icon] = spriteLinked.pieceType;
            spriteDictionary[spriteLinked.pieceType] = spriteLinked.icon;
        }
    }
    
    [System.Serializable]
    public struct MovementLinkedToIcon
    {
        public PieceType pieceType;
        public Sprite icon;
    } 
}
