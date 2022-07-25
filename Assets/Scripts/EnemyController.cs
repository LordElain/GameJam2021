using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public GameObject player;
    public GameObject HellEnemy;                                       //Hellversion of Enemy

    public float attackrange = 1.5f;                                    //Distance from player where enemy stops moving and starts attacking
    public float chaserange = 10;                                       //Distance from player where enemy starts chasing player
    public float enemyMovespeed = 1;
    public int EnemyHealth = 3;

    public int enemyDamage = 1;

    public bool isBoss = false;

    
    void Start()
    {
        HellEnemy.SetActive(false);
    }

    void Update()
    {
        if (EnemyHealth <= 0)
        {
            if (HellEnemy)
                HellEnemy.SetActive(true);
            if (isBoss)
                Application.Quit();
            gameObject.SetActive(false);
        }

        if (Vector2.Distance(player.transform.position, transform.position) <= attackrange)
        {
            //Attack
        }
        else if (Vector2.Distance(player.transform.position, transform.position) <= chaserange)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, enemyMovespeed * Time.deltaTime);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject == player)
        {
            player.GetComponent<CharacterController2D>().LoseHealth(enemyDamage);
            AkSoundEngine.PostEvent("Player_Attack_SFX", gameObject);

        }
            
        
        if (collision.gameObject.CompareTag("PlayerWeapon"))
            EnemyHealth--;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("PlayerWeapon"))
            EnemyHealth--;
    }
}
