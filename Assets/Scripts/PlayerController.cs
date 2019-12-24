using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    [SerializeField]
    protected float speed, shootSpeed, shootCD, shootDmg, maxHealth, invincibilityTime;
    [SerializeField]
    protected GameObject bulletPrefab, bulletSpawn;

    Animator anim;
    float lastX, lastY;
    private AudioSource audioSrc;

    [SerializeField]
    private float timeAfterShoot, invincibilityCount, health, rotatedAngle;
    private Vector2 shootForce = new Vector2(0f, 0f);
    private bool isInvincible;
    private Color originalColor;


    public void Awake()
    {
        health = maxHealth;
        anim = GetComponent<Animator>();
        isInvincible = false;
        timeAfterShoot = 0.0f;
        invincibilityCount = 0f;
        SpriteRenderer sr = gameObject.GetComponent<SpriteRenderer>();
        originalColor = sr.color;
    }

    // Use this for initialization
    void Start () {
        anim = GetComponent<Animator>();	
	}
	
	// Update is called once per frame
	void Update () {
        if (isInvincible)
            if (invincibilityTime < invincibilityCount)
            {
                isInvincible = false;
                gameObject.GetComponent<SpriteRenderer>().color = originalColor;
            }
            else
                invincibilityCount += Time.deltaTime;
    }

	void FixedUpdate(){
		float moveHorizontal = Input.GetAxisRaw ("Horizontal");
		float moveVertical = Input.GetAxisRaw ("Vertical");
		Vector2 movement = new Vector2 (moveHorizontal, moveVertical);
		movement.Normalize ();

        float shootHorizontal = Input.GetAxisRaw("HorizontalShoot");
        float shootVertical = Input.GetAxisRaw("VerticalShoot");
        Vector2 shootDir = new Vector2(shootHorizontal, shootVertical);
        shootDir.Normalize();

        Move(movement, shootDir);

        if (shootCD < timeAfterShoot)
        {

            Shoot(shootDir, movement);
            timeAfterShoot = 0f;
        }
        else
            timeAfterShoot += Time.fixedDeltaTime;
    }

    protected void Move(Vector2 movement, Vector2 shoot)
    {
        transform.position += (Vector3)movement * speed;
        UpdateMoveAnimation(movement, shoot);
        /*if(movement != Vector2.zero){
			movement.x *= 100000; //favorece olhar na horizontal (elimina olhar diagonal)
			transform.up = movement;	
		}*/
    }

    protected void Shoot(Vector2 shootDir, Vector2 movementDir)
    {
        bool willShoot = false;
        if (shootDir.x > 0.01f)
        {
            rotatedAngle = 90;
            bulletSpawn.transform.RotateAround(transform.position, Vector3.forward, rotatedAngle);
            shootForce = new Vector2(shootSpeed, 0f);
            willShoot = true;
        }
        else if (shootDir.x < -0.01f)
        {
            rotatedAngle = -90;
            bulletSpawn.transform.RotateAround(transform.position, Vector3.forward, rotatedAngle);
            shootForce = new Vector2(-shootSpeed, 0f);
            willShoot = true;
        }
        else if (shootDir.y > 0.01f)
        {
            rotatedAngle = 180;
            bulletSpawn.transform.RotateAround(transform.position, Vector3.forward, rotatedAngle);
            shootForce = new Vector2(0f, shootSpeed);
            willShoot = true;
        }
        else if (shootDir.y < -0.01f)
        {
            rotatedAngle = 0;
            shootForce = new Vector2(0f, -shootSpeed);
            willShoot = true;
        }
        if (willShoot)
        {
            GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.transform.position, bulletSpawn.transform.rotation);
            bullet.GetComponent<Rigidbody2D>().AddForce(shootForce + movementDir, ForceMode2D.Impulse);
            bullet.GetComponent<ProjectileController>().damage = this.shootDmg;
            bulletSpawn.transform.RotateAround(transform.position, Vector3.forward, -rotatedAngle);
        }
    }

    protected void UpdateMoveAnimation(Vector2 movement, Vector2 shoot)
    {
        Vector2 directionToFace;
        //If it is shooting, the animation direction takes priority over movement
        if (System.Math.Abs(shoot.x) > 0.01f || System.Math.Abs(shoot.y) > 0.01f)
        {
            anim.SetFloat("LastDirX", lastX);
            anim.SetFloat("LastDirY", lastY);
            Debug.Log("Shooting Direction");
            directionToFace = shoot;
        }
        //Else, will face the movement direction
        else
        {
            directionToFace = movement;
        }
        //If not shooting nor moving, maintain the idle direction
        if (directionToFace.x == 0f && directionToFace.y == 0f)
        {
            anim.SetFloat("LastDirX", lastX);
            anim.SetFloat("LastDirY", lastY);
        }
        //Else, update the idle direction
        else
        {
            lastX = directionToFace.x;
            lastY = directionToFace.y;
        }
        //If not moving, sets the animation boolean to false
        if (movement.x == 0f && movement.y == 0f)
        {
            anim.SetBool("IsMoving", false);
        }
        //Else, to true and updates the movement direction
        else
        {

            anim.SetFloat("DirX", directionToFace.x);
            anim.SetFloat("DirY", directionToFace.y);
            anim.SetBool("IsMoving", true);
        }
    }

    public void PlayGetkey()
    {
        audioSrc.PlayOneShot(audioSrc.clip, 1.0f);
    }

    public void ReceiveDamage(float damage)
    {
        if (!isInvincible)
        {
            gameObject.GetComponent<SpriteRenderer>().color = Color.red;
            health -= damage;
            if (health <= 0)
            {
                //TODO KILL
                Time.timeScale = 0f;
                GameManager.instance.GameOver();
                Debug.Log("RIP");
            }
            isInvincible = true;
            invincibilityCount = 0f;
        }
    }

    public void ResetHealth()
    {
        health = maxHealth;
    }
}
