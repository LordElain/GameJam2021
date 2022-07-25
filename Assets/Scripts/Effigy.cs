using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effigy : MonoBehaviour
{
    public int EffigyHealth = 3;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (EffigyHealth <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("PlayerWeapon"))
        {
            EffigyHealth--;
        }
    }
}
