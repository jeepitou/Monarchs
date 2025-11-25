using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Monarchs
{
    public class IncorporealBoardEffect 
    {
        private GameObject _addedIncorporealPlane;
        private GameObject _pieceFrame;
        private float _incorporealHoverEffectHeight = 0.05f;
        private float _incorporealHoverEffectDuration = 1f;
        bool _isWhite;
        private Tween _hoverTween;
        private static Tween _masterTween;
        private float _incorporealHoverEffectAngle = 0; // Angle in degrees relative to camera forward (90 = up)
        
        // Start is called before the first frame update

        public void ApplyIncorporealEffect(Transform transform, GameObject imagePlane, GameObject pieceFrame, bool isWhite)
        {
            _isWhite = isWhite;
            Material material = isWhite ? MaterialsData.Get().GetMaterial("incorporeal_white") : MaterialsData.Get().GetMaterial("incorporeal_black");
            pieceFrame.GetComponent<MeshRenderer>().material = material;
            
            _addedIncorporealPlane = GameObject.Instantiate(imagePlane, imagePlane.transform.position, imagePlane.transform.rotation, imagePlane.transform.parent);
            _addedIncorporealPlane.transform.Translate(0, 0, 0.01f);
            _addedIncorporealPlane.GetComponent<MeshRenderer>().material = material;
            InitMasterTween(_incorporealHoverEffectDuration*2);
            StartHoverEffect(transform);
        }
        
        public static void InitMasterTween(float duration)
        {
            if (_masterTween == null || !_masterTween.IsActive())
            {
                float dummyValue = 0f;
                _masterTween = DOTween.To(() => dummyValue, x => dummyValue = x, 1f, duration)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Restart);
            }
        }

        public static float GetMasterElapsedPercentage()
        {
            return _masterTween != null ? _masterTween.ElapsedPercentage(false) : 0f;
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

            float masterProgress = GetMasterElapsedPercentage();
            float hoverDuration = _incorporealHoverEffectDuration;
            float masterDuration = hoverDuration * 2f;
            float localProgress = (masterProgress * masterDuration) % masterDuration;
            bool reverse = localProgress > hoverDuration;
            float tweenProgress = reverse ? (localProgress - hoverDuration) / hoverDuration : localProgress / hoverDuration;

            if (reverse)
            {
                // Swap start and end for reverse direction
                var temp = startPos;
                startPos = endPos;
                endPos = temp;
            }

            _hoverTween = transform.DOMove(endPos, hoverDuration)
                .From(startPos)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
            _hoverTween.Goto(tweenProgress * hoverDuration, true);
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
