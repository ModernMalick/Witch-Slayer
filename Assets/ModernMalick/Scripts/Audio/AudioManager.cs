using ModernMalick.Core.MonoBehaviourExtensions;
using UnityEngine;

namespace ModernMalick.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviourSingleton<AudioManager>
    {
        public AudioSource AudioSource { get; private set; }

        private new void Awake()
        {
            base.Awake();
            AudioSource = GetComponent<AudioSource>();
        }
        
        public static void TryPlayAudio(AudioSource audioSource, AudioClip clip)
        {
            if (audioSource == null || clip == null) return;
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}