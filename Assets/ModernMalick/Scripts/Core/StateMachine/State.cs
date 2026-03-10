using System;
using ModernMalick.Core.MonoBehaviourExtensions;

namespace ModernMalick.Core.StateMachine
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