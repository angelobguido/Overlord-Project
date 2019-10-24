using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EnemyGenerator
{
    public class WeaponTypeSO : ScriptableObject
    {
        public ProjectileTypeSO projectile;
        public float attackSpeed;
        public float projectileSpeed;
    }
}