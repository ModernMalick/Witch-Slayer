using System;
using ModernMalick.UI.Dynamic;
using UnityEngine;

namespace ModernMalick.Timers
{
    [Serializable]
    public class Cooldown
    {
        [SerializeField] private float duration = 1f;
        [SerializeField] private DynamicProgress progress;

        public event Action<float> OnCooldownChanged = delegate { };
        public event Action OnCooldownFinished = delegate { };

        private float _cooldownProgress;
        public float CooldownProgress
        {
            get => _cooldownProgress;
            private set
            {
                _cooldownProgress = value;
                OnCooldownChanged.Invoke(value);

                if (progress)
                {
                    progress.UpdateProgress(value);
                }
                
                if (value >= 1f) OnCooldownFinished?.Invoke();
            }
        }
        
        private float _elapsed;
        private bool _running;
        
        public bool IsReady()
        {
            return Mathf.Approximately(CooldownProgress, 1f);
        }

        public void Reset()
        {
            _elapsed = 0f;
            CooldownProgress = 0f;
        }
        
        public void Start()
        {
            if (_running) return;
            _running = true;
        }

        public void Tick(float deltaTime)
        {
            if (!_running) return;

            _elapsed += deltaTime;
            CooldownProgress = Mathf.Clamp01(_elapsed / duration);

            if (CooldownProgress >= 1f)
            {
                _running = false;
            }
        }

        public void Refill()
        {
            CooldownProgress = 1f;
        }
    }
}
