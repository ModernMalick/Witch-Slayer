using UnityEngine;

namespace ModernMalick.Audio.AudioSourceEvents
{
    public static class AudioSourceExtensions
    {
        public static IAudioEventSource RequestEventHandlers(this AudioSource audioSource)
        {
            var eventSource = audioSource.GetComponent<AudioEventSource>();

            if (eventSource == null)
            {
                eventSource = audioSource.gameObject.AddComponent<AudioEventSource>();
            }

            return eventSource;
        }
    }
}