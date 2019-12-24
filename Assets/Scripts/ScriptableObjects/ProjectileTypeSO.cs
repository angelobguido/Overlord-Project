using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EnemyGenerator
{
    [CreateAssetMenu]
    public class ProjectileTypeSO : ScriptableObject
    {
        public int multiplier;
        public GameObject projectilePrefab;
    }
}