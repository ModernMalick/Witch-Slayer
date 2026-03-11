using UnityEngine;

namespace ModernMalick.Common.UI.Dynamic.Buttons
{
    public class ButtonQuit : AButton
    {
        protected override void OnClick()
        {
            Application.Quit();
        }
    }
}