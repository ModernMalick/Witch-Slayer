using ModernMalick.Core.MonoBehaviourExtensions;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ModernMalick.VFX.Surface
{
    [RequireComponent(typeof(DecalProjector))]
    public class DecalRandomizer : MonoBehaviourExtended
    {
        [SerializeField] private Vector2 widthRange = new(0.75f, 1.25f);
        [SerializeField] private Vector2 heightRange = new(0.75f, 1.25f);
        [SerializeField] private Vector2 rotationRange = new(0f, 180);

        [Component] private DecalProjector _projector;
        
        private void Start()
        {
            var width = Random.Range(widthRange.x, widthRange.y);
            var height = Random.Range(heightRange.x, heightRange.y);
            var rotation = Random.Range(rotationRange.x, rotationRange.y);

            
            var scale = _projector.size;
            scale.x = width;
            scale.y = height;
            _projector.size = scale;

            var euler = transform.localEulerAngles;
            euler.z = rotation;
            transform.localEulerAngles = euler;
        }
    }
}