using System;
using ModernMalick.Core.StateMachine;
using UnityEngine;

namespace ModernMalick.Enemies.Actions
{
    [RequireComponent(typeof(Animator))]
    public class EnemyAction : State
    {
        [Component] private Animator _animator;
        
        private static readonly int ACTION = Animator.StringToHash("Action");

        public event Action OnActionExecuted = delegate { };
        
        private void OnDisable()
        {
            CancelInvoke();
        }
        
        public override void OnEnter()
        {
            base.OnEnter();
            _animator.SetBool(ACTION, true);
        }

        public void ExecuteAction()
        {
            OnActionExecuted.Invoke();
        }

        public void EndAction()
        {
            onStateCompleted.Invoke();
            _animator.SetBool(ACTION, false);
        }
    }
}