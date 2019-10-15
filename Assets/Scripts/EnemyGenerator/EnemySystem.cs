using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using EnemyGenerator;
using UnityEngine.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

public class EnemySystem : ComponentSystem
{
    public delegate Vector3 MovementType(Vector3 playerPos, Vector3 enemyPos);
    protected override void OnUpdate()
    {

        /*Entities.ForEach((ref Translation translation, ref EnemyComponent enemyComponent) => {
            Vector3 _playerPos = GameManagerTest.instance.GetPlayerPos();
            MovementType enemyMovement = GetMovementType(enemyComponent.movement);
            translation.Value += (float3)(enemyComponent.movementSpeed * enemyMovement(_playerPos, translation.Value) * Time.deltaTime);
        });*/

    }

    //Returns the type of movement the enemy has
    public MovementType GetMovementType(EnemyComponent.MovementEnum moveTypeEnum)
    {
        switch (moveTypeEnum)
        {
            case EnemyComponent.MovementEnum.None:
                return EnemyMovement.NoMovement;
            case EnemyComponent.MovementEnum.Random:
                return EnemyMovement.MoveRandomly;
            case EnemyComponent.MovementEnum.Flee:
                return EnemyMovement.FleeFromPlayer;
            case EnemyComponent.MovementEnum.Follow:
                return EnemyMovement.FollowPlayer;
            default:
                Debug.Log("No Movement Attached to Enemy");
                return null;
        }
    }

    
}


//Job to calculate the fitness of the whole population
public class EASystem : JobComponentSystem {

    [RequireComponentTag(typeof(Population))]
    [BurstCompile]
    struct CopyPopulationJob : IJobForEachWithEntity<EnemyComponent, WeaponComponent>
    {
        public NativeArray<EnemyComponent> enemyPopulationCopy;
        public NativeArray<WeaponComponent> weaponPopulationCopy;

        public void Execute(Entity entity, int index, [ReadOnly]ref EnemyComponent enemy, [ReadOnly]ref WeaponComponent weapon)
        {
            enemyPopulationCopy[index] = enemy;
            weaponPopulationCopy[index] = weapon;
        }
    }

    [RequireComponentTag(typeof(IntermediatePopulation))]
    [BurstCompile]
    struct CopyIntermediatePopulationJob : IJobForEachWithEntity<EnemyComponent, WeaponComponent>
    {
        public NativeArray<EnemyComponent> enemyPopulationCopy;
        public NativeArray<WeaponComponent> weaponPopulationCopy;
        [ReadOnly] public int popSize;

        public void Execute(Entity entity, int index, [ReadOnly]ref EnemyComponent enemy, [ReadOnly]ref WeaponComponent weapon)
        {
            enemyPopulationCopy[index - popSize] = enemy;
            weaponPopulationCopy[index - popSize] = weapon;
        }
    }

    [RequireComponentTag(typeof(Population))]
    [BurstCompile]
    struct ReplacePopulationJob : IJobForEachWithEntity<EnemyComponent, WeaponComponent>
    {
        public NativeArray<EnemyComponent> enemyPopulationCopy;
        public NativeArray<WeaponComponent> weaponPopulationCopy;

        public void Execute(Entity entity, int index, ref EnemyComponent enemy, ref WeaponComponent weapon)
        {
            enemy = enemyPopulationCopy[index];
            weapon = weaponPopulationCopy[index];
        }
    }

    [RequireComponentTag(typeof(Population))]
    [BurstCompile]
    struct GetFitnessJob : IJobForEachWithEntity<EnemyComponent>
    {
        public NativeArray<float> fitness;

        public void Execute(Entity entity, int index, [ReadOnly]ref EnemyComponent enemy)
        {
            fitness[index] = enemy.fitness;
        }
    }

