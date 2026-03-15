using UnityEngine;

namespace ModernMalick.Enemies.Actions
{
    public class EnemySpawner : AEnemyAction
    {
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private GameObject spawnObject;

        protected override void OnActionExecuted()
        {
            Instantiate(spawnObject, spawnPoint.position, spawnPoint.rotation);
        }
    }
}