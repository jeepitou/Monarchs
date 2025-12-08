using System.Collections;
using System.Collections.Generic;
using Monarchs.Logic;
using TcgEngine;
using UnityEngine;

[CreateAssetMenu(fileName = "MovementPieceTypeLink", menuName = "ChessTCG/Misc/MovementPieceTypeLink")]
public class MovementPieceTypeLink : ScriptableObject
{
    [SerializeField]
    private TypeLinkedToMovementScheme[] _typeLinkedToMovementScheme = null;
    [SerializeField]
    private MovementScheme _monarchMovementScheme;
    
    private Dictionary<PieceType, MovementScheme> movementDictionary;
    private Dictionary<MovementScheme, PieceType> typeDictionary;

    public static MovementPieceTypeLink Get()
    {
        var dataLoader = DataLoader.Get();
        if (dataLoader == null)
        {
            Debug.LogError("DataLoader is not initialized. MovementPieceTypeLink.Get() called before DataLoader.Awake().");
            return null;
        }
        return dataLoader.movementPieceTypeLink;
    }
    
    public MovementScheme GetMovementScheme(PieceType pieceType)
    {
        if (movementDictionary == null)
        {
            GenerateDictionary();
        }
        
        return movementDictionary[pieceType];
    }
    
    public PieceType GetType(MovementScheme movementScheme)
    {
        if (typeDictionary == null)
        {
            GenerateDictionary();
        }
        
        return typeDictionary[movementScheme];
    }
    
    public MovementScheme GetMovementScheme(Card card)
    {
        if (movementDictionary == null)
        {
            GenerateDictionary();
        }
        
        if (card.IsMonarch())
        {
            return _monarchMovementScheme;
        }
        
        return movementDictionary[card.GetPieceType()];
    }
    
    public MovementScheme GetMovementScheme(CardData card)
    {
        if (movementDictionary == null)
        {
            GenerateDictionary();
        }
        
        if (card.GetPieceType() == PieceType.Monarch)
        {
            return _monarchMovementScheme;
        }
        
        return movementDictionary[card.GetPieceType()];
    }

    public PieceType GetType(Card card)
    {
        if (typeDictionary == null)
        {
            GenerateDictionary();
        }

        if (card.IsMonarch())
        {
            return PieceType.Monarch;
        }
        
        return typeDictionary[card.GetCurrentMovementScheme()];
    }

    public void GenerateDictionary()
    {
        movementDictionary = new Dictionary<PieceType, MovementScheme>();
        typeDictionary = new Dictionary<MovementScheme, PieceType>();
        
        foreach (var typeLinked in _typeLinkedToMovementScheme)
        {
            movementDictionary[typeLinked.pieceType] = typeLinked.movementScheme;
            typeDictionary[typeLinked.movementScheme] = typeLinked.pieceType;
        }
    }
    
    [System.Serializable]
    public struct TypeLinkedToMovementScheme
    {
        public MovementScheme movementScheme;
        public PieceType pieceType;
    } 
}
