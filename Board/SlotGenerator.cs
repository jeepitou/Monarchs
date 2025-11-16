using Monarchs.Client;
using TcgEngine.Client;
using UnityEngine;

/// <summary>
/// This class generates all the chessboard tiles.
/// It needs to be a child of the Gameobject with the chessboard model, and placed with a reference to the bottom left corner
/// of the playable squares.
/// It generates the mesh and collider of the individual tiles.
/// </summary>
public class SlotGenerator
{
    private int _tileCountX;
    private int _tileCountY;
    public BoardSlot[,] GenerateAllSlots(int tileCountX, int tileCountY, Transform transform, float tileSize,
        GameObject slotPrefab, bool firstPlayer)
    {
        BoardSlot[,] slots = new BoardSlot[tileCountX, tileCountY];
        _tileCountX = tileCountX;
        _tileCountY = tileCountY;
        for (int x=0; x<tileCountX; x++) 
        {
            for (int y=0; y<tileCountY; y++) 
            {
                if (firstPlayer)
                {
                    slots[x, y] = GenerateSingleSlot(x, y, transform, tileSize, slotPrefab, firstPlayer);
                }
                else
                {
                    slots[_tileCountX-1-x, _tileCountY-1-y] = GenerateSingleSlot(x, y, transform, tileSize, slotPrefab, firstPlayer);
                }
                
            }
        }

        return slots;
    }
    
    private BoardSlot GenerateSingleSlot(int x, int y, Transform transform, float tileSize, 
        GameObject slotPrefab, bool firstPlayer) 
    {
        
        GameObject tileObject = GameObject.Instantiate(slotPrefab);
        
        
        BoardSlot chessTile = tileObject.GetComponent<BoardSlot>();
        if (firstPlayer)
        {
            chessTile.x = x;
            chessTile.y = y;
        }
        else
        {
            chessTile.x = _tileCountX - 1 - x;
            chessTile.y = _tileCountY - 1 - y;
        }
        
        string coordinate = chessTile.x.ToString() + ", " + chessTile.y.ToString();
        tileObject.name = coordinate;
        tileObject.transform.parent = transform;

        float posX = transform.position.x + tileSize / 2 + tileSize * x;
        float posY = transform.position.y;
        float posZ = transform.position.z + tileSize / 2 + tileSize * y;
        Vector3 position = new Vector3(posX, posY, posZ);

        chessTile.transform.position = position;

        return chessTile;
    }
}
