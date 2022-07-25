using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHP : MonoBehaviour
{
    public GameObject player;
    public GameObject[] HPPoint;
    public Animator Animator;
    float health;
    void Start()
    {
        health = player.GetComponent<CharacterController2D>().m_Health;
        
       
         for (int i = 0; i < health; i++)
        {
            HPPoint[i].SetActive(true);
        }
      
    }

    // Update is called once per frame
    void Update()
    {
        health = player.GetComponent<CharacterController2D>().m_Health;

        
            for (int i = 0; i < HPPoint.Length; i++)
            {   
                //Debug.Log("Health and i" + health + i);
                if(i >= health)
                {
                    HPPoint[i].SetActive(false);
                }
                else
                {
                    HPPoint[i].SetActive(true);
                }
                

       
            }
        
        
        

    }
   
}
