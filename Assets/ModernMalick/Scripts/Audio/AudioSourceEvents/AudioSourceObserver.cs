using System;
using UnityEngine;

namespace ModernMalick.Audio.AudioSourceEvents
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioSourceObserver : MonoBehaviour
    {
        protected AudioSource audioSource;
        private bool _previousState;

        protected void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.ignoreListenerPause = true;

            HandleState();
        }

        private void Update()
        {
            HandleState();
        }

        private void HandleState()
        {
            if (audioSource.isPlaying)
            {
                HandlePlaying();
            }
            else
            {
                HandleStopped();
            }

            _previousState = audioSource.isPlaying;
        }

        private void HandlePlaying()
        {
            if (_previousState)
            {
                return;
            }

            if (IsRandomAudioContainer() || HasNoProgress())
            {
                OnAudioStart();
            }
            else
            {
                OnAudioResume();
            }
        }

        private void HandleStopped()
        {
            if (!_previousState)
            {
                return;
            }
            
            if (IsRandomAudioContainer() || HasPlayedToCompletion())
            {
                OnAudioStopped();
            }
            else
            {
                OnAudioPaused();
            }
        }
        
        private static bool AlmostEqual(float a, float b)
        {
            return Math.Abs(a - b) < float.Epsilon;
        }

        private bool IsRandomAudioContainer()
        {
            return audioSource.clip is null && audioSource.resource is not null;
        }

        private bool HasNoProgress()
        {
            return AlmostEqual(audioSource.time, 0f);
        }
        
        private bool HasPlayedToCompletion()
        {
            return AlmostEqual(audioSource.time, audioSource.clip.length) || HasNoProgress();
        }

        protected virtual void OnAudioStart()
        {
            OnAudioChanged(AudioState.Started);
        }

        protected virtual void OnAudioResume()
        {
            OnAudioChanged(AudioState.Resumed);
        }

        protected virtual void OnAudioPaused()
        {
            OnAudioChanged(AudioState.Paused);
        }

        protected virtual void OnAudioStopped()
        {
            OnAudioChanged(AudioState.Stopped);
        }

        protected virtual void OnAudioChanged(AudioState state)
        {
        }
    }
}