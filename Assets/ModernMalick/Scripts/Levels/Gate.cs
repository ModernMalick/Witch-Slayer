using ModernMalick.Core.MonoBehaviourExtensions;
using UnityEngine;

namespace ModernMalick.Levels
{
    [RequireComponent(typeof(Animator))]
    public class Gate : MonoBehaviourExtended
    {
        [Component] private Animator _animator;
        [ChildComponent] private Health.Health _leverHealth;

        private void OnEnable()
        {
            _leverHealth.OnHealthDecreased += OpenGate;
        }

        private void OnDisable()
        {
            _leverHealth.OnHealthDecreased -= OpenGate;
        }

        private void OpenGate(int damage)
        {
            _animator.SetTrigger("Open");
        }
    }
}