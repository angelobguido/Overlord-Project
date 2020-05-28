using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ProjectileController : MonoBehaviour
{

    [SerializeField]
    private AudioClip popSnd;
    private AudioSource audioSrc;
    private Rigidbody2D rb;

    private bool canDestroy;
    public int damage, enemyThatShot;

    public delegate void HitEnemyEvent();
    public static event HitEnemyEvent hitEnemyEvent;

    // Use this for initialization
    void Awake()
    {
        canDestroy = false;
        audioSrc = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!audioSrc.isPlaying && canDestroy)
        {
            Destroy(gameObject);
        }
    }

    public void DestroyBullet()
    {
        //Debug.Log("Destroying Bullet");
        audioSrc.PlayOneShot(popSnd, 0.15f);
        canDestroy = true;
        GetComponent<Collider2D>().enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;
    }

    protected virtual void OnHit()
    {
        hitEnemyEvent(); //?. é o operador null-conditional
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (CompareTag("EnemyBullet"))
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                collision.gameObject.GetComponent<HealthController>().ApplyDamage(damage, enemyThatShot);
                DestroyBullet();
            }
        }
        else if(CompareTag("Bullet"))
        {
            if(collision.gameObject.CompareTag("Enemy"))
            {
                OnHit();
                collision.gameObject.GetComponent<HealthController>().ApplyDamage(damage);
                DestroyBullet();
            }
            if (collision.gameObject.CompareTag("Shield"))
            {
                DestroyBullet();
            }
        }
        if (collision.gameObject.CompareTag("Block"))
        {
            DestroyBullet();
        }
    }

    public void SetEnemyThatShot(int _index)
    {
        enemyThatShot = _index;
    }

    public void Shoot(Vector2 facingDirection)
    {
        rb.AddForce(facingDirection, ForceMode2D.Impulse);
    }
}
