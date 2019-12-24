using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Rendering;
using EnemyGenerator;
using UnityEditor;

public class GameManagerTest : MonoBehaviour
{
    //singleton
    public static GameManagerTest instance = null;

    //The player the enemies will follow
    [SerializeField] private GameObject player = null;

    //Mesh and material if we will create the sprites of the enemies with them.
    //TODO check the video where the CodeMonkey guy uses a spritesheet animation
    //Maybe we will ignore ECS for the gameplay itself as it is overly complicated and we will have few enemyes per room.
    [SerializeField] private Mesh mesh;
    [SerializeField] private Material material;

    //Prefabs of the weapons
    //TODO create them in unity
    [SerializeField] private GameObject swordPrefab;
    [SerializeField] private GameObject bowPrefab;
    [SerializeField] private GameObject shieldPrefab;
    [SerializeField] private GameObject bombPrefab;

    //Prefabs of the projectiles
    //TODO create them in unity
    [SerializeField] private GameObject projectilePrefab;

    //Array of entities for the population of enemies
    public NativeArray<Entity> enemyPopulationArray;
    //Array of entities for the intermediate population of enemies
    public NativeArray<Entity> intermediateEnemyPopulationArray;

    public NativeArray<float> fitnessArray;

    public int generationCounter;

    public float startTime;

    public bool enemyGenerated, enemyPrinted, enemyReady, enemySorted;


    public NativeArray<EnemyComponent> enemyPop;
    public NativeArray<WeaponComponent> weaponPop;
    public int bestIdx;

