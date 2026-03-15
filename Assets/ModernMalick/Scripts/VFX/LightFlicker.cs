using UnityEngine;

namespace ModernMalick.VFX
{
    [RequireComponent(typeof(Light))]
    public class LightFlicker : MonoBehaviour
    {
        [Header("Intensity")]
        [SerializeField] private float minIntensity = 0.8f;
        [SerializeField] private float maxIntensity = 1.2f;

        [Header("Timing")]
        [SerializeField] private float flickerSpeed = 8f;
        [SerializeField] private float randomness = 0.5f;

        private Light _light;
        private float _seed;

        private void Awake()
        {
            _light = GetComponent<Light>();
            _seed = Random.value * 100f;
        }

        private void Update()
        {
            var time = Time.time * flickerSpeed + _seed;

            var noise = Mathf.PerlinNoise(time, 0f);
            noise = Mathf.Lerp(noise, Random.value, randomness);

            _light.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
        }
    }
}