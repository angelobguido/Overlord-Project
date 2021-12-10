using Game.EnemyGenerator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Game.Events;
using Game.LevelManager;
using Pathfinding;
using ScriptableObjects;
using UnityEngine;
using Util;
using Random = System.Random;

public class EnemyController : MonoBehaviour
{
    /// This constant holds the weapon prefab name of healers
    public static readonly string HEALER_PREFAB_NAME = "EnemyHealArea";
    private static readonly int HIT_ENEMY = 0;
    private static readonly int ENEMY_DEATH = 1;

    [field: SerializeField]
    public Sprite RandomMovementSprite { get; set; }
    [field: SerializeField]
    public Sprite FleeMovementSprite { get; set; }
    [field: SerializeField]
    public Sprite FollowMovementSprite { get; set; }
    [field: SerializeField]
    public Sprite NoneMovementSprite { get; set; }

    protected bool isActive;
    protected bool canDestroy;
    [SerializeField]
    protected float restTime, activeTime, movementSpeed, attackSpeed, projectileSpeed;
    protected int damage;
    [SerializeField]
    protected GameObject playerObj, weaponPrefab, projectilePrefab, projectileSpawn, weaponSpawn, shieldSpawn;
    [SerializeField]
    protected ParticleSystem bloodParticle;
    [SerializeField]
    protected MovementTypeSO movement;
    protected BehaviorType behavior;
    protected ProjectileTypeSO projectileType;

    protected Animator anim;
    private AudioSource[] audioSrcs;
    [SerializeField]
    protected float walkUntil, waitUntil, coolDownTime;
    protected bool isWalking, hasProjectile, isShooting;
    [SerializeField]
    protected bool hasMoveDirBeenChosen, hasFixedMoveDir, dataHasBeenLoaded;
    protected Color originalColor, armsColor, headColor, legsColor;
    protected float lastX, lastY;
    [SerializeField]
    protected Vector3 targetMoveDir;
    protected RoomBhv room;
    [SerializeField]
    protected int indexOnEnemyList;
    protected HealthController healthCtrl;
    protected SpriteRenderer sr;
    protected Rigidbody2D rb;

    public static event EventHandler playerHitEventHandler;
    public static event EventHandler KillEnemyEventHandler;

    private Seeker _seeker;
    private Path _path;
    private int _currentWaypoint = 0;
    private const float NextWaypointDistance = 3;
    private float _lastRepath = float.NegativeInfinity;
    private const float RepathRate = 0.5f;
    private Vector3 _targetPosition;

    private void Awake()
    {
        isActive = true;
        canDestroy = false;
        dataHasBeenLoaded = false;
        playerObj = Player.Instance.gameObject;
        anim = GetComponent<Animator>();
        audioSrcs = GetComponents<AudioSource>();
        sr = gameObject.GetComponent<SpriteRenderer>();
        healthCtrl = gameObject.GetComponent<HealthController>();
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        PlayerController.PlayerDeathEventHandler += PlayerHasDied;
    }

    private void OnDisable()
    {
        PlayerController.PlayerDeathEventHandler -= PlayerHasDied;
    }

    protected virtual void OnPlayerHit()
    {
        playerHitEventHandler?.Invoke(null, EventArgs.Empty);
    }

    public void ApplyDamageEffects(Vector3 impactDirection)
    {
        if (healthCtrl.GetHealth() > 0 && !audioSrcs[HIT_ENEMY].isPlaying)
        {
            audioSrcs[HIT_ENEMY].PlayOneShot(audioSrcs[HIT_ENEMY].clip, 1.0f);
            var mainParticle= bloodParticle.main;
            mainParticle.startSpeed = 0;
            var forceOverLifetime = bloodParticle.forceOverLifetime;
            forceOverLifetime.enabled = true;
            forceOverLifetime.x = impactDirection.x * 40;
            forceOverLifetime.y = impactDirection.y * 40;
            forceOverLifetime.z = impactDirection.z * 40;
            
            bloodParticle.Play();
        }
    }

    private void PlayerHasDied(object sender, EventArgs eventArgs)
    {
        isActive = false;
    }

    
    private void Start () {
        _seeker = GetComponent<Seeker>();
        SelectMoveDirection();
    }
    
    private void Update()
    {
        if (!audioSrcs[ENEMY_DEATH].isPlaying && canDestroy)
        {
            Destroy(gameObject);
        }
    }
    