    //The job that handles the fitness calculatios
    [RequireComponentTag(typeof(Population))]
    [BurstCompile]
    public struct FitnessJob : IJobForEach<EnemyComponent, WeaponComponent>
    {
        public void Execute(ref EnemyComponent enemy, [ReadOnly] ref WeaponComponent weapon)
        {
            float damageMultiplier, movementMultiplier;
            int projectileMultiplier = 0;
            
            //Depending on each weapon, assign a damage multiplier
            switch (enemy.weapon)
            {
                case EnemyComponent.WeaponEnum.None:
                    damageMultiplier = 1.0f;
                    break;
                case EnemyComponent.WeaponEnum.Sword:
                    damageMultiplier = 1.1f;
                    break;
                case EnemyComponent.WeaponEnum.Bow:
                    damageMultiplier = 1.1f;
                    break;
                case EnemyComponent.WeaponEnum.Bomb:
                    damageMultiplier = 1.2f;
                    break;
                case EnemyComponent.WeaponEnum.Shield:
                    damageMultiplier = 1.1f;
                    break;
                default:
                    damageMultiplier = 1.0f;
                    break;
            }

            //Depending on movement type, assign a movement multiplier
            switch (enemy.movement)
            {
                case EnemyComponent.MovementEnum.None:
                    movementMultiplier = 0.0f;
                    break;
                case EnemyComponent.MovementEnum.Random:
                    movementMultiplier = 1.1f;
                    break;
                case EnemyComponent.MovementEnum.Flee:
                    movementMultiplier = 1.2f;
                    break;
                case EnemyComponent.MovementEnum.Follow:
                    movementMultiplier = 1.3f;
                    break;
                default:
                    movementMultiplier = 1.0f;
                    break;
            }

            //If the weapon throws projectiles, assign a projectile multiplier
            if (enemy.weapon != EnemyComponent.WeaponEnum.None)
            {
                switch (weapon.projectile)
                {
                    case WeaponComponent.ProjectileEnum.None:
                        projectileMultiplier = 0;
                        break;
                    case WeaponComponent.ProjectileEnum.Arrow:
                        projectileMultiplier = 1;
                        break;
                    default:
                        projectileMultiplier = 0;
                        break;
                }
            }

            enemy.fitness = enemy.damage * damageMultiplier + enemy.health + enemy.movementSpeed * movementMultiplier + 1 / enemy.restTime + enemy.activeTime + projectileMultiplier * ((1 / weapon.attackSpeed) + weapon.projectileSpeed);
        }
    }

    //The job that handles the fitness calculatios
    [RequireComponentTag(typeof(IntermediatePopulation))]
    [BurstCompile]
    public struct NewPopFitnessJob : IJobForEach<EnemyComponent, WeaponComponent>
    {
        public void Execute(ref EnemyComponent enemy, [ReadOnly] ref WeaponComponent weapon)
        {
            float damageMultiplier, movementMultiplier;
            int projectileMultiplier = 0;

            //Depending on each weapon, assign a damage multiplier
            switch (enemy.weapon)
            {
                case EnemyComponent.WeaponEnum.None:
                    damageMultiplier = 1.0f;
                    break;
                case EnemyComponent.WeaponEnum.Sword:
                    damageMultiplier = 1.1f;
                    break;
                case EnemyComponent.WeaponEnum.Bow:
                    damageMultiplier = 1.1f;
                    break;
                case EnemyComponent.WeaponEnum.Bomb:
                    damageMultiplier = 1.2f;
                    break;
                case EnemyComponent.WeaponEnum.Shield:
                    damageMultiplier = 1.1f;
                    break;
                default:
                    damageMultiplier = 1.0f;
                    break;
            }

            //Depending on movement type, assign a movement multiplier
            switch (enemy.movement)
            {
                case EnemyComponent.MovementEnum.None:
                    movementMultiplier = 0.0f;
                    break;
                case EnemyComponent.MovementEnum.Random:
                    movementMultiplier = 1.1f;
                    break;
                case EnemyComponent.MovementEnum.Flee:
                    movementMultiplier = 1.2f;
                    break;
                case EnemyComponent.MovementEnum.Follow:
                    movementMultiplier = 1.3f;
                    break;
                default:
                    movementMultiplier = 1.0f;
                    break;
            }

            //If the weapon throws projectiles, assign a projectile multiplier
            if (enemy.weapon != EnemyComponent.WeaponEnum.None)
            {
                switch (weapon.projectile)
                {
                    case WeaponComponent.ProjectileEnum.None:
                        projectileMultiplier = 0;
                        break;
                    case WeaponComponent.ProjectileEnum.Arrow:
                        projectileMultiplier = 1;
                        break;
                    default:
                        projectileMultiplier = 0;
                        break;
                }
            }

            enemy.fitness = enemy.damage * damageMultiplier + enemy.health + enemy.movementSpeed * movementMultiplier + 1 / enemy.restTime + enemy.activeTime + projectileMultiplier * ((1 / weapon.attackSpeed) + weapon.projectileSpeed);
        }
    }

    [BurstCompile]
    public struct TournamentJob : IJobForEach<IntermediatePopulation>
    {
        [ReadOnly]
        public NativeArray<float> enemyPopulationFitness;
        public Unity.Mathematics.Random random;


