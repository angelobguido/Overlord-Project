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

    private bool canDestroy, hasBeenThrown, hasTimerBeenSet;
    public int damage, enemyThatShot;
    [SerializeField]
    protected float bombLifetime;
    protected float bombCountdown;
    // Use this for initialization
    void Awake()
    {
        bombLifetime = 3.0f;
        canDestroy = false;
        hasBeenThrown = false;
        hasTimerBeenSet = false;
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
        if(hasTimerBeenSet)
        {
            if (bombCountdown >= 0.01f)
                bombCountdown -= Time.deltaTime;
            else
                ExplodeBomb();
        }
        if (!audioSrc.isPlaying && canDestroy)
        {
            Debug.Log("Stopped playing");
            Destroy(gameObject);
        }
    }

    public void DestroyBomb()
    {
        Debug.Log("Destroying Bullet");
        audioSrc.PlayOneShot(popSnd, 0.3f);
        canDestroy = true;
    }

    public void SetEnemyThatShot(int _index)
    {
        enemyThatShot = _index;
    }

    public void ThrowBomb(float throwForce, Vector2 facingDirection)
    {
        rb.AddForce(new Vector2(facingDirection.x * throwForce, facingDirection.y * throwForce));
        hasBeenThrown = true;
    }

    private bool CheckIfStopped()
    {
        if(Mathf.Approximately(rb.velocity.magnitude, 0))
        {
            return true;
        }
        return false;
    }

    private void ExplodeBomb()
    {
        animator.SetTrigger("Explode");
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
