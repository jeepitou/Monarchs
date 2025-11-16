using Monarchs.Client;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Monarchs
{
    public class HistoryTextParser : MonoBehaviour
    {
        public GameObject prefab;

        public void ReplaceWildcardWithPrefab(TextMeshProUGUI text, string wildcard, GameObject prefab)
        {
            var initialString = text.text;
            // hide the wildcard by making it transparent
            var hiddenWildcardString = initialString.Replace(wildcard, $"<color=#fff0>{wildcard}</color>");
            text.text = hiddenWildcardString;
            
            // get the index of the wildcard in the string
            var wildcardStartIndex = initialString.IndexOf(wildcard);
            var wildcardEndIndex = wildcardStartIndex + wildcard.Length - 1;
            // get the positions of the first and last characters of the wildcard
            text.ForceMeshUpdate();
            var bottomLeftPos = text.textInfo.characterInfo[wildcardStartIndex].bottomLeft;
            var topRightPos = text.textInfo.characterInfo[wildcardEndIndex].topRight;
            var center = (bottomLeftPos + topRightPos) / 2f;
            
            // instantiate prefab
            var prefabInstance = Instantiate(prefab, text.transform);
            // set the local position
            prefabInstance.transform.localPosition = center;
        }

        public void ReplaceCardWildcardWithPrefab(TextMeshProUGUI text, GameObject prefab)
        {
            // Find all the instance that starts with {card_ and ends with }
            var initialString = text.text;
            var wildcard = "{card_";
            var wildcardEnd = "}";
            var wildcardStartIndex = initialString.IndexOf(wildcard);
            var wildcardEndIndex = initialString.IndexOf(wildcardEnd);
            int wildcardCount = 0;
            while (wildcardStartIndex != -1 && wildcardEndIndex != -1)
            {
                //Get the card uid
                var cardUid = initialString.Substring(wildcardStartIndex + wildcard.Length, wildcardEndIndex - wildcardStartIndex - wildcard.Length);
                //Remove all char between the wildcard and the end
                initialString = initialString.Remove(wildcardStartIndex+3, wildcardEndIndex - wildcardStartIndex - 2);
 
                text.text = initialString;
                string hiddenColor = "<color=#fff0>";
                string endHiddenColor = "</color>";
                var hiddenWildcardString = initialString.Insert(wildcardStartIndex, $"<color=#fff0>");
                hiddenWildcardString = hiddenWildcardString.Insert(wildcardStartIndex+3+hiddenColor.Length, $"</color>");
                text.text = hiddenWildcardString;
                
                // get the positions of the first and last characters of the wildcard
                text.ForceMeshUpdate();
                var bottomLeftPos = text.textInfo.characterInfo[wildcardStartIndex - (hiddenColor.Length+endHiddenColor.Length)*wildcardCount].bottomLeft;
                var topRightPos = text.textInfo.characterInfo[wildcardStartIndex+2- (hiddenColor.Length+endHiddenColor.Length)*wildcardCount].topRight;
                var center = (bottomLeftPos + topRightPos) / 2f;
                
                // instantiate prefab
                var prefabInstance = Instantiate(prefab, text.transform);
                // set the local position
                prefabInstance.transform.localPosition = center;
                Sprite sprite = GameClient.GetGameData().GetCard(cardUid).CardData.artBoard;
                prefabInstance.GetComponent<Image>().sprite = sprite;
                
                // Find the next wildcard
                initialString = text.text;
                wildcardStartIndex = initialString.IndexOf(wildcard);
                wildcardEndIndex = initialString.IndexOf(wildcardEnd);
                wildcardCount++;
            }
            
        }
    }
}
