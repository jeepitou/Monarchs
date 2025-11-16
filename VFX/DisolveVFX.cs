using DG.Tweening;
using Monarchs.Client;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Monarchs
{
    public class DisolveVFX : AbilityFXMultipleTarget
    {
        [Required] public Material disolveMaterial;
        public float disolveDuration = 1.5f;
        public Color baseColor = Color.red;
        public Ease easeType;
        private Tween _dissolveTween;

        public void Start()
        {
            
            BoardCard boardCard = (BoardCard)GetTargetGameObject().GetComponent<BoardCard>();
            if (boardCard == null)
            {
                return;
            }

            var meshRenderers = boardCard.GetComponentsInChildren<MeshRenderer>();
            boardCard.HideNumbersAndIcons();

            foreach (var mesh in meshRenderers)
            {
                Material initialMaterial = mesh.material;

                // Create instance of the material to avoid modifying the original asset
                var _instanceMaterial = new Material(disolveMaterial);
                
                // Set the initial color/albedo
                Texture texture = initialMaterial.GetTexture("_BaseMap");
                
                _instanceMaterial.SetColor("_BaseColor", initialMaterial.color); // Standard URP/HDRP property
                _instanceMaterial.SetTexture("_Albedo", initialMaterial.GetTexture("_BaseMap")); // Standard URP/HDRP property

                // Set initial dissolve amount to 0
                _instanceMaterial.SetFloat("_DissolveAmount", 0f);
                
                // Apply the material
                mesh.material = _instanceMaterial;
                
                // Create the DOTween animation
                _dissolveTween = DOTween.To(
                    () => 0f,                           // getter
                    x => _instanceMaterial.SetFloat("_DissolveAmount", x), // setter
                    1f,                                 // target value
                    disolveDuration                     // duration
                ).SetEase(easeType)               // optional: add easing
                ;
            }

            
        }

        private void OnDestroy()
        {
            DOTween.Kill(gameObject);
        }
    }
}