using UnityEngine;

namespace ModernMalick.Enemies.Actions
{
    [RequireComponent(typeof(EnemyAction))]
    public abstract class AEnemyAction : MonoBehaviour
    {
        private EnemyAction _enemyAction;

        protected virtual void Awake()
        {
            _enemyAction = GetComponent<EnemyAction>();
        }

        private void OnEnable()
        {
            _enemyAction.OnActionExecuted += OnActionExecuted;
        }

        private void OnDisable()
        {
            _enemyAction.OnActionExecuted -= OnActionExecuted;
        }

        protected abstract void OnActionExecuted();
    }
}