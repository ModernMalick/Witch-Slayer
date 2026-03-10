using System;
using ModernMalick.Common.Patterns.MonoBehaviourExtensions;

namespace ModernMalick.Common.Patterns.StateMachine
{
    public abstract class State : MonoBehaviourExtended
    {
        public Action onStateCompleted = delegate { };
        
        public virtual void OnEnter() { }
        public virtual void OnUpdate() { }
        public virtual void OnFixedUpdate() { }
        public virtual void OnExit() { }
    }
}