    public void OnPathComplete (Path p) {
        Debug.Log("A path was calculated. Did it fail with an error? " + p.error);

        // Path pooling. To avoid unnecessary allocations paths are reference counted.
        // Calling Claim will increase the reference count by 1 and Release will reduce
        // it by one, when it reaches zero the path will be pooled and then it may be used
        // by other scripts. The ABPath.Construct and Seeker.StartPath methods will
        // take a path from the pool if possible. See also the documentation page about path pooling.
        p.Claim(this);
        if (!p.error) {
            _path?.Release(this);
            _path = p;
            // Reset the waypoint counter so that we start to move towards the first point in the path
            _currentWaypoint = 0;
        } else {
            p.Release(this);
        }
    }

    private void FixedUpdate()
    {
        if (!dataHasBeenLoaded || !isActive || canDestroy) return;
        if (isWalking)
        {
            if (walkUntil > 0)
                Walk();
            else
            {
                walkUntil = 0.0f;
                isWalking = false;
                waitUntil = restTime/10;
                hasMoveDirBeenChosen = false;
            }
        }
        else
        {
            if (waitUntil > 0f)
                Wait();
            else
            {
                if (_seeker.IsDone()) {

                    // Start a new path to the targetPosition, call the the OnPathComplete function
                    // when the path has been calculated (which may take a few frames depending on the complexity)
                    var p = _seeker.StartPath(transform.position, _targetPosition, OnPathComplete);
                    p.BlockUntilCalculated();
                }
                if (_path == null) {
                    // We have no path to follow yet, so don't do anything
                    return;
                }
                waitUntil = 0;
                isWalking = true;
                walkUntil = activeTime*2;
            }
        }

        if (!hasProjectile) return;
        if (isShooting)
        {
            Shoot();
            isShooting = false;
            coolDownTime = 1.0f / attackSpeed;
        }
        else
        {
            if (coolDownTime > 0.0f)
                WaitShotCoolDown();
            else
            {
                isShooting = true;
            }
        }
    }

    private void Walk()
    {
        if (!hasMoveDirBeenChosen)
        {
            SelectMoveDirection();
        }
        // Check in a loop if we are close enough to the current waypoint to switch to the next one.
        // We do this in a loop because many waypoints might be close to each other and we may reach
        // several of them in the same frame.
        var reachedEndOfPath = false;
        // The distance to the next waypoint in the path
        float distanceToWaypoint;
        var position = transform.position;

        while (true) {
            // If you want maximum performance you can check the squared distance instead to get rid of a
            // square root calculation. But that is outside the scope of this tutorial.
            distanceToWaypoint = Vector3.Distance(position, _path.vectorPath[_currentWaypoint]);
            if (distanceToWaypoint < NextWaypointDistance) {
                // Check if there is another waypoint or if we have reached the end of the path
                if (_currentWaypoint + 1 < _path.vectorPath.Count) {
                    _currentWaypoint++;
                } else {
                    // Set a status variable to indicate that the agent has reached the end of the path.
                    // You can use this to trigger some special code if your game requires that.
                    reachedEndOfPath = true;
                    break;
                }
            } else {
                break;
            }
        }
        
        // Slow down smoothly upon approaching the end of the path
        // This value will smoothly go from 1 to 0 as the agent approaches the last waypoint in the path.
        //var speedFactor = reachedEndOfPath ? Mathf.Sqrt(distanceToWaypoint/NextWaypointDistance) : 1f;

        // Direction to the next waypoint
        // Normalize it so that it has a length of 1 world unit
        var dir = (_path.vectorPath[_currentWaypoint] - position).normalized;
        // Multiply the direction by our desired speed to get a velocity
        var velocity = dir * movementSpeed*2;
        position += velocity * Time.fixedDeltaTime;
        transform.position = position;
        
        walkUntil -= Time.deltaTime;
    }

    private void SelectMoveDirection()
    {
        int xOffset, yOffset;
        if (movement.movementType == null)
            Debug.LogError("NO MOVEMENT FUNCTION!");
        targetMoveDir = movement.movementType(playerObj.transform.position, gameObject.transform.position);
        targetMoveDir.Normalize();
        //TODO Animate the enemy
        //UpdateAnimation(targetMoveDir);
        targetMoveDir = new Vector3((targetMoveDir.x), (targetMoveDir.y), 0f);
        _targetPosition = transform.position + targetMoveDir * 5;
    }

    private void Wait()
    {
        //TODO Scream
        Debug.Log("Waiting");
        rb.velocity = Vector3.zero;
        waitUntil -= Time.fixedDeltaTime;
    }

    private void WaitShotCoolDown()
    {
        coolDownTime -= Time.fixedDeltaTime;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        var collisionDirection = Vector3.Normalize(gameObject.transform.position - collision.gameObject.transform.position);
        if (!collision.gameObject.CompareTag("Player")) return;
        OnPlayerHit();
        collision.gameObject.GetComponent<HealthController>().ApplyDamage(damage, collisionDirection, indexOnEnemyList);
    }

