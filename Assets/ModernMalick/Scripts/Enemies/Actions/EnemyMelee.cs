using UnityEngine;

namespace ModernMalick.Enemies.Actions
{
    [RequireComponent(typeof(EnemyVision))]
    public class EnemyMelee : AEnemyAction
    {
        [SerializeField] private int delta;
        
        private EnemyVision _vision;

        protected override void Awake()
        {
            base.Awake();
            _vision = GetComponent<EnemyVision>();
        }

        protected override void OnActionExecuted()
        {
            if(!_vision.IsTargetInActionRange()) return;
            Health.Health.TryModifyHealth(_vision.Target.gameObject, delta);
        }
    }
}