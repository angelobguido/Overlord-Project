using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnemyGenerator;
public class EnemyController : MonoBehaviour
{
    [SerializeField]
    protected float restTime, activeTime, health, movementSpeed, damage, invincibilityTime, attackSpeed, projectileSpeed;
    [SerializeField]
    protected GameObject playerObj, bloodParticle, weaponPrefab, projectilePrefab;
    protected MovementType movement;
    protected BehaviorType behavior;

    protected Animator anim;
    protected float waitingTime, walkingTime, walkUntil, waitUntil, invincibilityCount;
    protected bool isWalking, isInvincible, hasProjectile;
    protected Color originalColor, armsColor, headColor, legsColor;
    protected float lastX, lastY;

    private void Awake()
    {
        waitingTime = 0.0f;
        walkingTime = 0.0f;
        isWalking = false;
        waitUntil = 1.5f;
        playerObj = Player.instance.gameObject;
        isInvincible = false;
        anim = GetComponent<Animator>();
        SpriteRenderer sr = gameObject.GetComponent<SpriteRenderer>();
        originalColor = sr.color;
    }
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (isInvincible)
            if (invincibilityTime < invincibilityCount)
            {
                isInvincible = false;
                gameObject.GetComponent<SpriteRenderer>().color = originalColor;

            }
            else
            {
                invincibilityCount += Time.deltaTime;
            }

        if (isWalking)
        {
            if (walkingTime < walkUntil)
                Walk();
            else
            {
                walkingTime = 0.0f;
                isWalking = false;
                waitUntil = restTime;
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
    }

    void Walk()
    {
        int xOffset, yOffset;
        Vector2 target = new Vector2(playerObj.transform.position.x - transform.position.x, playerObj.transform.position.y - transform.position.y);
        target.Normalize();
        UpdateAnimation(target);
        if (target.x >= 0)
            xOffset = 1;
        else
            xOffset = -1;
        if (target.y >= 0)
            yOffset = 1;
        else
            yOffset = -1;

        transform.position += new Vector3((target.x + xOffset) * movementSpeed * Time.deltaTime, (target.y + yOffset) * movementSpeed * Time.deltaTime, 0f);
        walkingTime += Time.deltaTime;
    }

    void Wait()
    {
        //TODO Scream
        waitingTime += Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Bullet")
        {
            if (!isInvincible)
            {
                gameObject.GetComponent<SpriteRenderer>().color = Color.red;
                health -= collision.GetComponent<ProjectileController>().damage;
                CheckDeath();
                isInvincible = true;
                invincibilityCount = 0f;
                collision.GetComponent<ProjectileController>().DestroyBullet();
            }
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Debug.Log("Collide with Player");
            collision.gameObject.GetComponent<PlayerController>().ReceiveDamage(damage);
        }
    }

    private void CheckDeath()
    {
        if (health <= 0f)
        {
            //TODO Audio and Particles
            Instantiate(bloodParticle, transform.position, Quaternion.identity);
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

    public void LoadEnemyData(EnemySO enemyData)
    {
        health = enemyData.health;
        damage = enemyData.damage;
        movementSpeed = enemyData.movementSpeed;
        restTime = enemyData.restTime;
        activeTime = enemyData.activeTime;
        attackSpeed = enemyData.attackSpeed;
        projectileSpeed = enemyData.projectileSpeed;
        //projectilePrefab = Instantiate(enemyData.weapon.projectile.projectilePrefab);
        weaponPrefab = Instantiate(enemyData.weapon.weaponPrefab);
        hasProjectile = enemyData.weapon.hasProjectile;
        if (hasProjectile)
            projectilePrefab = enemyData.weapon.projectile.projectilePrefab;
        else
            projectilePrefab = null;
        movement = enemyData.movement.enemyMovement;
        behavior = enemyData.behavior.enemyBehavior;
        ApplyEnemyColors();
    }

    private void ApplyEnemyColors()
    {
        originalColor = new Color(Util.LogNormalization(health, EnemyUtil.minHealth, EnemyUtil.maxHealth, 1, 255)/255f, 0, 1 - Util.LogNormalization(health, EnemyUtil.minHealth, EnemyUtil.maxHealth, 1, 255)/255f);
        armsColor = new Color(Util.LogNormalization(damage, EnemyUtil.minDamage, EnemyUtil.maxDamage, 1, 255)/ 255f, 0, 1 - Util.LogNormalization(damage, EnemyUtil.minDamage, EnemyUtil.maxDamage, 1, 255)/ 255f);
        legsColor = new Color(Util.LogNormalization(movementSpeed, EnemyUtil.minMoveSpeed, EnemyUtil.maxMoveSpeed, 1, 255)/ 255f, 0, 1 - Util.LogNormalization(movementSpeed, EnemyUtil.minMoveSpeed, EnemyUtil.maxMoveSpeed, 1, 255)/ 255f);
        //TODO change head color according to movement
        headColor = originalColor;
    }

}
