using ModernMalick.Core.LeanTween;
using ModernMalick.UI.Dynamic;
using UnityEngine;
using UnityEngine.UI;

namespace ModernMalick.Player.Arsenal.Guns
{
    public class GunUI : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private DynamicText magazine;
        [SerializeField] private GameObject infiniteAmmoIcon;

        [Header("Tween")]
        [SerializeField] private float deselectedScale;
        [SerializeField] private float tweenTime;

        public void SetSelected(bool selected)
        {
            var scale = selected ? Vector3.one : Vector3.one * deselectedScale;
            LeanTween.scale(gameObject, scale, tweenTime);
        }
        
        public void SetIcon(Sprite sprite)
        {
            icon.sprite = sprite;
        }

        public void SetInfiniteAmmo()
        {
            magazine.gameObject.SetActive(false);
            infiniteAmmoIcon.SetActive(true);
        }
        
        public void OnCurrentAmmoChanged(int currentAmmo)
        {
            magazine.UpdateText(currentAmmo);
        }
    }
}