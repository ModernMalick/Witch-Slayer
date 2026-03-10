using AudioSourceEvents;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ModernMalick.Audio
{
    public class AudioRandomizer : AudioSourceObserver
    {
        [SerializeField, Range(0, 1)] private float pitchChangePercentage = 0.1f;

        private float _defaultPitch;
        
        protected new void Awake()
        {
            base.Awake();
            _defaultPitch = audioSource.pitch;
        }
        
        protected override void OnAudioStart()
        {
            base.OnAudioStart();
            audioSource.pitch = Random.Range(_defaultPitch - pitchChangePercentage, _defaultPitch + pitchChangePercentage);
        }
    }
}