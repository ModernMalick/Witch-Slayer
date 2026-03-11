using ModernMalick.Core.LeanTween;
using ModernMalick.Core.MonoBehaviourExtensions;
using UnityEngine;

namespace ModernMalick.UI.Managers
{
    public class UITweener : MonoBehaviourSingleton<UITweener>
    {
        [Header("Tween")] 
        [SerializeField] private float tweenMaxScale;
        [SerializeField] public float tweenTime;
        
        public void ValueChangeTween(GameObject tweenTarget)
        {
            LeanTween.scale(tweenTarget, tweenMaxScale * Vector3.one, tweenTime)
                .setOnComplete(() =>
                {
                    LeanTween.scale(tweenTarget, Vector3.one, tweenTime);
                });
        }
    }
}