using ModernMalick.Core;
using ModernMalick.Core.Components;
using UnityEngine;

namespace ModernMalick.Enemies.Actions
{
    public class EnemyProjectile : AEnemyAction
    {
        [SerializeField] private Transform projectileOrigin;
        [SerializeField] private Projectile projectile;

        protected override void OnActionExecuted()
        {
            var projectileInstance = Instantiate(projectile, projectileOrigin.position, transform.rotation);
            projectileInstance.Fire();
        }
    }
}