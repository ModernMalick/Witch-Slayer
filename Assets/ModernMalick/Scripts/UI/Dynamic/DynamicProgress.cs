using ModernMalick.Core.LeanTween;
using ModernMalick.Core.MonoBehaviourExtensions;
using ModernMalick.UI.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace ModernMalick.UI.Dynamic
{
    [RequireComponent(typeof(Image))]
    public class DynamicProgress : MonoBehaviourExtended
    {
        [SerializeField] protected float tweenDuration;
        [SerializeField] private bool animateMinMax;

        [Component] private Image _image;

        public void UpdateProgress(float progress)
        {
            LeanTween.cancel(gameObject);
            
            var diff = Mathf.Abs(_image.fillAmount - progress);
            var calculatedDuration = diff * tweenDuration;
            
            LeanTween.value(gameObject, _image.fillAmount, progress, calculatedDuration)
                .setOnUpdate(val => 
                {
                    _image.fillAmount = val;
                })
                .setOnComplete(() => 
                {
                    if (animateMinMax && progress == 0 || progress >= 1)
                    {
                        UITweener.Instance.ValueChangeTween(gameObject);
                    }
                });
        }
    }
}