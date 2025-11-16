using UnityEngine;

namespace Monarchs
{
    public class IncorporealBoardEffect 
    {
        private GameObject _addedIncorporealPlane;
        private GameObject _pieceFrame;
        bool _isWhite;
        // Start is called before the first frame update

        public void ApplyIncorporealEffect(GameObject imagePlane, GameObject pieceFrame, bool isWhite)
        {
            _isWhite = isWhite;
            Material material = isWhite ? MaterialsData.Get().GetMaterial("incorporeal_white") : MaterialsData.Get().GetMaterial("incorporeal_black");
            pieceFrame.GetComponent<MeshRenderer>().material = material;
            
            _addedIncorporealPlane = GameObject.Instantiate(imagePlane, imagePlane.transform.position, imagePlane.transform.rotation, imagePlane.transform.parent);
            _addedIncorporealPlane.transform.Translate(0, 0, 0.01f);
            _addedIncorporealPlane.GetComponent<MeshRenderer>().material = material;
            
        }
        
        public void RemoveIncorporealEffect()
        {
            if (_addedIncorporealPlane != null)
            {
                GameObject.Destroy(_addedIncorporealPlane);
            }
            Material material = _isWhite ? MaterialsData.Get().GetMaterial("white_piece") : MaterialsData.Get().GetMaterial("black_piece");
            _pieceFrame.transform.GetComponent<MeshRenderer>().material = material;
        }
    }
}
