using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModernMalick.UI.Dynamic.Buttons
{
    public class ButtonScene : AButton
    {
        [SerializeField] private string sceneName;
        
        protected override void OnClick()
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}