        public void Execute(ref IntermediatePopulation interPop)
        {
            int auxIdx1, auxIdx2;
            auxIdx1 = random.NextInt(0, enemyPopulationFitness.Length);
            auxIdx2 = random.NextInt(0, enemyPopulationFitness.Length);
            if(enemyPopulationFitness[auxIdx1] < enemyPopulationFitness[auxIdx2])
            {
                interPop.parent1 = auxIdx1;
            }
            else
            {
                interPop.parent1 = auxIdx2;
            }
            auxIdx1 = random.NextInt(0, enemyPopulationFitness.Length);
            auxIdx2 = random.NextInt(0, enemyPopulationFitness.Length);
            if (enemyPopulationFitness[auxIdx1] < enemyPopulationFitness[auxIdx2])
            {
                interPop.parent2 = auxIdx1;
            }
            else
            {
                interPop.parent2 = auxIdx1;
            }
            
        }
    }

    [BurstCompile]
    struct CrossoverJob : IJobForEach<EnemyComponent, WeaponComponent, IntermediatePopulation>
    {
        [ReadOnly]
        public NativeArray<EnemyComponent> enemyPopulationCopy;
        [ReadOnly]
        public NativeArray<WeaponComponent> weaponPopulationCopy;
        public Unity.Mathematics.Random random;
        public void Execute(ref EnemyComponent enemy, ref WeaponComponent weapon, [ReadOnly] ref IntermediatePopulation interPop)
        {

            if (random.NextInt(0, 100) < EnemyUtil.crossChance)
            {
                enemy.health = (enemyPopulationCopy[interPop.parent1].health + enemyPopulationCopy[interPop.parent2].health) / 2;
                enemy.damage = (enemyPopulationCopy[interPop.parent1].damage + enemyPopulationCopy[interPop.parent2].damage) / 2;
                enemy.movementSpeed = (enemyPopulationCopy[interPop.parent1].movementSpeed + enemyPopulationCopy[interPop.parent2].movementSpeed) / 2;
                enemy.activeTime = (enemyPopulationCopy[interPop.parent1].activeTime + enemyPopulationCopy[interPop.parent2].activeTime) / 2;
                enemy.restTime = (enemyPopulationCopy[interPop.parent1].restTime + enemyPopulationCopy[interPop.parent2].restTime) / 2;

                int mainParent, secParent;
                if (random.NextBool())
                {
                    mainParent = interPop.parent1;
                    secParent = interPop.parent2;
                }
                else
                {
                    mainParent = interPop.parent2;
                    secParent = interPop.parent1;
                }
                enemy.weapon = enemyPopulationCopy[mainParent].weapon;
                weapon.projectile = weaponPopulationCopy[mainParent].projectile;
                if (enemyPopulationCopy[mainParent].weapon == enemyPopulationCopy[secParent].weapon)
                {
                    weapon.attackSpeed = (weaponPopulationCopy[mainParent].attackSpeed + weaponPopulationCopy[secParent].attackSpeed) / 2;
                    weapon.projectileSpeed = (weaponPopulationCopy[mainParent].projectileSpeed + weaponPopulationCopy[secParent].projectileSpeed) / 2;
                }
                else
                {
                    weapon.attackSpeed = weaponPopulationCopy[mainParent].attackSpeed;
                    weapon.projectileSpeed = weaponPopulationCopy[mainParent].projectileSpeed;
                }

                if (random.NextBool())
                {
                    mainParent = interPop.parent1;
                    secParent = interPop.parent2;
                }
                else
                {
                    mainParent = interPop.parent2;
                    secParent = interPop.parent1;
                }
                enemy.movement = enemyPopulationCopy[mainParent].movement;
                //TODO think about the movement speed averaging according to the movement type
            }
        }
    }

