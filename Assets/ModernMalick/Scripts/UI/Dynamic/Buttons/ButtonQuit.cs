using UnityEngine;

namespace ModernMalick.UI.Dynamic.Buttons
{
    public class ButtonQuit : AButton
    {
        protected override void OnClick()
        {
            Application.Quit();
        }
    }
}