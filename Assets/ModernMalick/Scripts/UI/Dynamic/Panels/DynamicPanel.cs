using ModernMalick.Core.LeanTween;
using ModernMalick.UI.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace ModernMalick.UI.Dynamic.Panels
{
    public class DynamicPanel : MonoBehaviour
    {
        [SerializeField] private Button selectedButton;
        
        public void Open()
        {
            selectedButton.Select();
            transform.localScale = Vector3.zero;
            LeanTween.scale(gameObject, Vector3.one, UITweener.Instance.tweenTime)
                .setIgnoreTimeScale(true);
        }

        public void Close()
        {
            LeanTween.scale(gameObject, Vector3.zero, UITweener.Instance.tweenTime)
                .setIgnoreTimeScale(true)
                .setOnComplete(() => gameObject.SetActive(false));
        }
    }
}