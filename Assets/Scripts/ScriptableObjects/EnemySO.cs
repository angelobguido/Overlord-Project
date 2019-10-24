using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EnemyGenerator
{
    public class EnemySO : ScriptableObject
    {
        public int health;
        public int damage;
        public float movementSpeed;
        public float activeTime;
        public float restTime;
        [SerializeField]
        public WeaponTypeSO weapon;
        [SerializeField]
        public MovementTypeSO movement;
        public float fitness;
    }
}