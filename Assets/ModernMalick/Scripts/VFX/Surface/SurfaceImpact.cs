using UnityEngine;

namespace ModernMalick.VFX.Surface
{
    [CreateAssetMenu(menuName = "MM/Surface Impact")]
    public class SurfaceImpact : ScriptableObject
    {
        public SurfaceType surface;
        public GameObject effectPrefab;
    }
}