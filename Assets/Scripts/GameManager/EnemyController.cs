using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnemyGenerator;
public class EnemyController : MonoBehaviour
{
    [SerializeField]
    protected float restTime, activeTime, movementSpeed, attackSpeed, projectileSpeed;
    protected int damage;
    [SerializeField]
    protected GameObject playerObj, bloodParticle, weaponPrefab, projectilePrefab, projectileSpawn;
    protected MovementTypeSO movement;
    protected BehaviorType behavior;

    protected Animator anim;
    protected float waitingTime, walkingTime, walkUntil, waitUntil, coolDownTime;
    protected bool isWalking, hasProjectile, hasFixedMoveDir, hasMoveDirBeenChosen, isShooting;
    protected Color originalColor, armsColor, headColor, legsColor;
    protected float lastX, lastY;
    protected Vector3 targetMoveDir;
    protected RoomBHV room;
    [SerializeField]
    protected int indexOnEnemyList;
    protected HealthController healthCtrl;
    protected SpriteRenderer sr;

    private void Awake()
    {
        waitingTime = 0.0f;
        walkingTime = 0.0f;
        isWalking = false;
        isShooting = false;
        waitUntil = 1.5f;
        playerObj = Player.instance.gameObject;
        hasFixedMoveDir = false;
        hasMoveDirBeenChosen = false;
        anim = GetComponent<Animator>();
        sr = gameObject.GetComponent<SpriteRenderer>();
        healthCtrl = gameObject.GetComponent<HealthController>();
        originalColor = sr.color;
    }
    // Use this for initialization
    void Start()
    {
        healthCtrl.SetOriginalColor(originalColor);
        //If the movement needs to be fixed for the whole active time, set the flag here
        if (movement.enemyMovementIndex == MovementEnum.Random)
            hasFixedMoveDir = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isWalking)
        {
            if (walkingTime < walkUntil)
                Walk();
            else
            {
                walkingTime = 0.0f;
                isWalking = false;
                waitUntil = restTime;
                hasMoveDirBeenChosen = false;
            }
        }
        else
        {
            if (waitingTime < waitUntil)
                Wait();
            else
            {
                waitingTime = 0.0f;
                isWalking = true;
                walkUntil = activeTime;
            }
        }
        if (isShooting)
        {
            Shoot();
            isShooting = false;
            coolDownTime = attackSpeed;
        }
        else
        {
            if (coolDownTime > 0.0f)
                WaitShotCoolDown();
            else
            {
                waitingTime = 0.0f;
                isShooting = true;
            }
        }
    }

    void Walk()
    {
        if (!hasMoveDirBeenChosen)
        {
            int xOffset, yOffset;
            //Vector2 target = new Vector2(playerObj.transform.position.x - transform.position.x, playerObj.transform.position.y - transform.position.y);
            targetMoveDir = movement.movementType(playerObj.transform.position, transform.position);
            targetMoveDir.Normalize();

            UpdateAnimation(targetMoveDir);
            if (targetMoveDir.x > 0)
                xOffset = 1;
            else if (targetMoveDir.x < 0)
                xOffset = -1;
            else
                xOffset = 0;
            if (targetMoveDir.y > 0)
                yOffset = 1;
            else if (targetMoveDir.y < 0)
                yOffset = -1;
            else
                yOffset = 0;
            targetMoveDir = new Vector3((targetMoveDir.x + xOffset), (targetMoveDir.y + yOffset), 0f);
            if(!hasFixedMoveDir)
                hasMoveDirBeenChosen = true;
        }
        transform.position += new Vector3(targetMoveDir.x * movementSpeed * Time.deltaTime, targetMoveDir.y*movementSpeed*Time.deltaTime, 0f);
        //transform.position += new Vector3((target.x + xOffset) * movementSpeed * Time.deltaTime, (target.y + yOffset) * movementSpeed * Time.deltaTime, 0f);
        walkingTime += Time.deltaTime;
    }

    void Wait()
    {
        //TODO Scream
        waitingTime += Time.deltaTime;
    }

    void WaitShotCoolDown()
    {
        coolDownTime -= Time.deltaTime;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            collision.gameObject.GetComponent<HealthController>().ApplyDamage(damage, indexOnEnemyList);
        }
    }

    public void CheckDeath()
    {
        if (healthCtrl.GetHealth() <= 0f)
        {
            //TODO Audio and Particles
            //Instantiate(bloodParticle, transform.position, Quaternion.identity);
            room.CheckIfAllEnemiesDead();
            Destroy(gameObject);
        }
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

    public void LoadEnemyData(EnemySO enemyData, int index)
    {
        healthCtrl.SetHealth(enemyData.health);
        damage = enemyData.damage;
        movementSpeed = enemyData.movementSpeed;
        restTime = enemyData.restTime;
        activeTime = enemyData.activeTime;
        attackSpeed = enemyData.attackSpeed;
        projectileSpeed = enemyData.projectileSpeed;
        //projectilePrefab = Instantiate(enemyData.weapon.projectile.projectilePrefab);
        //weaponPrefab = Instantiate(enemyData.weapon.weaponPrefab);
        hasProjectile = enemyData.weapon.hasProjectile;
        /*if (hasProjectile)
            projectilePrefab = enemyData.weapon.projectile.projectilePrefab;
        else*/
            projectilePrefab = null;
        movement = enemyData.movement;
        behavior = enemyData.behavior.enemyBehavior;
        ApplyEnemyColors();
        indexOnEnemyList = index;
    }

    private void ApplyEnemyColors()
    {
        originalColor = new Color(Util.LogNormalization(healthCtrl.GetHealth(), EnemyUtil.minHealth, EnemyUtil.maxHealth, 30, 225)/225f, 0, 1 - Util.LogNormalization(healthCtrl.GetHealth(), EnemyUtil.minHealth, EnemyUtil.maxHealth, 30, 225)/225f);
        armsColor = new Color(Util.LogNormalization(damage, EnemyUtil.minDamage, EnemyUtil.maxDamage, 30, 225)/ 225f, 0, 1 - Util.LogNormalization(damage, EnemyUtil.minDamage, EnemyUtil.maxDamage, 30, 225)/ 225f);
        legsColor = new Color(Util.LogNormalization(movementSpeed, EnemyUtil.minMoveSpeed, EnemyUtil.maxMoveSpeed, 30, 225)/ 225f, 0, 1 - Util.LogNormalization(movementSpeed, EnemyUtil.minMoveSpeed, EnemyUtil.maxMoveSpeed, 30, 225)/ 225f);
        //TODO change head color according to movement
        headColor = originalColor;
        sr.color = originalColor;
        gameObject.transform.Find("EnemyArms").GetComponent<SpriteRenderer>().color = armsColor;
        gameObject.transform.Find("EnemyLegs").GetComponent<SpriteRenderer>().color = legsColor;
        gameObject.transform.Find("EnemyHead").GetComponent<SpriteRenderer>().color = headColor;
    }

    public void SetRoom(RoomBHV _room)
    {
        room = _room;
    }

    protected void Shoot()
    {
        Vector2 target = new Vector2(playerObj.transform.position.x - transform.position.x, playerObj.transform.position.y - transform.position.y);
        target.Normalize();
        target *= projectileSpeed;


        GameObject bullet = Instantiate(projectilePrefab, projectileSpawn.transform.position, projectileSpawn.transform.rotation);
        bullet.GetComponent<Rigidbody2D>().AddForce(target, ForceMode2D.Impulse);
        bullet.GetComponent<ProjectileController>().damage = damage;
        bullet.GetComponent<ProjectileController>().SetEnemyThatShot(indexOnEnemyList);
    }

    //TODO method to shoot a bomb
}
