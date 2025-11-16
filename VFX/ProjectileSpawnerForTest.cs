using Sirenix.OdinInspector;
using UnityEngine;

namespace Monarchs.VFX
{
    public class ProjectileSpawnerForTest : MonoBehaviour
    {
        public GameObject projectilePrefab;
        public GameObject startPosition;
        public GameObject targetPosition;

        public float spawnInterval = 5f;
        private bool isSpawning = false;
        private float timer = 0f;
        
        
        [Button("Spawn Projectile")]
        public void SpawnProjectile()
        {
            GameObject projectile = Instantiate(projectilePrefab, startPosition.transform.position, Quaternion.identity);
            projectile.GetComponent<Projectile>().SetTargetPosition(targetPosition.transform.position);
        }
        
        [Button("Spawn Interval Projectile")]
        public void SpawnIntervalProjectile()
        {
            isSpawning = true;
        }
        
        [Button("Stop Interval Projectile")]
        public void StopIntervalProjectile()
        {
            isSpawning = false;
        }
        
        public void Update()
        {
            if (isSpawning)
            {
                timer += Time.deltaTime;
                if (timer >= spawnInterval)
                {
                    timer = 0f;
                    SpawnProjectile();
                }
            }
        }
    }
}