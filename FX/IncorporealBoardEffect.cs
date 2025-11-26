using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Monarchs.FX;
using UnityEngine;

namespace Monarchs
{
    public class IncorporealBoardEffect 
    {
        private GameObject _addedIncorporealPlane;
        private GameObject _pieceFrame;
        private float _incorporealHoverEffectHeight = 0.05f;
        private float _incorporealHoverEffectDuration = 1f;
        private bool _isWhite;
        private Tween _hoverTween;
        private static YoyoLoopSyncMasterTween _masterTween;
        private float _incorporealHoverEffectAngle = 0; // Angle in degrees relative to camera forward (90 = up)

        public void OnDestroy()
        {
            StopHoverEffect();
        }
        
        public void ApplyIncorporealEffect(Transform transform, GameObject imagePlane, GameObject pieceFrame, bool isWhite)
        {
            _isWhite = isWhite;
            Material material = isWhite ? MaterialsData.Get().GetMaterial("incorporeal_white") : MaterialsData.Get().GetMaterial("incorporeal_black");
            pieceFrame.GetComponent<MeshRenderer>().material = material;
            
            _addedIncorporealPlane = GameObject.Instantiate(imagePlane, imagePlane.transform.position, imagePlane.transform.rotation, imagePlane.transform.parent);
            _addedIncorporealPlane.transform.Translate(0, 0, 0.01f);
            _addedIncorporealPlane.GetComponent<MeshRenderer>().material = material;
            _masterTween = _masterTween == null ? new YoyoLoopSyncMasterTween(_incorporealHoverEffectDuration) : _masterTween;
            StartHoverEffect(transform);
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

        public void StartHoverEffect(Transform transform)
        {
            if (_hoverTween != null)
            {
                return;
            }
            Camera cam = Camera.main;
            float rad = Mathf.Deg2Rad * _incorporealHoverEffectAngle;
            Vector3 moveDir = Mathf.Cos(rad) * cam.transform.up + Mathf.Sin(rad) * cam.transform.right;
            moveDir = moveDir.normalized;

            Vector3 startPos = _addedIncorporealPlane.transform.position;
            Vector3 endPos = startPos + moveDir * _incorporealHoverEffectHeight;

            if (_masterTween.IsReverse())
            {
                // Swap start and end for reverse direction
                var temp = startPos;
                startPos = endPos;
                endPos = temp;
            }

            _hoverTween = transform.DOMove(endPos, _incorporealHoverEffectDuration)
                .From(startPos)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
            _hoverTween.Goto(_masterTween.GetElapsedPercentage() * _incorporealHoverEffectDuration, true);
        }

        public void StopHoverEffect()
        {
            if (_hoverTween != null)
            {
                _hoverTween.Kill();
                _hoverTween = null;
            }
        }
    }
}
