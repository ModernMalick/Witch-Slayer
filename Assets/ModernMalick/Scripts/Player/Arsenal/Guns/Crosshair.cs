using ModernMalick.Core.LeanTween;
using UnityEngine;

namespace ModernMalick.Player.Arsenal.Guns
{
    public class Crosshair : MonoBehaviour
    {
        [SerializeField] private Vector2 shotSize;
        [SerializeField] private float shotTime;
        
        private RectTransform _crosshair;
        private Vector2 _defaultCrosshairSize;

        private void Awake()
        {
            _crosshair = GetComponent<RectTransform>();
            _defaultCrosshairSize = _crosshair.sizeDelta;
        }

        public void AnimateShot(float attackRate)
        {
            LeanTween.cancel(gameObject);

            LeanTween.size(_crosshair, shotSize, shotTime)
                .setOnComplete(() =>
                {
                    LeanTween.size(_crosshair, _defaultCrosshairSize, 1 / attackRate - shotTime);
                });
        }
        
        public void AnimateReload(float reloadTime)
        {
            LeanTween.cancel(gameObject);

            LeanTween.rotateZ(gameObject, -179, reloadTime)
                .setOnComplete(() => LeanTween.rotateZ(gameObject, 0, 0));
        }
    }
}