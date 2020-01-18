using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{

    [SerializeField]
    private AudioClip popSnd;
    private AudioSource audioSrc;

    private bool canDestroy;
    public int damage, enemyThatShot;
    // Use this for initialization
    void Awake()
    {
        canDestroy = false;
        audioSrc = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!audioSrc.isPlaying && canDestroy)
        {
            Debug.Log("Stopped playing");
            Destroy(gameObject);
        }
    }

    public void DestroyBullet()
    {
        Debug.Log("Destroying Bullet");
        audioSrc.PlayOneShot(popSnd, 0.3f);
        canDestroy = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (CompareTag("EnemyBullet"))
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                collision.gameObject.GetComponent<HealthController>().ApplyDamage(damage, enemyThatShot);
            }
        }
        else if(CompareTag("Bullet"))
        {
            if(collision.gameObject.CompareTag("Enemy"))
            {
                collision.gameObject.GetComponent<HealthController>().ApplyDamage(damage);
            }
        }
        DestroyBullet();
    }

    public void SetEnemyThatShot(int _index)
    {
        enemyThatShot = _index;
    }
}
