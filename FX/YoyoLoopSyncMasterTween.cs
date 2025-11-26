using DG.Tweening;

namespace Monarchs.FX
{
    public class YoyoLoopSyncMasterTween
    {
        public Tween masterTween;

        public YoyoLoopSyncMasterTween(float duration)
        {
            masterTween = DOTween.To(() => 0f, x => {}, 1f, duration*2)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);
        }

        public float GetElapsedPercentage()
        {
            if (IsReverse())
            {
                return (masterTween.ElapsedPercentage(false) - 0.5f) / 0.5f;
            }
            else
            {
                return masterTween.ElapsedPercentage(false) / 0.5f;
            }
        }
        
        public bool IsReverse()
        {
            return masterTween.ElapsedPercentage(false) > 0.5f;
        }
    }
}