    public void CheckDeath()
    {
        if (!(healthCtrl.GetHealth() <= 0f)) return;
        audioSrcs[ENEMY_DEATH].PlayOneShot(audioSrcs[ENEMY_DEATH].clip, 1.0f);
        canDestroy = true;
        var childrenSpriteRenderer = GetComponentsInChildren<SpriteRenderer>();
        var childrenCollider = GetComponentsInChildren<Collider2D>();
        GetComponent<Collider2D>().enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;
        foreach (var childSpriteRenderer in childrenSpriteRenderer)
        {
            childSpriteRenderer.enabled = false;
        }
        foreach (var childCollider in childrenCollider)
        {
            childCollider.enabled = false;
        }
        room.CheckIfAllEnemiesDead();
        KillEnemyEventHandler?.Invoke(null, EventArgs.Empty);
    }

    /// Restore the health of this enemy based on the given health amount.
    /// ATTENTION: This method can be called only by a healer enemy.
    public bool Heal(int health)
    {
        // Healers cannot cure other healers
        if (weaponPrefab != null &&
            weaponPrefab.name.Contains(HEALER_PREFAB_NAME))
        {
            return false;
        }
        // Heal this enemy
        return healthCtrl.ApplyHeal(health);
    }

    public float GetAttackSpeed()
    {
        return attackSpeed;
    }

    protected void UpdateAnimation(Vector2 movement)
    {
        if (movement.x == 0f && movement.y == 0f)
        {
            anim.SetFloat("LastDirX", lastX);
            anim.SetFloat("LastDirY", lastY);
            anim.SetBool("IsMoving", false);
        }
        else
        {
            lastX = movement.x;
            lastY = movement.y;
            anim.SetBool("IsMoving", true);
        }
        anim.SetFloat("DirX", movement.x);
        anim.SetFloat("DirY", movement.y);
    }
    /// <summary>
    /// Loads the enemy data.
    /// </summary>
    /// <param name="enemyData">The enemy data.</param>
    /// <param name="index">The index.</param>
    public void LoadEnemyData(EnemySO enemyData)
    {
        healthCtrl.SetHealth(enemyData.health);
        damage = enemyData.damage;
        movementSpeed = enemyData.movementSpeed;
        restTime = enemyData.restTime;
        activeTime = enemyData.activeTime;
        attackSpeed = enemyData.attackSpeed;
        projectileSpeed = enemyData.projectileSpeed * 4;


        if (enemyData.weapon.name == "Shield")
            weaponPrefab = Instantiate(enemyData.weapon.weaponPrefab, shieldSpawn.transform);
        else if (enemyData.weapon.name != "None")
            weaponPrefab = Instantiate(enemyData.weapon.weaponPrefab, weaponSpawn.transform);
        hasProjectile = enemyData.weapon.hasProjectile;
        if (hasProjectile)
        {
            projectilePrefab = enemyData.weapon.projectile.projectilePrefab;
            projectileType = enemyData.weapon.projectile;
        }
        else
            projectilePrefab = null;
        movement = enemyData.movement;
        behavior = enemyData.behavior.enemyBehavior;
        if (enemyData.weapon.hasProjectile || enemyData.weapon.name == "Cure")
        {
            ApplyMageHat();
        }
        else if (enemyData.weapon.name == "None")
        {
            ApplySlimeSprite();
        }
        // ApplyEnemyColors();
        hasMoveDirBeenChosen = false;
        originalColor = sr.color;
        healthCtrl.SetOriginalColor(originalColor);
        if (hasProjectile && projectilePrefab.name == "EnemyBomb")
        {
            attackSpeed /= 2;
        }
        //If the movement needs to be fixed for the whole active time, set the flag here
        if (movement.enemyMovementIndex == Enums.MovementEnum.Random || movement.enemyMovementIndex == Enums.MovementEnum.Random1D || movement.enemyMovementIndex == Enums.MovementEnum.Flee1D || movement.enemyMovementIndex == Enums.MovementEnum.Follow1D)
            hasFixedMoveDir = true;
        else
            hasFixedMoveDir = false;
        isWalking = false;
        isShooting = false;
        waitUntil = 0.5f;
        coolDownTime = 0.5f;
        dataHasBeenLoaded = true;
    }

    private void ApplyMageHat()
    {
        var head = gameObject.transform.Find("EnemyHead").GetComponent<SpriteRenderer>();
        if (head == null) return;
        head.sprite = movement.enemyMovementIndex switch
        {
            Enums.MovementEnum.Random => RandomMovementSprite,
            Enums.MovementEnum.Random1D => RandomMovementSprite,
            Enums.MovementEnum.Flee1D => FleeMovementSprite,
            Enums.MovementEnum.Flee => FleeMovementSprite,
            Enums.MovementEnum.Follow1D => FollowMovementSprite,
            Enums.MovementEnum.Follow => FollowMovementSprite,
            Enums.MovementEnum.None => NoneMovementSprite,
            _ => throw new InvalidEnumArgumentException("Movement Enum does not exist")
        };
    }    
    
