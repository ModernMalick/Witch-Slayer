using UnityEngine;

namespace ModernMalick.Audio
{
    public static class AudioHelper
    {
        public static void TryPlayAudio(AudioSource audioSource, AudioClip clip)
        {
            if (audioSource == null || clip == null) return;
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}