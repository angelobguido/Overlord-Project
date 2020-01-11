using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{

    [SerializeField]
    private AudioClip popSnd;
    private AudioSource audioSrc;

    private bool canDestroy;
    public int damage;
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
        //audioSrc.PlayOneShot(popSnd, 0.3f);
        canDestroy = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (tag == "EnemyBullet")
        {
            if (collision.gameObject.tag == "Player")
            {
                Debug.Log("Collide with Player");
                collision.gameObject.GetComponent<PlayerController>().ReceiveDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}
