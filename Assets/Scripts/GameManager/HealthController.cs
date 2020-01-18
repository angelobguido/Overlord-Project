using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthController : MonoBehaviour
{

    int health;
    bool isInvincible;
    float invincibilityTime, invincibilityCount;
    Color originalColor;

    private void Awake()
    {
        isInvincible = false;
        invincibilityCount = 0f;
    }
    // Start is called before the first frame update
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
    }

    public void ApplyDamage(int damage, int enemyIndex =-1)
    {
        if (!isInvincible)
        {
            gameObject.GetComponent<SpriteRenderer>().color = Color.red;
            health -= damage;
            isInvincible = true;
            invincibilityCount = 0f;
            if(gameObject.CompareTag("Player"))
            {
                if (enemyIndex > -1)
                    PlayerProfile.instance.OnEnemyDoesDamage(enemyIndex);
                else
                    Debug.LogError("Something's wrong with the enemy index that gave this damage!");
                gameObject.GetComponent<PlayerController>().CheckDeath();
            }
            else if(gameObject.CompareTag("Enemy"))
            {
                gameObject.GetComponent<EnemyController>().CheckDeath();
            }
        }
    }

    public void SetHealth(int _health)
    {
        health = _health;
    }
    public int GetHealth()
    {
        return health;
    }
    public void SetOriginalColor(Color _color)
    {
        originalColor = _color;
    }
}
