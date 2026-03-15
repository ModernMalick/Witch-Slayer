using System.Linq;
using ModernMalick.Core.MonoBehaviourExtensions;
using UnityEngine;

namespace ModernMalick.VFX.Surface
{
    public class SurfaceManager : MonoBehaviourSingleton<SurfaceManager>
    {
        [SerializeField] private SurfaceImpact[] effects;

        public void SpawnImpact(RaycastHit hit)
        {
            if(hit.collider == null) return;
            var surface = hit.collider.GetComponent<Surface>();
            var type = surface ? surface.surfaceType : SurfaceType.Concrete;

            var effect = FindEffect(type);
            
            if (effect == null || effect.effectPrefab == null) return;
            
            Instantiate(effect.effectPrefab, hit.point, Quaternion.LookRotation(-hit.normal));
        }
        
        private SurfaceImpact FindEffect(SurfaceType type)
        {
            return effects.FirstOrDefault(e => e.surface == type);
        }
    }
}