    private void ApplySlimeSprite()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = movement.enemyMovementIndex switch
        {
            Enums.MovementEnum.Random => RandomMovementSprite,
            Enums.MovementEnum.Random1D => RandomMovementSprite,
            Enums.MovementEnum.Flee1D => FleeMovementSprite,
            Enums.MovementEnum.Flee => FleeMovementSprite,
            Enums.MovementEnum.Follow1D => FollowMovementSprite,
            Enums.MovementEnum.Follow => FollowMovementSprite,
            Enums.MovementEnum.None => NoneMovementSprite,
            _ => throw new InvalidEnumArgumentException("Movement Enum does not exist")
        };
    }
    
    private void ApplyEnemyColors()
    {

        originalColor = Color.HSVToRGB(0.0f, Constants.LogNormalization(healthCtrl.GetHealth(), EnemyUtil.minHealth, EnemyUtil.maxHealth, 0, 1.0f) / 1.0f, 1.0f);
        //originalColor = new Color(, 0, 1 - Util.LogNormalization(healthCtrl.GetHealth(), EnemyUtil.minHealth, EnemyUtil.maxHealth, 30, 225)/225f);
        armsColor = Color.HSVToRGB(0.0f, Constants.LogNormalization(damage, EnemyUtil.minDamage, EnemyUtil.maxDamage, 0, 1.0f) / 1.0f, 1.0f);
        legsColor = Color.HSVToRGB(0.0f, Constants.LogNormalization(movementSpeed, EnemyUtil.minMoveSpeed, EnemyUtil.maxMoveSpeed, 0, 1.0f) / 1.0f, 1.0f);
        //armsColor = new Color(Util.LogNormalization(damage, EnemyUtil.minDamage, EnemyUtil.maxDamage, 30, 225)/ 225f, 0, 1 - Util.LogNormalization(damage, EnemyUtil.minDamage, EnemyUtil.maxDamage, 30, 225)/ 225f);
        //legsColor = new Color(Util.LogNormalization(movementSpeed, EnemyUtil.minMoveSpeed, EnemyUtil.maxMoveSpeed, 30, 225)/ 225f, 0, 1 - Util.LogNormalization(movementSpeed, EnemyUtil.minMoveSpeed, EnemyUtil.maxMoveSpeed, 30, 225)/ 225f);
        //TODO change head color according to movement
        headColor = originalColor;
        sr.color = originalColor;
        SpriteRenderer arms = gameObject.transform.Find("EnemyArms").GetComponent<SpriteRenderer>();
        if (arms != null)
        {
            arms.color = armsColor;
        }
        SpriteRenderer legs = gameObject.transform.Find("EnemyLegs").GetComponent<SpriteRenderer>();
        if (legs != null)
        {
            legs.color = legsColor;
        }
        SpriteRenderer head = gameObject.transform.Find("EnemyHead").GetComponent<SpriteRenderer>();
        if (head != null)
        {
            head.color = headColor;
        }
    }

    /// <summary>
    /// Sets the room.
    /// </summary>
    /// <param name="_room">The room.</param>
    public void SetRoom(RoomBhv _room)
    {
        room = _room;
    }
    /// <summary>
    /// Shoots this instance.
    /// </summary>
    protected void Shoot()
    {
        Vector2 target = new Vector2(playerObj.transform.position.x - transform.position.x, playerObj.transform.position.y - transform.position.y);
        target.Normalize();
        target *= projectileSpeed;


        GameObject bullet = Instantiate(projectilePrefab, projectileSpawn.transform.position, projectileSpawn.transform.rotation);
        //bullet.GetComponent<Rigidbody2D>().AddForce(target, ForceMode2D.Impulse);
        if (projectilePrefab.name == "EnemyBomb")
        {
            //Debug.Log("It's a bomb");
            bullet.GetComponent<BombController>().damage = damage;
            bullet.GetComponent<BombController>().SetEnemyThatShot(indexOnEnemyList);

        }
        else
        {
            //bullet.GetComponent<ProjectileController>().damage = damage;
            bullet.GetComponent<ProjectileController>().SetEnemyThatShot(indexOnEnemyList);
            bullet.GetComponent<ProjectileController>().ProjectileSO = projectileType;
        }
        bullet.SendMessage("Shoot", target);
    }

    //TODO method to shoot a bomb
}
