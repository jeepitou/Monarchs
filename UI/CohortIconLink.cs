using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CohortIconLink", menuName = "ChessTCG/Misc/CohortIconLink")]
public class CohortIconLink : ScriptableObject
{
    public IconQuantityLink[] cohortIcons;

    private Dictionary<int, Sprite> cohortDictionnary = null;
    
    public Sprite GetCohortIcon(int quantity)
    {
        if (cohortDictionnary == null)
        {
            GenerateDictionary();
        }

        return cohortDictionnary[quantity];
    }
    
    public void GenerateDictionary()
    {
        cohortDictionnary = new Dictionary<int, Sprite>();
        
        foreach (var spriteLinked in cohortIcons)
        {
            cohortDictionnary[spriteLinked.quantity] = spriteLinked.icon;
        }
    }


}

[System.Serializable]
public struct IconQuantityLink
{
    public int quantity;
    public Sprite icon;
}
