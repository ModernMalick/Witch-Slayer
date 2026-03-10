using ModernMalick.Common.Patterns.MonoBehaviourExtensions;

namespace ModernMalick.Common.Patterns.StateMachine
{
    public class StateMachine : MonoBehaviourExtended
    {
        public State CurrentState { get; private set; }

        public void SetState(State newState)
        {
            if (CurrentState == newState || newState == null) return;

            CurrentState?.OnExit();
            CurrentState = newState;
            CurrentState?.OnEnter();
        }

        private void Update()
        {
            CurrentState?.OnUpdate();
        }
        
        private void FixedUpdate()
        {
            CurrentState?.OnFixedUpdate();
        }
    }
}