    public ProjectileTypeRuntimeSetSO projectileSet;
    public MovementTypeRuntimeSetSO movementSet;
    public BehaviorTypeRuntimeSetSO behaviorSet;
    public WeaponTypeRuntimeSetSO weaponSet;
    public int[] projectileMultipliers;
    public float[] movementMultipliers;
    //TODO put them into the EA
    public float[] behaviorMultipliers;
    public float[] weaponMultipliers;
    public bool[] weaponHasProjectile;
    void Awake()
    {
        //Singleton
        if (instance == null)
        {
            instance = this;
            fitnessArray = new NativeArray<float>(EnemyUtil.popSize, Allocator.Persistent);
            enemyPop = new NativeArray<EnemyComponent>(EnemyUtil.popSize, Allocator.Persistent);
            weaponPop = new NativeArray<WeaponComponent>(EnemyUtil.popSize, Allocator.Persistent);
            generationCounter = 0;
            startTime = Time.realtimeSinceStartup;
            enemyGenerated = false;
            enemyPrinted = false;
            enemyReady = false;
            enemySorted = false;
            projectileMultipliers = new int[projectileSet.Items.Count];
            movementMultipliers = new float[movementSet.Items.Count];
            behaviorMultipliers = new float[behaviorSet.Items.Count];
            weaponMultipliers = new float[weaponSet.Items.Count];
            weaponHasProjectile = new bool[weaponSet.Items.Count];
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        
        for (int i = (projectileSet.Items.Count - 1); i >= 0; i--)
            projectileMultipliers[i] = projectileSet.Items[i].multiplier;

        for (int i = (movementSet.Items.Count - 1); i >= 0; i--)
            movementMultipliers[i] = movementSet.Items[i].multiplier;

        for (int i = (behaviorSet.Items.Count - 1); i >= 0; i--)
            behaviorMultipliers[i] = behaviorSet.Items[i].multiplier; 
        
        for (int i = (weaponSet.Items.Count - 1); i >= 0; i--)
            weaponMultipliers[i] = weaponSet.Items[i].multiplier;

        for (int i = (weaponSet.Items.Count - 1); i >= 0; i--)
            weaponHasProjectile[i] = weaponSet.Items[i].hasProjectile;
        //We must have an entity manager in our current world to create and handle the entities
        EntityManager entityManager = World.Active.EntityManager;

        //An entity archetype is kind of a struct of entities
        //This one is for the population of enemies itself, having a "population" tag to differentiate it from the intermediate population
        //Also it has the enemy and its weapon component and, 
        //as we may use this entity for the game (or not, and just handle the gameplay with the monobehaviour stuff), it also has a translation
        EntityArchetype enemyArchetype = entityManager.CreateArchetype(
            typeof(Population),
            typeof(EnemyComponent),
            typeof(Translation),
            typeof(WeaponComponent)
        );

        //The entity archetype for the intermediate population. As the translation is not used in the EA evolution, it does not contain one
        EntityArchetype intermediateEnemyArchetype = entityManager.CreateArchetype(
            typeof(IntermediatePopulation),
            typeof(EnemyComponent),
            typeof(WeaponComponent)
        );

        //Instantiate "Population Size" individuals for both populations using a native array
        enemyPopulationArray = new NativeArray<Entity>(EnemyUtil.popSize, Allocator.Persistent);
        intermediateEnemyPopulationArray = new NativeArray<Entity>(EnemyUtil.popSize, Allocator.Persistent);
        //Create the entities themselves in the memory
        entityManager.CreateEntity(enemyArchetype, enemyPopulationArray);
        entityManager.CreateEntity(intermediateEnemyArchetype, intermediateEnemyPopulationArray);

        //Initialize the initial population of enemies with random values
        for (int i = 0; i < enemyPopulationArray.Length; i++)
        {
            Entity entity = enemyPopulationArray[i];

            entityManager.SetComponentData(entity,
                new WeaponComponent
                {
                    //projectile = (WeaponComponent.ProjectileEnum)UnityEngine.Random.Range(0, (int)WeaponComponent.ProjectileEnum.COUNT),
                    projectile = UnityEngine.Random.Range(0, projectileMultipliers.Length),
                    attackSpeed = UnityEngine.Random.Range(EnemyUtil.minAtkSpeed, EnemyUtil.maxAtkSpeed),
                    projectileSpeed = UnityEngine.Random.Range(EnemyUtil.minProjectileSpeed, EnemyUtil.maxProjectileSpeed)
                }
            );

            entityManager.SetComponentData(entity,
                new EnemyComponent
                {
                    health = UnityEngine.Random.Range(EnemyUtil.minHealth, EnemyUtil.maxHealth),
                    damage = UnityEngine.Random.Range(EnemyUtil.minDamage, EnemyUtil.maxDamage),
                    movementSpeed = UnityEngine.Random.Range(EnemyUtil.minMoveSpeed, EnemyUtil.maxMoveSpeed),
                    activeTime = UnityEngine.Random.Range(EnemyUtil.minActivetime, EnemyUtil.maxActiveTime),
                    restTime = UnityEngine.Random.Range(EnemyUtil.minResttime, EnemyUtil.maxRestTime),
                    weapon = UnityEngine.Random.Range(0, weaponMultipliers.Length),
                    movement = UnityEngine.Random.Range(0, movementMultipliers.Length),
                    behavior = UnityEngine.Random.Range(0, behaviorMultipliers.Length),
                    fitness = Mathf.Infinity
                }
            );
            //We are not using this right now...
            //TODO: decide if we will use the translation from the ECS or the MonoBehavior, that is easier to handle
            entityManager.SetComponentData(entity,
                new Translation
                {
                    Value = new float3(UnityEngine.Random.Range(-8, 8f), UnityEngine.Random.Range(-5, 5f), 0)
                }
            );

            /*entityManager.SetSharedComponentData(entity, new RenderMesh
            {
                mesh = mesh,
                material = material,
            });*/
        }

        //Kill the temporary arrays to free memory
        //enemyPopulationArray.Dispose();
        //intermediateEnemyPopulationArray.Dispose();

    }

    public void Update()
    {
        if(enemyReady & enemySorted)
        {
            if(!enemyPrinted)
            {
                Debug.Log("Fitness: " + enemyPop[bestIdx].fitness);
                Debug.Log("Health: " + enemyPop[bestIdx].health);
                Debug.Log("damage: " + enemyPop[bestIdx].damage);
                Debug.Log("activetime: " + enemyPop[bestIdx].activeTime);
                Debug.Log("movement: " + enemyPop[bestIdx].movement);
                Debug.Log("movementspeed: " + enemyPop[bestIdx].movementSpeed);
                Debug.Log("resttime: " + enemyPop[bestIdx].restTime);
                enemyPrinted = true;
                CreateSOBestEnemies();
            }
        }
    }

    //Returns the current position of the player
    public Vector3 GetPlayerPos()
    {
        if(player != null)
            return player.transform.position;
        return new Vector3(0,0,0);
    }

    protected void OnApplicationQuit()
    {
        enemyPopulationArray.Dispose();
        intermediateEnemyPopulationArray.Dispose();
        fitnessArray.Dispose();
        enemyPop.Dispose();
        weaponPop.Dispose();
    }

    public void CreateSOBestEnemies()
    {
        EnemySO bestEnemy = ScriptableObject.CreateInstance<EnemySO>();
        for (int i = 0; i < EnemyUtil.nBestEnemies; ++i)
        {
            bestEnemy.Init(enemyPop[i].health, enemyPop[i].damage, enemyPop[i].movementSpeed, enemyPop[i].activeTime, enemyPop[i].restTime, enemyPop[i].weapon, enemyPop[i].movement, enemyPop[i].behavior, enemyPop[i].fitness, weaponPop[i].attackSpeed, weaponPop[i].projectileSpeed);
            AssetDatabase.CreateAsset(bestEnemy, "Assets/ScriptableObjectsData/" + "Enemy"+i+".asset");
        }   
    }
}