    [BurstCompile]
    struct MutationJob : IJobForEach<EnemyComponent, WeaponComponent, IntermediatePopulation>
    {
        public Unity.Mathematics.Random random;
        public void Execute(ref EnemyComponent enemy, ref WeaponComponent weapon, [ReadOnly] ref IntermediatePopulation interPop)
        {
            if (random.NextInt(0, 100) < EnemyUtil.mutChance)
                weapon.projectile = (WeaponComponent.ProjectileEnum)random.NextInt(0, (int)WeaponComponent.ProjectileEnum.COUNT);
            if (random.NextInt(0, 100) < EnemyUtil.mutChance)
                weapon.attackSpeed = random.NextInt(1, 11);
            if (random.NextInt(0, 100) < EnemyUtil.mutChance)
                weapon.projectileSpeed = random.NextInt(1, 11);
            if (random.NextInt(0, 100) < EnemyUtil.mutChance)
                enemy.health = random.NextInt(1, 11);
            if (random.NextInt(0, 100) < EnemyUtil.mutChance)
                enemy.damage = random.NextInt(1, 11);
            if (random.NextInt(0, 100) < EnemyUtil.mutChance)
                enemy.movementSpeed = random.NextInt(1, 11);
            if (random.NextInt(0, 100) < EnemyUtil.mutChance)
                enemy.activeTime = random.NextInt(1, 11);
            if (random.NextInt(0, 100) < EnemyUtil.mutChance)
                enemy.restTime = random.NextInt(1, 11);
            if (random.NextInt(0, 100) < EnemyUtil.mutChance)
                enemy.weapon = (EnemyComponent.WeaponEnum)random.NextInt(0, (int)EnemyComponent.WeaponEnum.COUNT);
            if (random.NextInt(0, 100) < EnemyUtil.mutChance)
                enemy.movement = (EnemyComponent.MovementEnum)random.NextInt(0, (int)EnemyComponent.MovementEnum.COUNT);
        }
    }

    [BurstCompile]
    struct EmptyJob : IJob
    {
        public void Execute()
        {
        }
    }
    //The EA main loop
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        JobHandle handle;
        if (GameManagerTest.instance.generationCounter < EnemyUtil.maxGenerations)
        {

            FitnessJob fitJob = new FitnessJob
            { };
            //return job.Schedule(this, inputDeps);
            handle = fitJob.Schedule(this, inputDeps);
            handle.Complete();


            GetFitnessJob getFitnessJob = new GetFitnessJob
            {
                fitness = GameManagerTest.instance.fitnessArray
            };

            handle = getFitnessJob.Schedule(this, inputDeps);

            handle.Complete();

            var random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, 100000));

            TournamentJob tournJob = new TournamentJob()
            {
                enemyPopulationFitness = GameManagerTest.instance.fitnessArray,
                random = random
            };

            handle = tournJob.Schedule(this, inputDeps);

            handle.Complete();

            NativeArray<EnemyComponent> enemyPopCopy = new NativeArray<EnemyComponent>(EnemyUtil.popSize, Allocator.Persistent);
            NativeArray<WeaponComponent> weaponPopCopy = new NativeArray<WeaponComponent>(EnemyUtil.popSize, Allocator.Persistent);
            CopyPopulationJob copyPopulation = new CopyPopulationJob
            {
                enemyPopulationCopy = enemyPopCopy,
                weaponPopulationCopy = weaponPopCopy
            };

            handle = copyPopulation.Schedule(this, inputDeps);

            handle.Complete();

            random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, 100000));

            CrossoverJob crossJob = new CrossoverJob
            {
                random = random,
                enemyPopulationCopy = enemyPopCopy,
                weaponPopulationCopy = weaponPopCopy
            };

            handle = crossJob.Schedule(this, inputDeps);

            handle.Complete();

            random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, 100000));

            MutationJob mutJob = new MutationJob
            {
                random = random,
            };

            handle = mutJob.Schedule(this, inputDeps);

            handle.Complete();

            NewPopFitnessJob newfitJob = new NewPopFitnessJob
            { };
            //return job.Schedule(this, inputDeps);
            handle = newfitJob.Schedule(this, inputDeps);

            handle.Complete();

            CopyIntermediatePopulationJob copyIntermediatePopJob = new CopyIntermediatePopulationJob
            {
                enemyPopulationCopy = enemyPopCopy,
                weaponPopulationCopy = weaponPopCopy
            };

            handle = copyIntermediatePopJob.Schedule(this, inputDeps);

            handle.Complete();

            ReplacePopulationJob replacePopulation = new ReplacePopulationJob
            {
                enemyPopulationCopy = enemyPopCopy,
                weaponPopulationCopy = weaponPopCopy
            };

            handle = replacePopulation.Schedule(this, inputDeps);

            GameManagerTest.instance.generationCounter++;
            return handle;
        }
        else if (GameManagerTest.instance.generationCounter == EnemyUtil.maxGenerations)
        {
            Debug.Log(Time.realtimeSinceStartup - GameManagerTest.instance.startTime);
            GameManagerTest.instance.generationCounter++;
        }
        EmptyJob emptyJob = new EmptyJob
        { };
        handle = emptyJob.Schedule();
        return handle;

    }

}