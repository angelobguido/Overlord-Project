using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombController : MonoBehaviour
{

    [SerializeField]
    private AudioClip popSnd;
    private AudioSource audioSrc;
    private Rigidbody2D rb;
    private Animator animator;

    private bool canDestroy, hasBeenThrown, hasTimerBeenSet, isExploding;
    public int damage, enemyThatShot;
    [SerializeField]
    protected float bombLifetime;
    protected float bombCountdown;
    // Use this for initialization
    void Awake()
    {
        bombLifetime = 2.0f;
        canDestroy = false;
        hasBeenThrown = false;
        hasTimerBeenSet = false;
        isExploding = false;
        audioSrc = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(hasBeenThrown && CheckIfStopped() && !hasTimerBeenSet)
        {
            bombCountdown = bombLifetime;
            hasTimerBeenSet = true;
        }
        if(hasTimerBeenSet && !isExploding)
        {
            if (bombCountdown >= 0.01f)
                bombCountdown -= Time.deltaTime;
            else
                ExplodeBomb();
        }
        if (!audioSrc.isPlaying && canDestroy)
        {
            //Debug.Log("Stopped playing");
            Destroy(gameObject);
        }
    }

    public void DestroyBomb()
    {
        //Debug.Log("Destroying Bomb");
        canDestroy = true;
    }

    public void SetEnemyThatShot(int _index)
    {
        enemyThatShot = _index;
    }

    public void Shoot(Vector2 facingDirection)
    {
        rb.AddForce(facingDirection, ForceMode2D.Impulse);
        hasBeenThrown = true;
    }

    private bool CheckIfStopped()
    {
        if(rb.velocity.magnitude < 5f)
        {
            return true;
        }
        return false;
    }

    private void ExplodeBomb()
    {
        animator.SetTrigger("Explode");
        audioSrc.PlayOneShot(popSnd, 0.3f);
        isExploding = true;
        Collider2D[] objectsInRange = Physics2D.OverlapCircleAll(new Vector2 (rb.position.x, rb.position.y), 0.45f);
        foreach (Collider2D col in objectsInRange)
        {
            if (col.gameObject.CompareTag("Enemy") || col.gameObject.CompareTag("Player"))
            {
                col.gameObject.GetComponent<HealthController>().ApplyDamage(damage, enemyThatShot);
            }
        }
    }
}
