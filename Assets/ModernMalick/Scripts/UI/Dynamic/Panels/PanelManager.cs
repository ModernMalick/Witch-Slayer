using ModernMalick.Core.MonoBehaviourExtensions;

namespace ModernMalick.UI.Dynamic.Panels
{
    public class PanelManager : MonoBehaviourSingleton<PanelManager>
    {
        public void OpenPanel(DynamicPanel panel)
        {
            if(panel.isActiveAndEnabled) return;
            panel.gameObject.SetActive(true);
            panel.Open();
        }

        public void ClosePanel(DynamicPanel panel)
        {
            if(!panel.isActiveAndEnabled) return;
            panel.Close();
        }
    }
}